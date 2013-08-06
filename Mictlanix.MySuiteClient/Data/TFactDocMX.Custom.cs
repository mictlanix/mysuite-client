using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Mictlanix.MySuite.Client.Data
{
    public partial class TFactDocMX
	{
		string schema_location;
		XmlSerializerNamespaces xmlns;

		[XmlAttributeAttribute("schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
		public string SchemaLocation {
			get {
				if (schema_location == null) {
					schema_location = "http://www.fact.com.mx/schema/fx http://www.mysuitemex.com/fact/schema/fx_2010_d.xsd";
				}

				return schema_location;
			}
			set { schema_location = value; }
		}

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns {
            get {
                if (xmlns == null) {
                    xmlns = new XmlSerializerNamespaces();
                    xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    xmlns.Add("fx", "http://www.fact.com.mx/schema/fx");
                }
                return xmlns;
            }
            set { xmlns = value; }
        }

		public override string ToString ()
		{
			return ToXmlString ();
		}

		public MemoryStream ToXmlStream ()
		{
			return CFDLib.Utils.SerializeToXmlStream (this, Xmlns);
		}

		public byte[] ToXmlBytes ()
		{
			using (var ms = ToXmlStream ()) {
				return ms.ToArray ();
			}
		}

		public string ToXmlString ()
		{
			using (var ms = ToXmlStream ()) {
				return Encoding.UTF8.GetString (ms.ToArray ());
			}
		}
    }
}
