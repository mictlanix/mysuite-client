using System;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mictlanix.MySuite.Client.Data;

namespace Mictlanix.MySuite.Client
{
    public enum MySuiteCountryCode
    {
        MX
    }

    [Flags]
    public enum MySuiteOutputFormat
    {
        XML,
        PDF,
        HTML
    }

    public class MySuiteClient
    {
        public static string DEVELOPMENT_URL = @"https://www.mysuitetest.com/mx.com.fact.wsFront/FactWSFront.asmx";
        public static string PRODUCTION_URL = @"https://www.mysuitecfdi.com/mx.com.fact.wsFront/FactWSFront.asmx";

        static readonly BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
        {
            MaxBufferPoolSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
            ReaderQuotas = new XmlDictionaryReaderQuotas
            {
                MaxDepth = int.MaxValue,
                MaxStringContentLength = int.MaxValue,
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue,
            }
        };
		
		DocID last_identifier;
		string url_end_point;
		EndpointAddress address;
        string[] output_data = new string[3];

        public MySuiteClient (string requestor, string entity, string loginName, MySuiteCountryCode country)
            : this(requestor, entity, loginName, country, PRODUCTION_URL)
        {
        }

        public MySuiteClient(string requestor, string entity, string loginName, MySuiteCountryCode country, string urlEndPoint)
        {
            Requestor = requestor;
            Entity = entity;
            LoginName = loginName;
            Country = country;
            UrlEndPoint = urlEndPoint;

            OverrideCertificateValidation();

            //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            //binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            //binding.Security.Transport.Realm = string.Empty;
        }

        public string Requestor { get; private set; }
        
        public MySuiteCountryCode Country { get; private set; }
        
        public string Entity { get; private set; }
        
        public string User { get { return Requestor; } }
        
        public string LoginName { get; private set; }

        public string UserName
        {
            get { return string.Format("{0}.{1}.{2}", Country, Entity, LoginName); }
        }
		
		public Object LastIdentifier
		{
			get { return last_identifier; }
		}

        public string UrlEndPoint
        {
            get { return url_end_point;}
            set
            {
                if (url_end_point == value)
                    return;

                url_end_point = value;
                address = new EndpointAddress(url_end_point);
            }
        }

        public MySuiteDocId CreateDocument (TFactDocMX doc, MySuiteOutputFormat format)
        {
            MySuiteWSClient ws;
            TransactionTag result;
            string xml;

            Reset();

            ws = new MySuiteWSClient (binding, address);
            xml = SerializeToXML (doc);
            result = ws.RequestTransaction (Requestor, Transactions.CONVERT_NATIVE_XML.ToString (),
                                            Country.ToString (), Entity, User, UserName,
                                            xml, FormatString (format), "");
            ws.Close ();

            if (result.Response.Result)
                SaveData (result.ResponseData, format);

            return HandleResponse (result.Response);
        }

        public MySuiteDocId GetDocument (string branch, string serial, MySuiteOutputFormat format)
        {
            MySuiteWSClient ws;
            TransactionTag result;

            Reset();

            ws = new MySuiteWSClient (binding, address);
            result = ws.RequestTransaction (Requestor, Transactions.GET_DOCUMENT.ToString(),
                                            Country.ToString(), Entity, User, UserName,
                                            branch, serial, FormatString (format));
            ws.Close();

            if (result.Response.Result)
                SaveData (result.ResponseData, format);

            return HandleResponse (result.Response);
        }

        public MySuiteDocId CancelDocument (string batch, string serial)
        {
            MySuiteWSClient ws;
            TransactionTag result;

            Reset();

            ws = new MySuiteWSClient (binding, address);
            result = ws.RequestTransaction (Requestor, Transactions.CANCEL_DOCUMENT.ToString(),
                                            Country.ToString(), Entity, User, UserName,
                                            batch, serial, "");
            ws.Close();

            return HandleResponse (result.Response);
        }

        public DataSet SearchDocuments (SearchCriteria criteria)
        {
            MySuiteWSClient ws;
            TransactionTag result;
            string xml = SerializeToXML(criteria);

            ws = new MySuiteWSClient(binding, address);
            result = ws.RequestTransaction(Requestor, Transactions.SEARCH_BASIC.ToString(),
                                           Country.ToString(), Entity, User, UserName,
                                           string.Format("<![CDATA[{0}]]>", xml), "", "");

            HandleResponse(result.Response);

            return result.ResponseData.ResponseDataSet;
        }

