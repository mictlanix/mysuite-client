using System;
using System.Xml.Serialization;

namespace Mictlanix.MySuite.Client.Data
{
    [Flags]
    [Serializable()]
    [XmlTypeAttribute(Namespace = "")]
    public enum SearchCriteriaDocumentKind
    {
        [XmlEnum("1")]
        Invoice = 1,
        [XmlEnum("2")]
        CreditNote = 2,
        [XmlEnum("4")]
        DebitNote = 4
    }

    [Serializable()]
    [XmlTypeAttribute(Namespace = "")]
    public enum SearchCriteriaCountryCode
    {
        MX
    }

    [Serializable()]
    [XmlTypeAttribute(Namespace = "")]
    public enum SearchCriteriaOption
    {
        [XmlEnum("0")]
        No,
        [XmlEnum("1")]
        Yes,
        [XmlEnum("2")]
        Both
    }

    [Serializable()]
    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SearchCriteria
    {
        bool applySearchCriteriaField = true;
        SearchCriteriaCountryCode sCountryCodeField = SearchCriteriaCountryCode.MX;
        string sTaxIdOrNameField;
        object bIBranchField;
        object rCountryCodeField;
        object rTaxIdOrNameField;
        SearchCriteriaDocumentKind sKindField;
        bool returnBatchAsLikeField;
        object batchField;
        bool useSerialFromField;
        bool useSerialToField;
        uint serialFromField;
        uint serialToField;
        bool useDateFromField;
        bool useDateToField;
        DateTime dateFromField = DateTime.Today;
        DateTime dateToField = DateTime.Today;
        bool useAmountFromField;
        bool useAmountToField;
        decimal amountFromField;
        decimal amountToField;
        SearchCriteriaOption paidField;
        SearchCriteriaOption cancelledField;
        SearchCriteriaOption distributedField;
        uint queryTopField;
        uint orderByField;
        bool descendingField;

        
        public bool ApplySearchCriteria
        {
            get
            {
                return this.applySearchCriteriaField;
            }
            set
            {
                this.applySearchCriteriaField = value;
            }
        }


        public SearchCriteriaCountryCode SCountryCode
        {
            get
            {
                return this.sCountryCodeField;
            }
            set
            {
                this.sCountryCodeField = value;
            }
        }

        
        public string STaxIdOrName
        {
            get
            {
                return this.sTaxIdOrNameField;
            }
            set
            {
                this.sTaxIdOrNameField = value;
            }
        }

        
        public object BIBranch
        {
            get
            {
                return this.bIBranchField;
            }
            set
            {
                this.bIBranchField = value;
            }
        }

        
        public object RCountryCode
        {
            get
            {
                return this.rCountryCodeField;
            }
            set
            {
                this.rCountryCodeField = value;
            }
        }

        
        public object RTaxIdOrName
        {
            get
            {
                return this.rTaxIdOrNameField;
            }
            set
            {
                this.rTaxIdOrNameField = value;
            }
        }


        public SearchCriteriaDocumentKind SKind
        {
            get
            {
                return this.sKindField;
            }
            set
            {
                this.sKindField = value;
            }
        }

        
        public bool ReturnBatchAsLike
        {
            get
            {
                return this.returnBatchAsLikeField;
            }
            set
            {
                this.returnBatchAsLikeField = value;
            }
        }

        
        public object Batch
        {
            get
            {
                return this.batchField;
            }
            set
            {
                this.batchField = value;
            }
        }

        
        public bool UseSerialFrom
        {
            get
            {
                return this.useSerialFromField;
            }
            set
            {
                this.useSerialFromField = value;
            }
        }

        
        public bool UseSerialTo
        {
            get
            {
                return this.useSerialToField;
            }
            set
            {
                this.useSerialToField = value;
            }
        }

        
        public uint SerialFrom
        {
            get
            {
                return this.serialFromField;
            }
            set
            {
                this.serialFromField = value;
            }
        }

        
        public uint SerialTo
        {
            get
            {
                return this.serialToField;
            }
            set
            {
                this.serialToField = value;
            }
        }

        
        public bool UseDateFrom
        {
            get
            {
                return this.useDateFromField;
            }
            set
            {
                this.useDateFromField = value;
            }
        }

        
        public bool UseDateTo
        {
            get
            {
                return this.useDateToField;
            }
            set
            {
                this.useDateToField = value;
            }
        }

        
        public System.DateTime DateFrom
        {
            get
            {
                return this.dateFromField;
            }
            set
            {
                this.dateFromField = value;
            }
        }

        
        public System.DateTime DateTo
        {
            get
            {
                return this.dateToField;
            }
            set
            {
                this.dateToField = value;
            }
        }

        
        public bool UseAmountFrom
        {
            get
            {
                return this.useAmountFromField;
            }
            set
            {
                this.useAmountFromField = value;
            }
        }

        
        public bool UseAmountTo
        {
            get
            {
                return this.useAmountToField;
            }
            set
            {
                this.useAmountToField = value;
            }
        }

        
        public decimal AmountFrom
        {
            get
            {
                return this.amountFromField;
            }
            set
            {
                this.amountFromField = value;
            }
        }

        
        public decimal AmountTo
        {
            get
            {
                return this.amountToField;
            }
            set
            {
                this.amountToField = value;
            }
        }


        public SearchCriteriaOption Paid
        {
            get
            {
                return this.paidField;
            }
            set
            {
                this.paidField = value;
            }
        }


        public SearchCriteriaOption Cancelled
        {
            get
            {
                return this.cancelledField;
            }
            set
            {
                this.cancelledField = value;
            }
        }


        public SearchCriteriaOption Distributed
        {
            get
            {
                return this.distributedField;
            }
            set
            {
                this.distributedField = value;
            }
        }

        
        public uint QueryTop
        {
            get
            {
                return this.queryTopField;
            }
            set
            {
                this.queryTopField = value;
            }
        }

        
        public uint OrderBy
        {
            get
            {
                return this.orderByField;
            }
            set
            {
                this.orderByField = value;
            }
        }

        
        public bool Descending
        {
            get
            {
                return this.descendingField;
            }
            set
            {
                this.descendingField = value;
            }
        }
    }
}