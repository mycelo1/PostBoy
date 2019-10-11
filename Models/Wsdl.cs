using System.Xml.Serialization;
using System.Collections.Generic;

namespace PostBoy.Models
{
    [XmlRoot(ElementName = "definitions", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlRoot
    {
        [XmlElement(ElementName = "types", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public WsdlTypes? Types { get; set; }

        [XmlElement(ElementName = "message", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public List<WsdlMessage>? Messages { get; set; }

        [XmlElement(ElementName = "portType", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public WsdlPortType? PortType { get; set; }

        [XmlElement(ElementName = "binding", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public List<WsdlBinding>? Bindings { get; set; }

        [XmlElement(ElementName = "service", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public WsdlService? Service { get; set; }

        [XmlAttribute(AttributeName = "soap", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string? Soap { get; set; }

        [XmlAttribute(AttributeName = "soap12", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string? Soap12 { get; set; }

        [XmlAttribute(AttributeName = "tns", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string? Tns { get; set; }

        [XmlAttribute(AttributeName = "targetNamespace")]
        public string? TargetNamespace { get; set; }
    }

    [XmlRoot(ElementName = "element", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class WsdlElement
    {
        [XmlAttribute(AttributeName = "minOccurs")]
        public string? MinOccurs { get; set; }

        [XmlAttribute(AttributeName = "maxOccurs")]
        public string? MaxOccurs { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string? Type { get; set; }

        [XmlElement(ElementName = "complexType", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public WsdlComplexType? ComplexType { get; set; }

        [XmlAttribute(AttributeName = "nillable")]
        public string? Nillable { get; set; }
    }

    [XmlRoot(ElementName = "sequence", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class WsdlSequence
    {
        [XmlElement(ElementName = "element", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public List<WsdlElement>? Elements { get; set; }
    }

    [XmlRoot(ElementName = "complexType", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class WsdlComplexType
    {
        [XmlElement(ElementName = "sequence", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public WsdlSequence? Sequence { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }
    }

    [XmlRoot(ElementName = "schema", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class WsdlSchema
    {
        [XmlElement(ElementName = "element", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public List<WsdlElement>? Elements { get; set; }

        [XmlElement(ElementName = "complexType", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public List<WsdlComplexType>? ComplexTypes { get; set; }

        [XmlAttribute(AttributeName = "elementFormDefault")]
        public string? ElementFormDefault { get; set; }

        [XmlAttribute(AttributeName = "targetNamespace")]
        public string? TargetNamespace { get; set; }
    }

    [XmlRoot(ElementName = "types", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlTypes
    {
        [XmlElement(ElementName = "schema", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public WsdlSchema? Schema { get; set; }
    }

    [XmlRoot(ElementName = "part", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlPart
    {
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        [XmlAttribute(AttributeName = "element")]
        public string? Element { get; set; }
    }

    [XmlRoot(ElementName = "message", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlMessage
    {
        [XmlElement(ElementName = "part", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public WsdlPart? Part { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }
    }

    [XmlRoot(ElementName = "input", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlInput
    {
        [XmlAttribute(AttributeName = "message")]
        public string? Message { get; set; }

        [XmlElement(ElementName = "body", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
        public WsdlBodySoap11? BodySoap11 { get; set; }

        [XmlElement(ElementName = "body", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
        public WsdlBodySoap12? BodySoap12 { get; set; }
    }

    [XmlRoot(ElementName = "output", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlOutput
    {
        [XmlAttribute(AttributeName = "message")]
        public string? Message { get; set; }

        [XmlElement(ElementName = "body", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
        public WsdlBodySoap11? BodySoap11 { get; set; }

        [XmlElement(ElementName = "body", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
        public WsdlBodySoap12? BodySoap12 { get; set; }
    }

    [XmlRoot(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlOperation
    {
        [XmlElement(ElementName = "input", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public WsdlInput? Input { get; set; }

        [XmlElement(ElementName = "output", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public WsdlOutput? Output { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        [XmlElement(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
        public WsdlOperationSoap11? OperationSoap11 { get; set; }

        [XmlElement(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
        public WsdlOperationSoap12? OperationSoap12 { get; set; }
    }

    [XmlRoot(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
    public class WsdlOperationSoap11
    {
        [XmlAttribute(AttributeName = "soapAction")]
        public string? SoapAction { get; set; }

        [XmlAttribute(AttributeName = "style")]
        public string? Style { get; set; }
    }

    [XmlRoot(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
    public class WsdlOperationSoap12
    {
        [XmlAttribute(AttributeName = "soapAction")]
        public string? SoapAction { get; set; }

        [XmlAttribute(AttributeName = "style")]
        public string? Style { get; set; }
    }

    [XmlRoot(ElementName = "portType", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlPortType
    {
        [XmlElement(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public List<WsdlOperation>? Operations { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }
    }

    [XmlRoot(ElementName = "binding", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlBinding
    {
        [XmlElement(ElementName = "operation", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public List<WsdlOperation>? Operations { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string? Type { get; set; }

        [XmlElement(ElementName = "binding", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
        public WsdlBindingSoap11? BindingSoap11 { get; set; }

        [XmlElement(ElementName = "binding", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
        public WsdlBindingSoap12? BindingSoap12 { get; set; }
    }

    [XmlRoot(ElementName = "binding", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
    public class WsdlBindingSoap11
    {
        [XmlAttribute(AttributeName = "transport")]
        public string? Transport { get; set; }
    }

    [XmlRoot(ElementName = "binding", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
    public class WsdlBindingSoap12
    {
        [XmlAttribute(AttributeName = "transport")]
        public string? Transport { get; set; }
    }

    [XmlRoot(ElementName = "body", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
    public class WsdlBodySoap11
    {
        [XmlAttribute(AttributeName = "use")]
        public string? Use { get; set; }
    }

    [XmlRoot(ElementName = "body", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
    public class WsdlBodySoap12
    {
        [XmlAttribute(AttributeName = "use")]
        public string? Use { get; set; }
    }

    [XmlRoot(ElementName = "address", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
    public class WsdlAddressSoap11
    {
        [XmlAttribute(AttributeName = "location")]
        public string? Location { get; set; }
    }

    [XmlRoot(ElementName = "address", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
    public class WsdlAddressSoap12
    {
        [XmlAttribute(AttributeName = "location")]
        public string? Location { get; set; }
    }

    [XmlRoot(ElementName = "port", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlPort
    {
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        [XmlAttribute(AttributeName = "binding")]
        public string? Binding { get; set; }

        [XmlElement(ElementName = "address", Namespace = "http://schemas.xmlsoap.org/wsdl/soap/")]
        public WsdlAddressSoap11? Address11 { get; set; }

        [XmlElement(ElementName = "address", Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/")]
        public WsdlAddressSoap12? Address12 { get; set; }
    }

    [XmlRoot(ElementName = "service", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
    public class WsdlService
    {
        [XmlElement(ElementName = "port", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
        public List<WsdlPort>? Port { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }
    }
}
