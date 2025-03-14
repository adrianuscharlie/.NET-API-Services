using Microsoft.AspNetCore.Mvc;
using PartnerAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PartnerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Reversal : ControllerBase
    {
        
        // POST api/<Reversal>
        [HttpPost("Dana")]
        public IActionResult Post([FromBody]RequestStandard request)
        {
            string apiKey = Request.Headers["X-API-KEY"];

            // 🔹 Validate request headers
            if (string.IsNullOrEmpty(apiKey)) return Unauthorized(new { message = "Missing required headers" });

            ResponseStandard response = new ResponseStandard();
            response.RespCode = "00";
            response.RespDetail = "Reversal Success";
            response.clientID = request.clientdID;
            response.trxType = request.trxType;
            response.productType = request.productType;
            response.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            response.Detail = request.Detail;
            return Ok(response);
        }

        
    }
}
