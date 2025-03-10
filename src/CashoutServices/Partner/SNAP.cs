using CashoutServices.Models;
using CashoutServices.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
namespace CashoutServices.Partner
{
    public class SNAP : IPartnerHandler
    {
        public Response Cashout(Request request, ConfigRequest config, string url, string trxID)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                RequestCashoutSNAP requestSnap = new RequestCashoutSNAP();
                requestSnap.partnerReferenceNo = trxID;
                requestSnap.customerNumber = request.customerNumber;
                requestSnap.otp = request.otp;
                requestSnap.amount = new Amount(request.amount);
                requestSnap.additionalInfo = GetAdditionalInfo(request, config);


                string json = JsonConvert.SerializeObject(requestSnap);
                Function.InsertTransaction(trxID, config.productType, timestamp, request.amount, request.otp, request.customerNumber, request.cacode, json, request.trxType);
                Dictionary<string, string> headers = GetHeaders(request.partnerID, url, config.isToken);
                string responseString = Function.SendHTTP_POST(url, json, headers);

                ResponseCashoutSnap responseSnap = new ResponseCashoutSnap();
                Response response = new Response();
                if (!string.IsNullOrEmpty(responseString) && !responseString.StartsWith("[ERROR]"))
                {
                    responseSnap = JsonConvert.DeserializeObject<ResponseCashoutSnap>(responseString);
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseSnap.responseCode, responseSnap.responseMessage, responseSnap.referenceNo);
                }
                else if (responseString.StartsWith("[ERROR]"))
                {
                    string responseCode = responseString.Split("|")[1];
                    string responseMessage = responseString.Split("|")[2];
                    responseSnap.responseCode = responseCode;
                    responseSnap.responseMessage = responseMessage;
                    responseSnap.partnerReferenceNo = trxID;
                    responseSnap.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    responseSnap.additionalInfo = null;
                    responseSnap.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseCode, responseMessage, "FAIL");
                }
                else
                {
                    responseSnap.responseCode = "21";
                    responseSnap.responseMessage = "Server  Partner Timeout or Internal Server  Error";
                    responseSnap.partnerReferenceNo = trxID;
                    responseSnap.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    responseSnap.additionalInfo = null;
                    responseSnap.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseSnap.responseCode, responseSnap.responseMessage, "FAIL");
                }
                response = ConvertResponseCashoutSNAP(responseSnap);
                response.customerNumber = request.customerNumber;
                response.originalReferenceNo = trxID;
                response.customerNumber = request.customerNumber;
                return response;
            }
            catch(Exception ex)
            {
                Log.Error($"Error processing cashout request {ex.Message} {ex.StackTrace}");
                Response response = new Response();
                response.responseCode = "501";
                response.responseMessage = ex.Message;
                response.originalReferenceNo = trxID;
                response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                response.referenceNo = "ERROR";
                response.customerNumber = request.customerNumber;
                return response;
            }
        }


        public Response Reversal(Request request, ConfigRequest config, string url, string trxID)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                RequestReversalSnap requestSnap = new RequestReversalSnap();
                requestSnap.originalPartnerReferenceNo = trxID;
                requestSnap.customerNumber = request.customerNumber;
                requestSnap.additionalInfo = GetAdditionalInfo(request, config);


                string json = JsonConvert.SerializeObject(requestSnap);
                Function.InsertTransaction(trxID, config.productType, timestamp, request.amount, request.otp, request.customerNumber, request.cacode, json, request.trxType);
                Dictionary<string, string> headers = GetHeaders(request.partnerID, url, config.isToken);
                string responseString = Function.SendHTTP_POST(url, json, headers);
                ResponseReversalSNAP responseSnap = new ResponseReversalSNAP();
                Response response = new Response();
                if (!string.IsNullOrEmpty(responseString) && !responseString.StartsWith("[ERROR]"))
                {
                    responseSnap = JsonConvert.DeserializeObject<ResponseReversalSNAP>(responseString);
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseSnap.responseCode, responseSnap.responseMessage, responseSnap.originalReferenceNo);
                }
                else if (responseString.StartsWith("[ERROR]"))
                {
                    responseSnap.responseCode = responseString.Split("|")[1];
                    responseSnap.responseMessage = responseString.Split("|")[2];
                    response.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseSnap.responseCode, responseSnap.responseMessage, "FAIL");
                }
                else
                {
                    responseSnap.responseCode = "21";
                    responseSnap.responseMessage = "Server  Partner Timeout or Internal Server  Error";
                    responseSnap.originalPartnerReferenceNo = trxID;
                    response.referenceNo = "FAIL";
                    Function.UpdateTransaction(trxID, request.otp, request.trxType, responseString, responseSnap.responseCode, responseSnap.responseMessage, "FAIL");
                }
                response = ConvertResponseReversalSNAP(responseSnap);
                response.customerNumber = request.customerNumber;
                response.originalReferenceNo = trxID;
                response.customerNumber = request.customerNumber;
                return response;

            }
            catch(Exception ex)
            {
                Log.Error($"Error processing reversal request {ex.Message} {ex.StackTrace}");
                Response response = new Response();
                response.responseCode = "501";
                response.responseMessage = ex.Message;
                response.originalReferenceNo = trxID;
                response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                response.customerNumber = request.customerNumber;
                response.referenceNo = "ERROR";
                return response;
            }
        }

        public Response ConvertResponseCashoutSNAP(ResponseCashoutSnap responseSnap)
        {
            Response response = new Response();
            response.responseCode = responseSnap.responseCode;
            response.responseMessage = responseSnap.responseMessage;
            response.originalReferenceNo = responseSnap.partnerReferenceNo;
            response.referenceNo = responseSnap.referenceNo;
            response.additionalInfo = responseSnap.additionalInfo?.ToObject<Dictionary<string, string>>();
            response.transactionDate = responseSnap.transactionDate;
            return response;
        }
        public Response ConvertResponseReversalSNAP(ResponseReversalSNAP responseSnap)
        {
            Response response = new Response();
            response.responseCode = responseSnap.responseCode;
            response.responseMessage = responseSnap.responseMessage;
            response.originalReferenceNo = responseSnap.originalPartnerReferenceNo;
            response.referenceNo = responseSnap.originalReferenceNo;
            response.transactionDate = responseSnap.cancelTime;
            return response;
        }


        public virtual object GetAdditionalInfo(Request request,ConfigRequest config)
        {
            return null;
        }

        public virtual Dictionary<string,string> GetHeaders(string partnerID,string url,bool isToken=false,string json="")
        {

            string timeStamp= DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            Dictionary<string,string> headers=new Dictionary<string,string>();
            if (isToken)
            {
                string token = CheckToken(partnerID);
                if (!string.IsNullOrEmpty(token)) headers.Add("Bearer", token);
                else throw new Exception("Token Not Found");
            }
            return headers;
        }

        public string? CheckToken(string partnerID)
        {
            string? token= Function.CheckToken(partnerID);
            if (string.IsNullOrEmpty(token)) return GetToken(partnerID);
            return token;
        }
        public virtual string? GetToken(string partnerID)
        {
            return "";
        }
    }


