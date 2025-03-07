using CashoutServices.Models;
using CashoutServices.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CashoutServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Reversal : ControllerBase
    {
        private readonly ICashoutServices services;
        public Reversal(ICashoutServices cashoutServices)
        {
            services = cashoutServices;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Request request)
        {
            try
            {
                Log.Information($"Request Masuk  CACODE:{request.cacode};customerNumber:{request.customerNumber};amount:{request.amount};trxType:{request.trxType}");
                if (request == null)
                {
                    Log.Information($"Request null");
                    return BadRequest("Wrong JSON Request");
                }
                return Ok(services.Reversal(request));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
