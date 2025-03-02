using CashoutServices.Models;
using CashoutServices.Services;
using Newtonsoft.Json;
namespace CashoutServices.Partner
{
    public class SNAP : IPartnerHandler
    {
        public object Cashout(Request request, ConfigRequest config, string url, string trxID)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RequestSNAP requestSnap = new RequestSNAP();
            requestSnap.partnerReferenceNo = trxID;
            requestSnap.customerNumber = request.customerNumber;
            requestSnap.otp = request.otp;
            requestSnap.amount = new Amount(request.amount);
            requestSnap.additionalInfo = GetAdditionalInfo(request, config);
            string json = JsonConvert.SerializeObject(requestSnap);
            Function.InsertTransaction(trxID, config.productType, timestamp, request.amount, request.otp, request.customerNumber, request.cacode, json, request.trxType);
            string responseString = Function.SendHTTP_POST(url, json);
            Response response = JsonConvert.DeserializeObject<Response>(responseString);

            Function.UpdateTransaction(trxID, request.trxType, json, response.responseCode, response.responseMessage, response.originalReferenceNo);

            return response;
        }

        public object Notification(Request request, ConfigRequest config, string url, string trxID)
        {
            throw new NotImplementedException();
        }

        public object Reversal(Request request, ConfigRequest config, string url, string trxID)
        {
            throw new NotImplementedException();
        }

        public virtual object GetAdditionalInfo(Request request,ConfigRequest config)
        {
            return null;
        }
    }

    public class Gopay : SNAP
    {
        public override object GetAdditionalInfo(Request request,ConfigRequest config)
        {
            AdditionalInfoGopay detail = new();
            detail.merchantId = request.cacode;
            detail.merchantName = "INDONESIA " + request.cacode;
            detail.branchId = "";
            detail.externalStoreId = "";
            detail.terminalId = detail.branchId + detail.externalStoreId;
            return detail;

        }

    }
}
