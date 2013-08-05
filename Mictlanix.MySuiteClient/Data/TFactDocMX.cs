using System;
using System.Xml;
using System.Xml.Serialization;

namespace Mictlanix.MySuite.Client.Data
{
    public partial class TFactDocMX
    {
        XmlSerializerNamespaces xmlns;

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns
        {
            get
            {
                if (xmlns == null)
                {
                    xmlns = new XmlSerializerNamespaces();
                    xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    xmlns.Add("fx", "http://www.fact.com.mx/schema/fx");
                }
                return xmlns;
            }
            set { xmlns = value; }
        }

        [XmlAttributeAttribute("schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string schemaLocation = "http://www.fact.com.mx/schema/fx http://www.mysuitemex.com/fact/schema/fx_2010_d.xsd";
    }
}
