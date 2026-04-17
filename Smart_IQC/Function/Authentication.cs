using System.Security.Cryptography;
using System.Text;

namespace Smart_IQC.Function
{
    public class Authentication
    {
        public string GenerateCodeVerifier(int length = 43)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Base64UrlEncode(randomBytes);
        }
        public string GenerateCodeChallenge(string codeVerifier)
        {
            using (var hasher = SHA256.Create())
            {
                var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(hashed);
            }
        }
        private string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input)
                                .TrimEnd('=')
                                .Replace('+', '-')
                                .Replace('/', '_');
            return base64;
        }
        public string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }
    }
}

