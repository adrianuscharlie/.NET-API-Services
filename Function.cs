using CashoutServices.Models;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using static System.Net.WebRequestMethods;
using System.Reflection.Emit;
using System.Text;

namespace CashoutServices
{
    public class Function
    {
        private static readonly IConfiguration configuration=new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


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
                                return null;
                            }
                        }
                    }
                }

            }catch(Exception ex)
            {
                return null;
            }
        }
        public static string GetURL(string partnerID, string trxType)
        {
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
                        return command.ExecuteScalar().ToString();
                    }
                }
            }catch(Exception ex)
            {
                return "[ERROR] URL NOT FOUND";
            }
        }

        public static string GenerateTRXID(string cacode)
        {
            return cacode + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss").Replace("-", "").Replace(" ", "");
        }


        public static void InsertTransaction(string trxID, string productType, string timeStamp,string amount,
            string otp, string customerNumber, string cacode, string request, string trxType)
        {
            using(MySqlConnection connection = GetConnection())
            {
                connection.Open();
                using(MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO TRANSACTION (trxID, productType, timeStamp, amount, otp, customerNumber, cacode, request, trxType) " +
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

                }
            }
        }

        public static void UpdateTransaction(string trxID, string trxType, string response, string responseCode, string responseMessage, string trxConfirm)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE TRANSACTION SET response = @response, " +
                                          "responseCode = @responseCode, responseMessage = @responseMessage, trxConfirm = @trxConfirm " +
                                          "WHERE trxID = @trxID and trxType = @trxType";

                    command.Parameters.AddWithValue("@trxID", trxID);
                    command.Parameters.AddWithValue("@trxType", trxType);
                    command.Parameters.AddWithValue("@response", response);
                    command.Parameters.AddWithValue("@responseCode", responseCode);
                    command.Parameters.AddWithValue("@responseMessage", responseMessage);
                    command.Parameters.AddWithValue("@trxConfirm", trxConfirm);
                    command.ExecuteNonQuery();

                }
            }
        }


        public static string SendHTTP_POST(string url, string json, Dictionary<string,string> headers = null, string contentType="application/json")
        {
            string responseString = "";
            using(HttpClient client=new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                if (headers != null)
                {
                    foreach(var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                using (HttpContent content = new StringContent(json, Encoding.UTF8, contentType))
                {
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                return responseString;
            }
            
        }

    }
}
