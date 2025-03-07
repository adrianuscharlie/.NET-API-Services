using CashoutServices.Models;
using Google.Protobuf.WellKnownTypes;
using Serilog;
namespace CashoutServices.Services
{

    public interface ICashoutServices
    {
        object Cashout(Request request);
        object Reversal(Request request);

    }
    public class CashoutService:ICashoutServices
    {
        private readonly IKredigramServices kredigramFactory;

        public CashoutService(IKredigramServices kredigramFactory)
        {
            this.kredigramFactory = kredigramFactory;
        }

        public object Cashout(Request request)
        {
            Log.Information($"Get Request Configuration with PartnerID : {request.partnerID}");
            ConfigRequest config = Function.GetConfigRequest(request.partnerID);
            Log.Information($"Getting Mode Kredigram Handler for {config.kredigram} {config.partner}");
            var handler = kredigramFactory.GetPartnerHandler(config.kredigram,config.partner);
            string url = Function.GetURL(request.partnerID, request.trxType);
            string trxID = Function.GenerateTRXID(request.cacode);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Log.Information($"{trxID} Request Cashout {url} partnerID:{request.partnerID} otp:{request.otp} amount:{request.amount}");
            return handler.Cashout(request,config,url,trxID);
        }


        public object Reversal(Request request)
        {
            Log.Information($"Get Request Configuration with PartnerID : {request.partnerID}");
            ConfigRequest config = Function.GetConfigRequest(request.partnerID);
            Log.Information($"Checking Reversal Transaction for {request.detail}");
            if (!Function.CheckReversal(request.detail.ToString(), config.productType, request.cacode, request.otp))
            {
                Response response = new();
                response.responseMessage = "Transaction Not Found";
                response.responseCode = "404";
                Log.Warning($"{response.responseMessage}");
                return response;
            }
            Log.Information($"Getting Mode Kredigram Handler for {config.kredigram} {config.partner}");
            var handler = kredigramFactory.GetPartnerHandler(config.kredigram, config.partner);
            string url = Function.GetURL(request.partnerID, request.trxType);
            string trxID = Function.GenerateTRXID(request.cacode);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Log.Information($"{trxID} Request Reversal {url} partnerID:{request.partnerID} otp:{request.otp} amount:{request.amount}");
            return handler.Reversal(request, config, url, trxID);
        }
    }
}
