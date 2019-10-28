using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PostBoy.Helpers
{
    public static class XmlLinqExtensions
    {
        public static IEnumerable<XAttribute> XPathAttributes(this XNode xnode, string xpath, XmlNamespaceManager xml_ns_mgr)
        {
            return
                from XObject? xobject in (IEnumerable)(xnode.XPathEvaluate(xpath, xml_ns_mgr))
                where (xobject != null) && (xobject is XAttribute)
                select (XAttribute)xobject;
        }

        public static IEnumerable<XElement> XPathElements(this XNode xnode, string xpath, XmlNamespaceManager xml_ns_mgr)
        {
            return
                from XObject? xobject in (IEnumerable)(xnode.XPathEvaluate(xpath, xml_ns_mgr))
                where (xobject != null) && (xobject is XElement)
                select (XElement)xobject;
        }

        public static (string value, string? ns) LocalValue(this XAttribute xattribute, IEnumerable<string> prefixes)
        {
            if (xattribute.Value.IndexOf(':') >= 0)
            {
                var split = xattribute.Value.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2)
                {
                    string ns = prefixes.Where(x => x == split[0]).FirstOrDefault();
                    if (!String.IsNullOrEmpty(ns))
                    {
                        return (split[1], ns);
                    }
                }
            }
            return (xattribute.Value, null);
        }
    }
}
