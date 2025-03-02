using CashoutServices.Models;
using CashoutServices.Services;
using Newtonsoft.Json;

namespace CashoutServices.Partner
{
    public class STANDARD : IPartnerHandler
    {
        public object Cashout(Request request, ConfigRequest config, string url, string trxID)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RequestStandard requestStandar = new RequestStandard();
            requestStandar.clientdID = config.clientID;
            requestStandar.timeStamp = timestamp;
            requestStandar.productType = config.productType;
            requestStandar.trxType = request.trxType;
            requestStandar.Detail = GetDetail(request, config, trxID);
            

            string json = JsonConvert.SerializeObject(requestStandar);
            Function.InsertTransaction(trxID, config.productType, timestamp, request.amount, request.otp, request.customerNumber, request.cacode,json, request.trxType);
            string responseString = Function.SendHTTP_POST(url, json);
            Response response = JsonConvert.DeserializeObject<Response>(responseString);

            Function.UpdateTransaction(trxID, request.trxType, json, response.responseCode, response.responseMessage, response.originalReferenceNo);
            return response;
        }

        public virtual object GetDetail(Request request,ConfigRequest config,string trxID)
        {
            DetailStandard detail = new DetailStandard();
            detail.trxID = trxID;
            detail.amount = request.amount;
            detail.token = request.otp;
            detail.noHp = request.customerNumber;
            return detail;
        }

        public object Notification(Request request, ConfigRequest config, string url, string trxID)
        {
            throw new NotImplementedException();
        }

        public object Reversal(Request request, ConfigRequest config, string url, string trxID)
        {
            throw new NotImplementedException();
        }

        public virtual string GetToken()
        {
            return "";
        }

        public virtual Dictionary<string,string> GetHeaders()
        {
            return new Dictionary<string, string>();
        }
    }

    public class DANACO : STANDARD
    {
        public override string GetToken()
        {
            return "TOKENABCD";
        }
    }
}
