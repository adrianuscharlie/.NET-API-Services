using CashoutServices.Partner;
using Serilog;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace CashoutServices.Services
{

    public interface IKredigramServices
    {
        IPartnerHandler GetPartnerHandler(string partnerID,string partner);
    }
    public class KredigramServices:IKredigramServices
    {
        private readonly IServiceProvider serviceProvider;

        public KredigramServices(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }


        public IPartnerHandler GetPartnerHandler(string kredigram, string partner)
        {
            Log.Information($"Getting Mode Kredigram Handler CASHOUT for {kredigram} {partner}");
            try
            {
                return (kredigram, partner) switch
                {
                    ("STANDARD", "DANA") => serviceProvider.GetRequiredService<DANACO>(),
                    ("SNAP", "GOPAY") => serviceProvider.GetRequiredService<GopayCO>(),
                    ("SNAP", "ISAKU") => serviceProvider.GetRequiredService<ISakuCO>(),
                    _ => throw new Exception($"No handler found for PartnerID: {kredigram} and Partner: {partner}")
                };
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

    }
}
