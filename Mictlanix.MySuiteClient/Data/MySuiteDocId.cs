using System;
using System.Xml.Serialization;

namespace Mictlanix.MySuite.Client.Data
{
    public class MySuiteDocId
    {
        string batch;
        string serial;
        DateTime? date;

        [XmlElementAttribute()]
        public string Batch
        {
            get
            {
                return this.batch;
            }
            set
            {
                this.batch = value;
            }
        }

        [XmlElementAttribute()]
        public string Serial
        {
            get
            {
                return this.serial;
            }
            set
            {
                this.serial = value;
            }
        }

        [XmlElementAttribute()]
        public DateTime? Date
        {
            get
            {
                return this.date;
            }
            set
            {
                this.date = value;
            }
        }
    }
}
