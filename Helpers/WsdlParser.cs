using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Xml.XMLGen;

namespace PostBoy.Helpers
{
    public class WsdlParser
    {
        private const string NS_WSDL = "http://schemas.xmlsoap.org/wsdl/";
        private const string NS_SOAP = "http://schemas.xmlsoap.org/wsdl/soap/";
        private const string NS_XSD = "http://www.w3.org/2001/XMLSchema";
        private const string NS_XSI = "http://www.w3.org/2001/XMLSchema-instance";
        private const string NS_ENV = "http://schemas.xmlsoap.org/soap/envelope/";

        protected string targetNamespace { get; }
        protected XElement wsdl_object { get; }
        protected XmlNamespaceManager xml_ns_mgr { get; }
        protected Dictionary<string, string> namespaces { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> operations { get; } = new Dictionary<string, string>();

        public WsdlParser(string wsdl_text)
        {
            using StringReader reader = new StringReader(wsdl_text);
            wsdl_object = XElement.Parse(wsdl_text);
            targetNamespace = wsdl_object.Attribute("targetNamespace").Value;
            xml_ns_mgr = new XmlNamespaceManager(new NameTable());
            xml_ns_mgr.AddNamespace("wsdl", NS_WSDL);
            xml_ns_mgr.AddNamespace("soap", NS_SOAP);
            xml_ns_mgr.AddNamespace("xsd", NS_XSD);
            xml_ns_mgr.AddNamespace("tns", targetNamespace);
            foreach (var attribute in
                from attribute in wsdl_object.Attributes()
                where attribute.IsNamespaceDeclaration && (!String.IsNullOrWhiteSpace(attribute.Name.NamespaceName))
                select attribute)
            {
                namespaces.Add(attribute.Name.LocalName, attribute.Name.NamespaceName);
            }
            foreach (var (xelement, soapAction) in
                from XElement xelement in wsdl_object.XPathElements("/wsdl:binding[1]/wsdl:operation", xml_ns_mgr)
                let xpath = xelement.XPathElements("./soap:operation[1]", xml_ns_mgr)
                let soapAction = xpath.FirstOrDefault().Attribute("soapAction").Value
                select (xelement, soapAction))
            {
                operations.Add(xelement.Attribute("name").LocalValue(namespaces.Keys).value, soapAction);
            }
        }

        public string BuildRequest(string operation)
        {
            // load operation's input message
            // MESSAGE = portType/operation[].(name=OPERATION)/input.message
            var operations = wsdl_object.XPathElements($"/wsdl:portType[1]/wsdl:operation[@name='{operation}']/wsdl:input[1]", xml_ns_mgr);
            var input_message = operations.FirstOrDefault().Attribute("message").LocalValue(namespaces.Keys).value;

            // load message's main element
            // ELEMENT = message[].(name=MESSAGE)/(part.(name="body")).element
            var messages = wsdl_object.XPathElements($"/wsdl:message[@name='{input_message}']/wsdl:part[@element]", xml_ns_mgr);
            var (xsd_element, ns_prefix) = messages.FirstOrDefault().Attribute("element").LocalValue(namespaces.Keys);

            using var stream_xmldom = new MemoryStream();
            using var stream_schema = new MemoryStream();
            using var stream_sample = new MemoryStream();
            using var stream_xmlout = new MemoryStream();
            using var stream_reader = new StreamReader(stream_xmlout, true);
            using var xml_writer = XmlWriter.Create(stream_sample);

            // extract XSD from WSDL
            var xelement_schema = wsdl_object.XPathElements("/wsdl:types/xsd:schema[1]", xml_ns_mgr).FirstOrDefault();

            // remove all <element ref="schema"> (causes problems in sample generation)
            var nodes = xelement_schema.XPathElements("//xsd:element[@ref]", xml_ns_mgr).ToList();
            foreach (var element in
                from XElement element in nodes
                where (element.Parent != null) && (element.Attribute("ref").LocalValue(namespaces.Keys).value == "schema")
                select element)
            {
                element.Remove();
            }

            // add targetNamespace
            xelement_schema.Save(stream_xmldom, SaveOptions.DisableFormatting);
            var xml_dom_schema = new XmlDocument();
            stream_xmldom.Position = 0;
            xml_dom_schema.Load(stream_xmldom);
            xml_dom_schema.DocumentElement.SetAttribute($"xmlns:{ns_prefix}", targetNamespace);
            xml_dom_schema.Save(stream_schema);

            // generate sample XML from XSD
            stream_schema.Position = 0;
            var xml_schema = XmlSchema.Read(stream_schema, (s, v) => { throw new XmlSchemaException(v.Message); });
            var generator = new XmlSampleGenerator(xml_schema, new XmlQualifiedName(xsd_element, targetNamespace));
            generator.WriteXml(xml_writer);
            xml_writer.Flush();

            // load XML
            var xml_dom_inner = new XmlDocument();
            stream_sample.Position = 0;
            xml_dom_inner.Load(stream_sample);

            // create SOAP envelope
            var xml_dom_outer = new XmlDocument() { PreserveWhitespace = false };
            var elm_envelope = xml_dom_outer.CreateElement("soap", "Envelope", NS_ENV);
            var elm_body = xml_dom_outer.CreateElement("soap", "Body", NS_ENV);
            var elm_operation = xml_dom_outer.CreateElement(operation, targetNamespace);
            foreach (var node in
                from XmlNode? node in xml_dom_inner.DocumentElement.ChildNodes
                where node!.NodeType == XmlNodeType.Element
                select node)
            {
                elm_operation.AppendChild(xml_dom_outer.ImportNode(node, true));
            }
            elm_body.AppendChild(elm_operation);
            elm_envelope.AppendChild(elm_body);
            xml_dom_outer.AppendChild(elm_envelope);
            xml_dom_outer.DocumentElement.SetAttribute("xmlns:xsi", NS_XSI);
            xml_dom_outer.DocumentElement.SetAttribute("xmlns:xsd", NS_XSD);
            xml_dom_outer.Save(stream_xmlout);

            // output SOAP
            stream_xmlout.Position = 0;
            var xml_string = stream_reader.ReadToEnd();

            return xml_string;
        }
    }
}