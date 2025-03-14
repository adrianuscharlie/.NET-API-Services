using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PartnerAPI
{
    public class Function
    {
        private static readonly IConfiguration configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        public static string GetConfiguration(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key)) return configuration[key];
                return "Key Empty or Not Found";
            }
            catch (Exception ex)
            {
                return "[ERROR] config not found";
            }

        }
        public static string GenerateJwtToken(string clientId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetConfiguration("Jwt:SecretKey")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, clientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", "B2B Partner")
        };

            var token = new JwtSecurityToken(
                issuer: GetConfiguration("Jwt:Issuer"),
                audience: GetConfiguration("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static bool VerifyXSignature(string publicKeyPath, string clientID, string timeStamp, string signatureBase64)
        {
            try
            {
                // 🔹 Reconstruct the original signed string
                string stringToSign = $"{clientID}|{timeStamp}";

                // 🔹 Load the public key from the file
                string publicKeyContent = System.IO.File.ReadAllText(publicKeyPath);
                using (RSA rsa = LoadPublicKeyFromPem(publicKeyContent))
                {
                    byte[] stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);
                    byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

                    // 🔹 Verify signature using public key
                    bool isValid = rsa.VerifyData(stringToSignBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    return isValid;
                }
            }
            catch
            {
                return false;
            }
        }

        private static RSA LoadPublicKeyFromPem(string publicKeyPem)
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            return rsa;
        }
    }
}
