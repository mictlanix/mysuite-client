using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Mictlanix.MySuite.Client
{
    public class MySuiteClientException : System.Exception
    {
        public MySuiteClientException ()
        {
        }

        public MySuiteClientException (string message)
            : this(0, message)
        {
        }

        public MySuiteClientException (int code, string message)
            : this(code, message, null, null)
        {
        }

        public MySuiteClientException (int code, string message, string hint, string info)
            : base(message)
        {
            Code = code;
            Hint = hint;
            Info = info;
        }

        public int Code { get; private set; }

        public string Hint { get; private set; }

        public string Info { get; private set; }
    }
}
