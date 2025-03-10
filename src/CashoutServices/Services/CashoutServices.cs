using CashoutServices.Models;
using Serilog;
namespace CashoutServices.Services
{

    public interface ICashoutServices
    {
        Response Cashout(Request request);
        Response Reversal(Request request);

    }
    public class CashoutService:ICashoutServices
    {
        private readonly IKredigramServices kredigramFactory;

        public CashoutService(IKredigramServices kredigramFactory)
        {
            this.kredigramFactory = kredigramFactory;
        }

        public Response Cashout(Request request)
        {
            try
            {
                Response response = new Response();
                ConfigRequest config = Function.GetConfigRequest(request.partnerID);
                if (config == null)
                {
                    string message = "Product is not available or Configuration not found";
                    Log.Warning(message);
                    response.responseMessage = message;
                    response.responseCode = "401";
                    response.additionalInfo = null;
                    response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.Warning($"TRANSACTION  Return :{response.responseCode};{response.responseMessage}");
                    return response;
                }
                var handler = kredigramFactory.GetPartnerHandler(config.kredigram, config.partner);
                if (handler == null)
                {
                    string message = $"Kredigram for {request.partnerID} Not Found or Not Available";
                    Log.Warning(message);
                    response.responseMessage = message;
                    response.responseCode = "402";
                    response.additionalInfo = null;
                    response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.Warning($"TRANSACTION  Return :{response.responseCode};{response.responseMessage}");
                    return response;
                }
                string url = Function.GetURL(request.partnerID, request.trxType);
                string trxID = Function.GenerateTRXID(request.cacode);
                Log.Information($"TRANSACTION {trxID} Request Cashout {url} partnerID:{request.partnerID} otp:{request.otp} amount:{request.amount}");
                return handler.Cashout(request, config, url, trxID);
            }
            catch(Exception ex)
            {
                Log.Error($"Error processing CASHOUT with detail: {ex.Message} {ex.StackTrace}");
                Response response = new Response();
                response.responseCode = "501";
                response.responseMessage = "Internal Server Error Processing CASHOUT";
                return response;
            }
        }


        public Response Reversal(Request request)
        {
            try
            {
                Response response = new Response();
                string trxID = request.detail.ToString();
                ConfigRequest config = Function.GetConfigRequest(request.partnerID);
                if (config == null)
                {
                    Log.Warning("Product is not available or Configuration not found");
                    response.responseMessage = "Product is not available or Configuration not found";
                    response.responseCode = "401";
                    response.additionalInfo = null;
                    response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.Warning($"REVERSAL {trxID} Return :{response.responseCode};{response.responseMessage}");
                    return response;
                }
                Log.Information($"Checking Reversal Transaction for {request.detail}");
                if (!Function.CheckReversal(request.detail.ToString(), config.productType, request.cacode, request.otp, request.amount))
                {
                    Log.Warning($"Transaction with trxID {trxID} and otp {request.otp} and amount {request.amount} Not Found");
                    response.responseMessage = $"Transaction with trxID {trxID} and otp {request.otp} and amount {request.amount} Not Found";
                    response.responseCode = "404";
                    response.additionalInfo = null;
                    response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.Warning($"REVERSAL {trxID} Return :{response.responseCode};{response.responseMessage}");
                    return response;
                }
                Log.Information($"Getting Mode Kredigram Handler for REVERSAL {config.kredigram} {config.partner}");
                var handler = kredigramFactory.GetPartnerHandler(config.kredigram, config.partner);
                if (handler == null)
                {
                    string message = $"Kredigram for {request.partnerID} Not Found or Not Available";
                    Log.Warning(message);
                    response.responseMessage = message;
                    response.responseCode = "402";
                    response.additionalInfo = null;
                    response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.Warning($"TRANSACTION  Return :{response.responseCode};{response.responseMessage}");
                    return response;
                }
                string url = Function.GetURL(request.partnerID, request.trxType);
                Log.Information($"REVERSAL {trxID} Request Reversal {url} partnerID:{request.partnerID} otp:{request.otp} amount:{request.amount}");
                return handler.Reversal(request, config, url, trxID);

            }
            catch(Exception ex)
            {
                Log.Error($"Error processing REVERSAL with detail: {ex.Message} {ex.StackTrace}");
                Response response = new Response();
                response.responseCode = "501";
                response.responseMessage = "Internal Server Error Processing REVERSAL";
                return response;
            }
        }
    }
}