//------------------------------------------ ISAKU ---------------------------------------------------------------------------------------

    public class ISakuCO : SNAP
    {
        private static string clientID = Function.GetConfiguration("ApplicationSettings:merchant:isaku:clientID");
        private static string merchantID = Function.GetConfiguration("ApplicationSettings:merchant:isaku:merchantID");
        private static string client_secret = Function.GetConfiguration("ApplicationSettings:merchant:isaku:clientSecret");
        private static string x_device_id = Function.GetConfiguration("ApplicationSettings:merchant:isaku:X-DEVICE-ID");
        private static readonly string privateKey=Function.GetConfiguration("ApplicationSettings:merchant:isaku:privateKey");

        public override object GetAdditionalInfo(Request request, ConfigRequest config)
        {
            AdditionalInfoISaku detail= new ();
            detail.merchantId = request.cacode;
            detail.subMerchantId = "INDONESIA " + request.cacode;
            detail.externalStoreId = "";
            detail.TrxType= request.trxType;
            return detail;

        }

        public override Dictionary<string, string> GetHeaders(string partnerID, string url, bool isToken = false, string json = "")
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string token = "";
            if (isToken)
            {
                token = CheckToken(partnerID);
                if (!string.IsNullOrEmpty(token)) headers.Add("Bearer", token);
                else  throw new Exception("Token Not Found");
            }
            string signatureServices = Function.GenerateSignatureService(client_secret, url, timeStamp, token, json);
            string externalID = "";
            headers.Add("X-TIMESTAMP", timeStamp);
            headers.Add("X-SIGNATURE", signatureServices);
            headers.Add("X-PARTNER-ID", clientID);
            headers.Add("X-EXTERNAL-ID", externalID);
            headers.Add("CHANNEL-ID", merchantID);
            return headers;
        }

        public override string? GetToken(string partnerID)
        {
            string token = "";
            string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            string X_SIGNATURE =Function.GenerateXSignature(privateKey,clientID, timeStamp);
            var requestBody = new JObject
            {
                ["grantType"] = "client_credentials",
                ["additionalInfo"] = new JObject
                {
                    ["merchant_id"] = merchantID
                }
            };
            string url = Function.GetURL(partnerID, "TOKEN");
            string json = JsonConvert.SerializeObject(requestBody);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-TIMESTAMP", timeStamp);
            headers.Add("X-CLIENT-KEY", clientID);
            headers.Add("X-SIGNATURE", X_SIGNATURE);
            Log.Information($"Get new B2B Token for ISAKU");
            string responseString = Function.SendHTTP_POST(url, json, headers);
            try
            {
                JObject responseObject = JObject.Parse(responseString);
                JObject additionalInfo = (JObject)responseObject["additionalInfo"];
                TimeSpan expired = TimeSpan.FromSeconds(Convert.ToInt32(additionalInfo["expiresIn"]));
                token = responseObject["accessToken"].ToString();
                Function.SetToken(partnerID, token, expired);
            }catch(Exception ex)
            {
                Log.Error($"Getting new token for ISAKU {ex.Message} {ex.StackTrace}");
                token = null;
            }
            return token;
        }
    }


