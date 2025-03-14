using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
using PartnerAPI.Models;
namespace PartnerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Cashout : ControllerBase
    {


        // POST api/<Cashout>
        [Authorize(Policy = "GopayPolicy")]
        [HttpPost("Gopay")]
        public IActionResult CashoutGopay([FromBody]RequestSNAP request)
        {

            ResponseSNAP response = new ResponseSNAP();
            response.partnerReferenceNo = request.partnerReferenceNo;
            response.responseCode = "00";
            response.responseMessage = "SUKSES";
            response.transactionDate= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            response.additionalInfo = request.additionalInfo;
            response.referenceNo = "Gopay-" + request.partnerReferenceNo;
            return Ok(response);
        }

        [Authorize(Policy = "ISakuPolicy")]
        [HttpPost("ISaku")]
        public IActionResult CashoutISaku([FromBody] RequestSNAP request)
        {

            ResponseSNAP response = new ResponseSNAP();
            response.partnerReferenceNo = request.partnerReferenceNo;
            response.responseCode = "00";
            response.responseMessage = "SUKSES";
            response.transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            response.additionalInfo = request.additionalInfo;
            response.referenceNo = "ISaku-" + request.partnerReferenceNo;
            return Ok(response);
        }

        [HttpPost("Dana")]
        public IActionResult CashoutDana([FromBody] RequestStandard request)
        {
            string apiKey = Request.Headers["X-API-KEY"];

            // 🔹 Validate request headers
            if (string.IsNullOrEmpty(apiKey)) return Unauthorized(new { message = "Missing required headers" });

            ResponseStandard response = new ResponseStandard();
            response.RespCode = "00";
            response.RespDetail = "Success!";
            response.clientID = request.clientdID;
            response.trxType = request.trxType;
            response.productType = request.productType;
            response.timeStamp= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            response.Detail = request.Detail;
            return Ok(response);

        }

    }
}
