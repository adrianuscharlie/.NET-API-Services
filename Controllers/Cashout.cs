using Microsoft.AspNetCore.Mvc;
using CashoutServices.Models;
using CashoutServices.Services;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CashoutServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Cashout : ControllerBase
    {
        private readonly ICashoutServices services;
        public Cashout(ICashoutServices cashoutServices)
        {
            services = cashoutServices;
        }

        [HttpPost]
        public IActionResult Post([FromBody]Request request)
        {
            try
            {
                if (request == null) return BadRequest("Wrong JSON Request");
                return Ok(services.Cashout(request));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
