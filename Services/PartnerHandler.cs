using CashoutServices.Models;
using CashoutServices.Services;
using CashoutServices.Partner;
namespace CashoutServices.Services
{
    public interface IPartnerHandler
    {
        object Cashout(Request request, ConfigRequest config, string url,string trxID);
        object Reversal(Request request, ConfigRequest config, string url,string trxID);
        object Notification(Request request, ConfigRequest config, string url, string trxID);
    }
}
