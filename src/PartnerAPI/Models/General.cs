using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PartnerAPI.Models
{
    public class RequestSNAP
    {
        public string partnerReferenceNo { get; set; }
        public string customerNumber { get; set; }
        public string otp { get; set; }
        public Amount amount { get; set; }
        public object additionalInfo { get; set; }
    }


    public class Amount
    {
        public string value { get; set; }
        public string currency { get; set; }
        public Amount(string value)
        {
            this.value = value;
            this.currency = "IDR";
        }
    }
    public class ResponseSNAP
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string referenceNo { get; set; }
        public string partnerReferenceNo { get; set; }
        public string transactionDate { get; set; }
        public object additionalInfo { get; set; }

        public ResponseSNAP() { }
    }


    public class RequestStandard
    {
        public string timeStamp { get; set; }
        public string clientdID { get; set; }
        public string productType { get; set; }
        public string trxType { get; set; }

        public object Detail { get; set; }

    }

    public class DetailStandard
    {
        public string trxID { get; set; }
        public string token { get; set; }
        public string amount { get; set; }
        public string noHp { get; set; }
    }

    public class ResponseStandard
    {
        public string timeStamp { get; set; }
        public string clientID { get; set; }
        public string productType { get; set; }
        public string trxType { get; set; }
        [JsonConverter(typeof(ExpandoObjectConverter))]
        public object Detail { get; set; }
        public string RespCode { get; set; }
        public string RespDetail { get; set; }
    }
}
