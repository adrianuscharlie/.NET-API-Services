using CashoutServices.Models;
using CashoutServices.Services;
using Newtonsoft.Json;
using Serilog;

namespace CashoutServices.Partner
{
    public class STANDARD : IPartnerHandler
    {
        public Response Cashout(Request request, ConfigRequest config, string url, string trxID)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                RequestStandard requestStandard = new RequestStandard();
                requestStandard.clientdID = config.clientID;
                requestStandard.timeStamp = timestamp;
                requestStandard.productType = config.productType;
                requestStandard.trxType = request.trxType;
                requestStandard.Detail = GetDetail(request, config, trxID);


                string json = JsonConvert.SerializeObject(requestStandard);
                Function.InsertTransaction(trxID, config.productType, timestamp, request.amount, request.otp, request.customerNumber, request.cacode, json, request.trxType);
                Dictionary<string, string> headers = GetHeaders(requestStandard, config);
                string responseString = Function.SendHTTP_POST(url, json, headers);

                ResponseStandard responseStandard = new ResponseStandard();
                Response response = new Response();
                response.originalReferenceNo = trxID;
                response.customerNumber = request.customerNumber;
                if (!string.IsNullOrEmpty(responseString) && !responseString.StartsWith("[ERROR]"))
                {
                    responseStandard = JsonConvert.DeserializeObject<ResponseStandard>(responseString);
                    response.transactionDate = responseStandard.timeStamp;
                    response.responseCode = responseStandard.RespCode;
                    response.responseMessage = responseStandard.RespDetail;
                    response.additionalInfo = responseStandard.Detail?.ToObject<Dictionary<string, string>>();
                    response.referenceNo = "OK";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, response.responseCode, response.responseMessage, "OK");
                }
                else if (responseString.StartsWith("[ERROR]"))
                {
                    string responseCode = responseString.Split("|")[1];
                    string responseMessage = responseString.Split("|")[2];
                    response.responseCode = responseCode;
                    response.responseMessage = responseMessage;
                    response.additionalInfo = null;
                    response.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseCode, responseMessage, "FAIL");
                }
                else
                {
                    response.responseCode = "21";
                    response.responseMessage = "Server  Partner Timeout or Internal Server  Error";
                    response.additionalInfo = null;
                    response.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, response.responseCode, response.responseCode, "FAIL");
                }
                return response;
            }
            catch(Exception ex)
            {
                Log.Error($"Error processing cashout request {ex.Message} {ex.StackTrace}");
                Response response = new Response();
                response.responseCode = "501";
                response.responseMessage = ex.Message;
                response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                response.referenceNo = "ERROR";
                return response;
            }
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

        public Response Reversal(Request request, ConfigRequest config, string url, string trxID)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                RequestStandard requestStandar = new RequestStandard();
                requestStandar.clientdID = config.clientID;
                requestStandar.timeStamp = timestamp;
                requestStandar.productType = config.productType;
                requestStandar.trxType = request.trxType;
                requestStandar.Detail = GetDetail(request, config, trxID);


                string json = JsonConvert.SerializeObject(requestStandar);
                Function.InsertTransaction(trxID, config.productType, timestamp, request.amount, request.otp, request.customerNumber, request.cacode, json, request.trxType);
                Dictionary<string, string> headers = GetHeaders(requestStandar, config);
                string responseString = Function.SendHTTP_POST(url, json, headers);

                ResponseStandard responseStandard = new ResponseStandard();
                Response response = new Response();
                response.originalReferenceNo = trxID;
                response.customerNumber = request.customerNumber;
                if (!string.IsNullOrEmpty(responseString) && !responseString.StartsWith("[ERROR]"))
                {
                    responseStandard = JsonConvert.DeserializeObject<ResponseStandard>(responseString);
                    response.transactionDate = responseStandard.timeStamp;                    
                    response.responseCode = responseStandard.RespCode;
                    response.responseMessage = responseStandard.RespDetail;
                    response.additionalInfo = responseStandard.Detail?.ToObject<Dictionary<string, string>>();
                    response.originalReferenceNo = trxID;
                    response.referenceNo = "OK";
                    Function.UpdateTransaction(trxID,request.otp, request.trxType, responseString, response.responseCode, response.responseMessage, "OK");
                }
                else if (responseString.StartsWith("[ERROR]"))
                {
                    string responseCode = responseString.Split("|")[1];
                    string responseMessage = responseString.Split("|")[2];
                    response.responseCode = responseCode;
                    response.responseMessage = responseMessage;
                    response.additionalInfo = null;
                    response.originalReferenceNo = trxID;
                    response.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseCode, responseMessage, "FAIL");
                }
                else
                {
                    response.responseCode = "21";
                    response.responseMessage = "Server  Partner Timeout or Internal Server  Error";
                    response.additionalInfo = null;
                    response.originalReferenceNo = trxID;
                    response.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, response.responseCode, response.responseCode, "FAIL");
                }
                return response;
            }
            catch(Exception ex)
            {
                Log.Error($"Error processing reversal request {ex.Message} {ex.StackTrace}");
                Response response = new Response();
                response.responseCode = "501";
                response.responseMessage = ex.Message;
                response.customerNumber = request.customerNumber;
                response.originalReferenceNo = trxID;
                response.referenceNo = "ERROR";
                response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                return response;
            }
        }

        public virtual string GetToken()
        {
            return null;
        }

        public virtual Dictionary<string,string> GetHeaders(RequestStandard request, ConfigRequest config)
        {
            return new Dictionary<string, string>();
        }
    }

    public class DANACO : STANDARD
    {
        private readonly string apiKey = Function.GetConfiguration("ApplicationSettings:merchant:Dana:apiKey");
        public override Dictionary<string, string> GetHeaders(RequestStandard request, ConfigRequest config)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-API-KEY", apiKey);
            return headers;
        }
    }
}
