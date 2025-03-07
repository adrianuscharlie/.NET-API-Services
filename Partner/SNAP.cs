using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using CashoutServices.Models;
using CashoutServices.Services;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CashoutServices.Partner
{
    public class SNAP : IPartnerHandler
    {
        public object Cashout(Request request, ConfigRequest config, string url, string trxID)
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
            Dictionary<string,string> headers=GetHeaders(request.partnerID,url,config.isToken);
            string responseString = Function.SendHTTP_POST(url, json,headers);
            ResponseCashoutSnap response = new();
            if (!string.IsNullOrEmpty(responseString)&& !responseString.StartsWith("[ERROR]")){
                response = JsonConvert.DeserializeObject<ResponseCashoutSnap>(responseString);
                Function.UpdateTransaction(trxID, request.trxType, responseString, response.responseCode, response.responseMessage, response.referenceNo);
            }else if (responseString.StartsWith("[ERROR]"))
            {
                response.responseCode = responseString.Split("|")[1];
                response.responseMessage = responseString.Split("|")[2];
            }
            else
            {
                response.responseCode = "21";
                response.responseMessage = "Server  Partner Timeout or Internal Server  Error";
            }
                return response;
        }


        public object Reversal(Request request, ConfigRequest config, string url, string trxID)
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
            ResponseReversalSNAP response = new();
            if (!string.IsNullOrEmpty(responseString) && !responseString.StartsWith("[ERROR]"))
            {
                response = JsonConvert.DeserializeObject<ResponseReversalSNAP>(responseString);
                Function.UpdateTransaction(trxID, request.trxType, responseString, response.responseCode, response.responseMessage, response.OriginalReferenceNo);
            }
            else if (responseString.StartsWith("[ERROR]"))
            {
                response.responseCode = responseString.Split("|")[1];
                response.responseMessage = responseString.Split("|")[2];
            }
            else
            {
                response.responseCode = "21";
                response.responseMessage = "Server  Partner Timeout or Internal Server  Error";
            }
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
            }
            return headers;
        }

        public virtual string? CheckToken(string partnerID)
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

    public class ISaku : SNAP
    {
        private static string clientID = Function.GetConfiguration("applicationSettings:isaku:clientID");
        private static string merchantID = Function.GetConfiguration("applicationSettings:isaku:merchantID");
        private static string client_secret = Function.GetConfiguration("applicationSettings:isaku:clientSecret");
        private static string x_device_id = Function.GetConfiguration("applicationSettings:isaku:X-DEVICE-ID");
        private static readonly string privateKey=Function.GetConfiguration("applicationSettings:isaku:privateKey");

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
            }
            string signatureServices = Function.GenerateSignatureService(client_secret, url, timeStamp, token, json);
            string externalID = "";
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;
                string[] keyToFind = { "partnerReferenceNo", "originalPartnerReferenceNo" };
                foreach (string key in keyToFind)
                {
                    if (root.TryGetProperty(key, out var value))
                    {
                        externalID = value + DateTime.Now.ToString("yyyyMMddss");
                        break;
                    }
                }
            }
            headers.Add("Bearer", token);
            headers.Add("X-TIMESTAMP", timeStamp);
            headers.Add("X-SIGNATURE", signatureServices);
            headers.Add("X-PARTNER-ID", clientID);
            headers.Add("X-EXTERNAL-ID", externalID);
            headers.Add("CHANNEL-ID", merchantID);
            return headers;
        }

        public override string? GetToken(string partnerID)
        {
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
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            string url = Function.GetURL(partnerID, "TOKEN");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Add("X-TIMESTAMP", timeStamp);
                    client.DefaultRequestHeaders.Add("X-CLIENT-KEY", clientID);
                    client.DefaultRequestHeaders.Add("X-SIGNATURE", X_SIGNATURE);
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        JObject responseObject = JObject.Parse(responseString);
                        JObject additionalInfo = (JObject)responseObject["additionalInfo"];
                        TimeSpan expired = TimeSpan.FromSeconds(Convert.ToInt32(additionalInfo["expiresIn"]));
                        string token = responseObject["accessToken"].ToString();
                        Function.SetToken(partnerID, token, expired);
                        return token;
                    }
                    else
                    {
                        throw new Exception(responseString);
                    }
                }

            }
            catch (Exception ex)
            {
                return "";
            }

        }
    }



//------------------------------------------ GOPAY ---------------------------------------------------------------------------------------

    public class Gopay : SNAP
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
            }
            string signatureServices = Function.GenerateSignatureService(clientSecret,url, timeStamp, token, json);
            string externalID = "";
            //using (JsonDocument document = JsonDocument.Parse(json))
            //{
            //    JsonElement root = document.RootElement;
            //    string[] keyToFind = { "partnerReferenceNo", "originalPartnerReferenceNo" };
            //    foreach (string key in keyToFind)
            //    {
            //        if (root.TryGetProperty(key, out var value))
            //        {
            //            externalID = value + DateTime.Now.ToString("yyyyMMddss");
            //            break;
            //        }
            //    }
            headers.Add("X-TIMESTAMP", timeStamp);
            headers.Add("X-SIGNATURE", signatureServices);
            headers.Add("X-PARTNER-ID", clientID);
            headers.Add("X-EXTERNAL-ID", externalID);
            headers.Add("CHANNEL-ID", channelID);
            return headers;
        }

        public override string? GetToken(string partnerID)
        {
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
            string jsonContent=JsonConvert.SerializeObject(requestBody);
            StringContent content = new StringContent(jsonContent,Encoding.UTF8,"application/json");
            string url = Function.GetURL(partnerID, "TOKEN");
            try
            {
                using(HttpClient client=new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Add("X-TIMESTAMP", timeStamp);
                    client.DefaultRequestHeaders.Add("X-CLIENT-KEY", clientID);
                    client.DefaultRequestHeaders.Add("X-SIGNATURE", X_SIGNATURE);
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        JObject responseObject = JObject.Parse(responseString);
                        JObject additionalInfo = (JObject)responseObject["additionalInfo"];
                        TimeSpan expired = TimeSpan.FromSeconds(Convert.ToInt32(additionalInfo["expiresIn"]));
                        string token = responseObject["accessToken"].ToString();
                        Function.SetToken(partnerID, token, expired);
                        return token;
                    }
                    else
                    {
                        throw new Exception(responseString);
                    }
                }

            }catch(Exception ex)
            {
                return "";
            }

        }
    }
}