        MySuiteDocId HandleResponse (FactResponse response)
        {
            MySuiteDocId doc_id;

            if (!response.Result)
            {
                throw new MySuiteClientException (response.Code,
                                                  response.Description,
                                                  response.Hint,
                                                  response.Data);
            }

            last_identifier = response.Identifier;

            doc_id = new MySuiteDocId
            {
                Batch = response.Identifier.Batch,
                Serial = response.Identifier.Serial,
                Date = DateTime.Parse (response.Identifier.IssuedTimeStamp)
            };

            return doc_id;
        }

        public string GetEncodedData(MySuiteOutputFormat format)
        {
            return output_data [(int)format];
        }

        public string GetData (MySuiteOutputFormat format)
        {
            return UTF8Encoding.UTF8.GetString(DecodeFromBase64 (output_data [(int)format]));
        }

        public byte[] GetBytes (MySuiteOutputFormat format)
        {
            return DecodeFromBase64 (output_data [(int)format]);
        }

        public void SaveFile (string filename, MySuiteOutputFormat format)
        {
            if ((format & MySuiteOutputFormat.XML) == MySuiteOutputFormat.XML)
                SaveFileFromBase64 (string.Format("{0}.xml", filename),
                                    output_data [(int)MySuiteOutputFormat.XML]);

            if ((format & MySuiteOutputFormat.HTML) == MySuiteOutputFormat.HTML)
                SaveFileFromBase64 (string.Format("{0}.html", filename),
                                    output_data [(int)MySuiteOutputFormat.HTML]);

            if ((format & MySuiteOutputFormat.PDF) == MySuiteOutputFormat.PDF)
                SaveFileFromBase64 (string.Format("{0}.pdf", filename),
                                    output_data [(int)MySuiteOutputFormat.PDF]);
        }

        void SaveData (FactResponseData data, MySuiteOutputFormat format)
        {
            if ((format & MySuiteOutputFormat.XML) == MySuiteOutputFormat.XML)
                output_data [(int)MySuiteOutputFormat.XML] = data.ResponseData1;

            if ((format & MySuiteOutputFormat.HTML) == MySuiteOutputFormat.HTML)
                output_data [(int)MySuiteOutputFormat.HTML] = data.ResponseData2;

            if ((format & MySuiteOutputFormat.PDF) == MySuiteOutputFormat.PDF)
                output_data [(int)MySuiteOutputFormat.PDF] = data.ResponseData3;
        }

        void Reset ()
        {
            for (int i = 0; i < 3; i++)
                output_data[i] = null;

            last_identifier = null;
        }

        static string FormatString (MySuiteOutputFormat format)
        {
            string fmt = "";

            if ((format & MySuiteOutputFormat.XML) == MySuiteOutputFormat.XML)
                fmt += string.Format("{0} ", MySuiteOutputFormat.XML);

            if ((format & MySuiteOutputFormat.HTML) == MySuiteOutputFormat.HTML)
                fmt += string.Format("{0} ", MySuiteOutputFormat.HTML);

            if ((format & MySuiteOutputFormat.PDF) == MySuiteOutputFormat.PDF)
                fmt += string.Format("{0} ", MySuiteOutputFormat.PDF);

            return fmt;
        }

        static string SerializeToXML<T> (T obj)
        {
            StringBuilder ms;
            XmlSerializer xs;
            XmlWriter xml;

            ms = new StringBuilder();
            xs = new XmlSerializer(typeof(T));
            xml = XmlWriter.Create(ms, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = System.Text.Encoding.UTF8
            });

            xs.Serialize(xml, obj);

            return ms.ToString();
        }

        static void SaveFileFromBase64 (string filename, string source)
        {
            BinaryWriter writer;
            
            writer = new BinaryWriter (new FileStream (filename, FileMode.Create));
            writer.Write (DecodeFromBase64 (source));
            writer.Flush ();
            writer.Close ();
        }

        static public byte[] DecodeFromBase64 (string encodedData)
        {
            return System.Convert.FromBase64String(encodedData);
        }

        static void OverrideCertificateValidation ()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                delegate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error) {
                    return true;
                });
        }
    }
}
