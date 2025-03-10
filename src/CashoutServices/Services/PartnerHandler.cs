using CashoutServices.Models;
namespace CashoutServices.Services
{
    public interface IPartnerHandler
    {
        Response Cashout(Request request, ConfigRequest config, string url,string trxID);
        Response Reversal(Request request, ConfigRequest config, string url,string trxID);
    }
}
