using CashoutServices.Models;
using MySql.Data.MySqlClient;
using System.Text;
using CashoutServices.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
using StackExchange.Redis;
using System.Net.Http.Headers;
using Serilog;
using System.Net;

namespace CashoutServices
{
    public class Function
    {
        private static readonly IConfiguration configuration=new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        private static readonly Lazy<RedisServices> _redisServices = new(() =>
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            return new RedisServices(redis);
        });

        public static RedisServices redisServices => _redisServices.Value;
        public static string GetConfiguration(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key)) return configuration[key];
                return "Key Empty or Not Found";
            }
            catch(Exception ex)
            {
                return "[ERROR] config not found";
            }
            
        }

        public static MySqlConnection GetConnection(string connectionString="")
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = configuration["ApplicationSettings:connectionString"];
            else connectionString = configuration[connectionString];
            return new MySqlConnection(connectionString);
        }

        public static ConfigRequest GetConfigRequest(string partnerID)
        {
            Log.Information($"Get Request Configuration with PartnerID : {partnerID}");
            ConfigRequest request = new(partnerID);
            try
            {
                using(MySqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using(MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM MASTERMERCHANT WHERE partnerID=@partnerID LIMIT 1";
                        command.Parameters.AddWithValue("@partnerID", partnerID);
                        using(MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                request.productType = reader.GetString("productType");
                                request.partner = reader.GetString("partner");
                                request.kredigram = reader.GetString("kredigram");
                                request.clientID = reader.GetString("clientID");
                                request.isToken = reader.GetBoolean("isToken");
                                request.extraParam1 = reader.GetString("extraParam1");
                                request.extraParam2 = reader.GetString("extraParam2");
                                request.extraParam3 = reader.GetString("extraParam3");
                                return request;
                            }
                            else
                            {
                                Log.Warning($"Config for partnerID:{partnerID} Not Found!");
                                return null;
                            }
                        }
                    }
                }

            }catch(Exception ex)
            {
                Log.Error($"Error Getting Config for partnerID {partnerID} : {ex.Message} \n {ex.StackTrace}");
                return null;
            }
        }
        public static string GetURL(string partnerID, string trxType)
        {
            Log.Information($"Getting URL for Partner {partnerID} with TrxType {trxType}");
            try
            {
                using(MySqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using(MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT URL FROM LIST_URL WHERE partnerID=@partnerID and trxType=@trxType LIMIT 1";
                        command.Parameters.AddWithValue("@partnerID", partnerID);
                        command.Parameters.AddWithValue("@trxType", trxType);
                        string url= command.ExecuteScalar().ToString();
                        Log.Information($"Found, returning URL {url}");
                        return url;
                    }
                }
            }catch(Exception ex)
            {
                Log.Error($"Error! URL for {partnerID} {trxType} not found");
                return "[ERROR] URL NOT FOUND";
            }
        }

        public static string GenerateTRXID(string cacode)
        {
            return cacode + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss").Replace("-", "").Replace(" ", "");
        }



        public static void InsertTransaction(string trxID, string productType, string timeStamp, string amount,
            string otp, string customerNumber, string cacode, string request, string trxType)
        {
            Log.Information($"Insert Transaction {trxID};{productType};{amount};{otp};{trxType};{request}");
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    string table = (trxType != "REVERSAL") ? "TRANSACTION" : "REVERSAL";
                    command.CommandText = $"INSERT INTO {table} (trxID, productType, timeStamp, amount, otp, customerNumber, cacode, request, trxType) " +
                      "VALUES (@trxID, @productType, @timeStamp, @amount, @otp, @customerNumber, @cacode, @request, @trxType)";

                    command.Parameters.AddWithValue("@trxID", trxID);
                    command.Parameters.AddWithValue("@productType", productType);
                    command.Parameters.AddWithValue("@timeStamp", timeStamp);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@otp", otp);
                    command.Parameters.AddWithValue("@customerNumber", customerNumber);
                    command.Parameters.AddWithValue("@cacode", cacode);
                    command.Parameters.AddWithValue("@request", request);
                    command.Parameters.AddWithValue("@trxType", trxType);
                    command.ExecuteNonQuery();

                    if (trxType == "REVERSAL")
                    {
                        command.CommandText = $"UPDATE TRANSACTION SET reversalDate=@reversalDate, reversalCode=@reversalCode," +
                            "reversalMessage=@reversalMessage where otp=@otp and trxID=@trxID and trxType!='REVERSAL'";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@reversalDate",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@reversalCode","XX");
                        command.Parameters.AddWithValue("@reversalMessage","Before Request");
                        command.Parameters.AddWithValue("@otp",otp);
                        command.Parameters.AddWithValue("@trxID",trxID);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void UpdateTransaction(string trxID,string otp, string trxType, string response, string responseCode, string responseMessage, string trxConfirm)
        {
            Log.Information($"Update Transaction {trxID};{trxType};{response};{responseCode};{responseMessage};{trxConfirm}");
            using (MySqlConnection connection = GetConnection())
            {
                string table = (trxType != "REVERSAL" ? "TRANSACTION" : "REVERSAL");
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    if (trxType != "REVERSAL")
                    {
                        command.CommandText = "UPDATE TRANSACTION SET response = @response, " +
                                          "responseCode = @responseCode, responseMessage = @responseMessage, trxConfirm = @trxConfirm " +
                                          "WHERE trxID = @trxID and otp=@otp and trxType = @trxType";
                        command.Parameters.AddWithValue("@trxID", trxID);
                        command.Parameters.AddWithValue("@trxType", trxType);
                        command.Parameters.AddWithValue("@response", response);
                        command.Parameters.AddWithValue("@responseCode", responseCode);
                        command.Parameters.AddWithValue("@responseMessage", responseMessage);
                        command.Parameters.AddWithValue("@trxConfirm", trxConfirm);
                        command.Parameters.AddWithValue("@otp", otp);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "UPDATE TRANSACTION SET reversalDate = @reversalDate, " +
                                               "reversalCode = @reversalCode, reversalMessage = @reversalMessage " +  
                                               "WHERE trxID = @trxID AND trxType = @trxType";
                        command.Parameters.AddWithValue("@trxID", trxID);
                        command.Parameters.AddWithValue("@trxType", trxType);
                        command.Parameters.AddWithValue("@reversalDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@reversalCode", responseCode);
                        command.Parameters.AddWithValue("@reversalMessage", responseMessage);
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();
                        command.CommandText = "UPDATE REVERSAL SET response = @response, " +
                                         "responseCode = @responseCode, responseMessage = @responseMessage, trxConfirm = @trxConfirm " +
                                         "WHERE trxID = @trxID and otp=@otp and trxType = @trxType";
                        command.Parameters.AddWithValue("@trxID", trxID);
                        command.Parameters.AddWithValue("@trxType", trxType);
                        command.Parameters.AddWithValue("@response", response);
                        command.Parameters.AddWithValue("@responseCode", responseCode);
                        command.Parameters.AddWithValue("@responseMessage", responseMessage);
                        command.Parameters.AddWithValue("@trxConfirm", "OK");
                        command.Parameters.AddWithValue("@otp", otp);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }


        public static string SendHTTP_POST(string url, string json, Dictionary<string,string> headers = null, string contentType="application/json")
        {
            try
            {
                string responseString = "";
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            if (header.Key == "Bearer") client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", header.Value);
                            else client.DefaultRequestHeaders.Add(header.Key, header.Value);

                        }
                    }
                    Log.Information($"Send HTTP POST Request {url} with headers:{client.DefaultRequestHeaders.ToString()} body:{json}");
                    using (HttpContent content = new StringContent(json, Encoding.UTF8, contentType))
                    {
                        HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                        responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (!response.IsSuccessStatusCode)
                        {
                            Log.Error($"HTTP POST Request to {url} failed. Response: {responseString}");
                            if (!string.IsNullOrWhiteSpace(responseString))
                            {
                                responseString = $"[ERROR]|{response.StatusCode}|{response.ReasonPhrase}";
                            }
                            else
                            {
                                throw new WebException();
                            }
                        }
                        else
                        {
                            Log.Information($"HTTP POST Response: {responseString}");
                        }
                        return responseString;
                    }

                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response is HttpWebResponse httpResponse)
                {
                    int statusCode = (int)httpResponse.StatusCode;
                    string statusDescription = httpResponse.StatusDescription;
                    Log.Error($"WebException - Status Code: {statusCode}, Status Message: {statusDescription}");
                    return $"[ERROR]|{statusCode}|{statusDescription}";
                }
                else
                {
                    Log.Error($"WebException occurred: {webEx.Message}");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR Sending HTTP POST Request {url} with body:{json}");
                return "";
            }
            
        }


        public static string CheckToken(string partnerID)
        {
            return redisServices.GetToken(partnerID);

        }

        public static void SetToken(string key, string token, TimeSpan expires)
        {
            redisServices.SetToken(key, token, expires);
        }

        public static string ComputeSHA256(string content)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static string ComputeHMACSHA512(string key, string content)
        {
            using (HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
                return Convert.ToBase64String(hashBytes);
            }
        }
        public static string ExtractRelativeURL(string endpoint)
        {
            try
            {
                Uri fullUri = new Uri(endpoint);
                string relativePath = fullUri.PathAndQuery;
                if (relativePath.StartsWith("/")) relativePath = relativePath.Substring(1);
                return relativePath;
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Format URL Salah");
            }
        }

        public static string MinifyJSON(string jsonContent)
        {
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonContent);
            return JsonConvert.SerializeObject(jsonObject, Formatting.None);
        }

        public static RSA LoadPrivateKeyFromPem(string pemContents)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(pemContents);
            return rsa;
        }

        public static string GenerateSignatureService( string clientSecret,string endpoint, string timeStamp, string bearerToken, string jsonContent)
        {
            string relativeUrl = ExtractRelativeURL(endpoint);
            string minifiedJson = MinifyJSON(jsonContent);
            string shaResult = ComputeSHA256(minifiedJson);
            string stringToSign = $"POST:{relativeUrl}:{bearerToken}:{shaResult}:{timeStamp}";
            string signature = ComputeHMACSHA512(clientSecret, stringToSign);
            return signature;

        }
        public static string GenerateXSignature(string privateKey,string clientID, string timeStamp)
        {
            string stringToSign = $"{clientID}|{timeStamp}";

            // Load the private key content from the configuration file
            string privateKeyContent = System.IO.File.ReadAllText(privateKey);

            // Create the RSA instance and sign the data
            using (RSA rsa = Function.LoadPrivateKeyFromPem(privateKeyContent))
            {
                byte[] stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);
                byte[] signatureBytes = rsa.SignData(stringToSignBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                string signatureBase64 = Convert.ToBase64String(signatureBytes);

                return signatureBase64;
            }
        }


        public static bool CheckReversal(string trxID, string productType, string cacode,  string otp, string amount)
        {
            try
            {
                using(MySqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using(MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT COUNT(*) FROM TRANSACTION WHERE " +
                            "trxID=@trxID and otp=@otp and cacode=@cacode and amount=@amount LIMIT 1";
                        command.Parameters.AddWithValue("@trxID", trxID);
                        command.Parameters.AddWithValue("@cacode", cacode);
                        command.Parameters.AddWithValue("@otp", otp);
                        command.Parameters.AddWithValue("@amount", amount);
                        return Convert.ToInt32(command.ExecuteScalar().ToString()) > 0;
                    }
                }
            }catch(Exception ex)
            {
                return false;
            }
        }

    }
}
