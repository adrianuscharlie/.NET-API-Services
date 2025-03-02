using Org.BouncyCastle.Asn1.IsisMtt.X509;
using System.Security.AccessControl;

namespace CashoutServices.Models
{

    public class Request
    {
        
        public string cacode { get; set; }
        public string otp { get; set; }
        public string partnerID { get;set; }
        public string customerNumber { get; set; }
        public string trxType { get; set; }
        public string amount { get; set; }

    }

    public class Response
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string originalReferenceNo { get; set; }
        public string referenceNo { get; set; }
        public string transactionDate { get;set; }
        public object additionalInfo { get;set; }
    }

    public class ConfigRequest
    {
        public string partnerID { get; set; }
        public string productType { get; set; }
        public string partner { get; set; }
        public string kredigram { get; set; }
        public string clientID { get; set; }
        public bool isToken { get; set; }
        public string extraParam1 { get; set; }
        public string extraParam2 { get; set; }
        public string extraParam3 { get; set; }


        public ConfigRequest() { }
        public ConfigRequest(string partnerID)
        {
            this.partnerID = partnerID;
        }
        public ConfigRequest(string partnerID, string productType, string partner, string kredigram, string clientID, bool isToken, string extraParam1, string extraParam2, string extraParam3)
        {
            this.partnerID = partnerID;
            this.productType = productType;
            this.partner = partner;
            this.kredigram = kredigram;
            this.clientID = clientID;
            this.isToken = isToken;
            this.extraParam1 = extraParam1;
            this.extraParam2 = extraParam2;
            this.extraParam3 = extraParam3;
        }
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
        public string cliendID { get; set; }
        public string productType { get; set; }
        public string trxType { get; set; }

        public DetailStandard Detail { get; set; }
        public string RespCode { get; set; }
        public string RespDetail { get; set; }
    }



    public class RequestSNAP
    {
        public string partnerReferenceNo { get; set; }
        public string customerNumber { get; set; }
        public string otp { get; set; }
        public Amount amount { get; set; }
        public object additionalInfo { get; set; }
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


    public class AdditionalInfoGopay
    {
        public string merchantId { get; set; }
        public string merchantName { get; set; }
        public string externalStoreId { get; set; }
        public string branchId { get; set; }
        public string terminalId { get; set; }

        public AdditionalInfoGopay(string merchantId, string merchantName, string externalStoreId, string branchId, string terminalId)
        {
            this.merchantId = merchantId;
            this.merchantName = merchantName;
            this.externalStoreId = externalStoreId;
            this.branchId = branchId;
            this.terminalId = terminalId;
        }

        public AdditionalInfoGopay() { }
    }


}
