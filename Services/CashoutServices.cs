using CashoutServices.Models;
using Google.Protobuf.WellKnownTypes;
using Serilog;
namespace CashoutServices.Services
{

    public interface ICashoutServices
    {
        object Cashout(Request request);
        object Reversal(Request request);
        object Notification(Request request);

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
            Log.Information("Request Transaksi Masuk {@request}", request);
            ConfigRequest config = Function.GetConfigRequest(request.partnerID);
            var handler = kredigramFactory.GetPartnerHandler(config.kredigram,config.partner);
            string url = Function.GetURL(request.partnerID, request.trxType);
            string trxID = Function.GenerateTRXID(request.cacode);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return handler.Cashout(request,config,url,trxID);
        }

        public object Notification(Request request)
        {
            throw new NotImplementedException();
        }

        public object Reversal(Request request)
        {
            throw new NotImplementedException();
        }
    }
}