//------------------------------------------ GOPAY ---------------------------------------------------------------------------------------

    public class GopayCO : SNAP
    {
        private static readonly string clientSecret=Function.GetConfiguration("ApplicationSettings:merchant:Gopay:clientSecret");
        private static readonly string clientID=Function.GetConfiguration("ApplicationSettings:merchant:Gopay:clientID");
        private static string channelID = Function.GetConfiguration("ApplicationSettings:merchant:Gopay:channelID");
        private static readonly string privateKey=Function.GetConfiguration("ApplicationSettings:merchant:Gopay:privateKey");
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

        public override Dictionary<string, string> GetHeaders(string partnerID,string url, bool isToken = false, string json = "")
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string token = "";
            if (isToken)
            {
                token = CheckToken(partnerID);
                if (!string.IsNullOrEmpty(token)) headers.Add("Bearer", token);
                else throw new Exception("Token Not Found");
            }
            string signatureServices = Function.GenerateSignatureService(clientSecret,url, timeStamp, token, json);
            string externalID = "";
            headers.Add("X-TIMESTAMP", timeStamp);
            headers.Add("X-SIGNATURE", signatureServices);
            headers.Add("X-PARTNER-ID", clientID);
            headers.Add("X-EXTERNAL-ID", externalID);
            headers.Add("CHANNEL-ID", channelID);
            return headers;
        }

        public override string? GetToken(string partnerID)
        {
            string token = "";
            string timeStamp= DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            string X_SIGNATURE = Function.GenerateXSignature(privateKey,clientID,timeStamp);
            var requestBody = new JObject
            {
                ["grantType"] = "client_credentials",
                ["additionalInfo"] = new JObject
                {
                    ["clientId"] = clientID,
                    ["clientSecret"] = clientSecret
                }
            };
            string json=JsonConvert.SerializeObject(requestBody);
            string url = Function.GetURL(partnerID, "TOKEN");
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-TIMESTAMP", timeStamp);
            headers.Add("X-CLIENT-KEY", clientID);
            headers.Add("X-SIGNATURE", X_SIGNATURE);
            Log.Information($"Get new B2B Token for GOPAY");
            string responseString = Function.SendHTTP_POST(url, json, headers);
            try
            {
                JObject responseObject = JObject.Parse(responseString);
                JObject additionalInfo = (JObject)responseObject["additionalInfo"];
                TimeSpan expired = TimeSpan.FromSeconds(Convert.ToInt32(additionalInfo["expiresIn"]));
                token = responseObject["accessToken"].ToString();
                Function.SetToken(partnerID, token, expired);
            }catch(Exception ex)
            {
                Log.Error($"Getting new token for GOPAY {ex.Message} {ex.StackTrace}");
                token = null;
            }
            return token;
        }
    }
}
