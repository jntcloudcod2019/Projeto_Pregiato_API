using System.Text.Json.Serialization;

namespace Pregiato.API.Response
{
    public class BillingResponseProducers
    {

        public bool SUCESS { get; set; }
        public string MESSAGE { get; set; }
        public List<BillingDataProducers> DATA { get; set; } 

        public BillingDataResume RESUME { get; set; }

    }


    public class BillingDataProducers
    {
        public string NAMEPRODUCERS { get; set; }
        public decimal AMOUNTCONTRACT{ get; set; }
        public int TRANSACTIONSCOUNT { get; set; }
        public string DATE { get; set; }
        public string STATUSCONTRACT { get; set; }
        public ModelDetails MODELDETAILS { get; set; }
    }

    public class BillingDataResume
    {
        public decimal TOTASTALESCONTRACT { get; set; }

        public int TOTALCONTRACTS { get; set; }
    }

    public class ModelDetails
    {
        public string IdModel { get; set; }
        public string NameModel { get; set; }
        public string DocumentModel { get; set; }
    }
}

