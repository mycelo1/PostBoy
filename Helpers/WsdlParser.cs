using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using PostBoy.Models;

namespace PostBoy.Helpers
{
    public static class WsdlParser
    {
        public static string[] Parse(string wsdl_text, ref WsdlRoot? wsdl_obj)
        {
            List<String> result = new List<string>();
            XmlSerializer xml_serializer = new XmlSerializer(typeof(WsdlRoot));
            using StringReader reader = new StringReader(wsdl_text);
            wsdl_obj = (WsdlRoot)xml_serializer.Deserialize(reader);
            if (wsdl_obj?.Bindings != null)
            {
                foreach (var binding in wsdl_obj.Bindings)
                {
                    var operations = binding.Operations;
                    if ((operations != null) && (operations.Count > 0))
                    {
                        foreach (var operation in operations)
                        {
                            if (operation.Name != null)
                            {
                                result.Add(operation.Name);
                            }
                        }
                        break;
                    }
                }
            }
            return result.ToArray();
        }

        //<?xml version="1.0" encoding="utf-8"?>
        //<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
        //  <soap:Body>
        //    <ConsultarBoletos xmlns="http://tempuri.org/">
        //      <Cpf>string</Cpf>
        //    </ConsultarBoletos>
        //  </soap:Body>
        //</soap:Envelope>
        public static (string body, string soap_action) Build(WsdlRoot wsdl_obj, string operation, string charset)
        {
            var ResultBody = new StringBuilder();
            ResultBody.Append($"<?xml version=\"1.0\" encoding=\"{charset}\"?>");
            ResultBody.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            ResultBody.Append("<soap:Body>");
            ResultBody.Append($"<{operation}> xmlns=\"{wsdl_obj.TargetNamespace}\"");

            /*
            MESSAGE = portType/operation[].(name=OPERATION)/input.message
            ELEMENT = message[].(name=MESSAGE)/(part.(name="body")).element
            FOREACH types.schema.element[].(name=ELEMENT)/complexType/sequence/element[]
                NAME = .name
                TYPE = .type
                FOREACH types.schema.complexType[].(name=TYPE)/sequence/element[]
                    NAME = .name
                    TYPE = .type
                    FOREACH types.schema.complexType[].(name=TYPE)/sequence/element[]
                        NAME = .name
                        TYPE = .type
            */

            ResultBody.Append($"</{operation}>");
            ResultBody.Append("</soap:Body>");
            ResultBody.Append("</soap:Envelope>");

            return (ResultBody.ToString(), wsdl_obj.TargetNamespace + operation);

            string RemoveTNS(string value)
            {
                if (value.IndexOf(':') >= 0)
                {
                    return value.Split(':')[1];
                }
                else
                {
                    return value;
                }
            }

            /*
            WsdlMessage? FindMessage(string operation)
            {
                if (wsdl_obj?.Messages?.Count > 0)
                {
                    foreach (var message in wsdl_obj.Messages)
                    {
                        if (element?.Name == RemoveTNS(element_name))
                        {
                            return element;
                        }
                    }
                }
                return null;
            }
            */

            WsdlElement? FindElement(string element_name)
            {
                if (wsdl_obj?.Types?.Schema?.Elements?.Count > 0)
                {
                    foreach (var element in wsdl_obj.Types.Schema.Elements)
                    {
                        if (element?.Name == RemoveTNS(element_name))
                        {
                            return element;
                        }
                    }
                }
                return null;
            }

            WsdlComplexType? FindType(string type_name)
            {
                if (wsdl_obj?.Types?.Schema?.ComplexTypes?.Count > 0)
                {
                    foreach (var complex_type in wsdl_obj.Types.Schema.ComplexTypes)
                    {
                        if (complex_type?.Name == RemoveTNS(type_name))
                        {
                            return complex_type;
                        }
                    }
                }
                return null;
            }
        }
    }
}