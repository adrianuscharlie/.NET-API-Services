using CashoutServices.Partner;

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


        public IPartnerHandler GetPartnerHandler(string partnerID, string partner)
        {
            return (partnerID, partner) switch
            {
                ("STANDARD", "DANA") => serviceProvider.GetRequiredService<DANACO>(),
                ("SNAP", "GOPAY") => serviceProvider.GetRequiredService<Gopay>(),
                _ => throw new Exception($"No handler found for PartnerID: {partnerID} and Partner: {partner}")
            };
        }

    }
}
