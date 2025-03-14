using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PartnerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class B2BToken : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public B2BToken(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("Gopay")]
        public IActionResult B2BGopay([FromBody] TokenRequest requestBody)
        {
            try
            {
                string clientID = Request.Headers["X-CLIENT-KEY"];
                string timeStamp = Request.Headers["X-TIMESTAMP"];
                string signature = Request.Headers["X-SIGNATURE"];

                // 🔹 Validate request headers
                if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(timeStamp) || string.IsNullOrEmpty(signature))
                    return Unauthorized(new { message = "Missing required headers" });

                // 🔹 Verify signature
                string publicKeyPath = Function.GetConfiguration("ApplicationSettings:merchant:gopay:publicKey");
                bool isValid = Function.VerifyXSignature(publicKeyPath, clientID, timeStamp, signature);

                if (!isValid)
                    return Unauthorized(new { message = "Invalid signature" });

                string grantType = requestBody.GrantType;
                AdditionalInfoGopay additionalInfo = JsonConvert.DeserializeObject<AdditionalInfoGopay>(requestBody.AdditionalInfo.ToString());

                string clientId = additionalInfo.ClientId;
                string clientSecret = additionalInfo.ClientSecret;

                // 🔹 Validate client credentials
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return Unauthorized(new { message = "Invalid client credentials" });
                }

                // 🔹 Check against stored credentials (replace with your database lookup)
                if (clientId != Function.GetConfiguration("ApplicationSettings:merchant:gopay:clientID") || clientSecret != Function.GetConfiguration("ApplicationSettings:merchant:gopay:clientSecret"))
                {
                    return Unauthorized(new { message = "Invalid client ID or secret" });
                }

                // 🔹 Generate JWT token
                var token = Function.GenerateJwtToken(clientId);

                return Ok(new
                {
                    accessToken = token,
                    additionalInfo = new { expiresIn = 300 } // Token valid for  5 minutes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        [HttpPost("ISaku")]
        public IActionResult B2BISaku([FromBody] TokenRequest requestBody)
        {
            try
            {
                string clientID = Request.Headers["X-CLIENT-KEY"];
                string timeStamp = Request.Headers["X-TIMESTAMP"];
                string signature = Request.Headers["X-SIGNATURE"];

                // 🔹 Validate request headers
                if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(timeStamp) || string.IsNullOrEmpty(signature))
                    return Unauthorized(new { message = "Missing required headers" });

                // 🔹 Verify signature
                string publicKeyPath = Function.GetConfiguration("ApplicationSettings:merchant:isaku:publicKey");
                bool isValid = Function.VerifyXSignature(publicKeyPath, clientID, timeStamp, signature);

                if (!isValid)
                    return Unauthorized(new { message = "Invalid signature" });

                string grantType = requestBody.GrantType;
                AdditionalInfoISaku additionalInfo = JsonConvert.DeserializeObject<AdditionalInfoISaku>(requestBody.AdditionalInfo.ToString());
                string merchantID = additionalInfo.merchantId;

                // 🔹 Validate client credentials
                if (string.IsNullOrEmpty(clientID))
                {
                    return Unauthorized(new { message = "Invalid client credentials" });
                }

                // 🔹 Check against stored credentials (replace with your database lookup)
                if (clientID != Function.GetConfiguration("ApplicationSettings:merchant:isaku:clientID") ||  merchantID!= Function.GetConfiguration("ApplicationSettings:merchant:isaku:merchantID"))
                {
                    return Unauthorized(new { message = "Invalid client ID or secret" });
                }

                // 🔹 Generate JWT token
                var token = Function.GenerateJwtToken(clientID);

                return Ok(new
                {
                    accessToken = token,
                    additionalInfo = new { expiresIn = 300 } // Token valid for 5 minutes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }




    }
    public class TokenRequest
    {
        [JsonProperty("grantType")]
        public string GrantType { get; set; }

        [JsonProperty("additionalInfo")]
        public object AdditionalInfo { get; set; }
    }

    public class AdditionalInfoGopay
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
    }
    public class AdditionalInfoISaku
    {
        [JsonProperty("merchantId")]
        public string merchantId { get; set; }
    }

}
