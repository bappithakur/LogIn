using System.Security.Cryptography;
using System.Text;

namespace Login.Microservice.Services.TokenServices
{
    public class RefreshToken
    {
        public string GenrateRefreshToken(string UserName, string RefreshToken = null)
        {
            if (RefreshToken == null)
            {

                var Refreshtoken = Encrypt(DateTime.Now.ToString() + "-" + UserName);
                return Refreshtoken;
            }
            else
            {
                string[] token = RefreshToken.Split(":");
                string user = Decrypt(token[1]);
                string TimeStamp = Decrypt(token[0]);
                TimeSpan ts1 = Convert.ToDateTime(TimeStamp) - DateTime.Now;
                if (ts1.Minutes < 20)
                {
                    var Refreshtoken = Encrypt(DateTime.Now.ToString()) + "-" + Encrypt(UserName);
                    return Refreshtoken;
                }
                else
                {
                    return "Your Session is Expired";
                }

            }

        }
        public string Encrypt(string originalString)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");
            if (!String.IsNullOrEmpty(originalString))
            {
                
                System.Security.Cryptography.DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                System.IO.MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream,
                  cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
                StreamWriter writer = new StreamWriter(cryptoStream);
                writer.Write(originalString);
                writer.Flush();
                cryptoStream.FlushFinalBlock();
                writer.Flush();
                return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
            else
            {
                return null;

            }
        }

        public string Decrypt(string cryptedString)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");
            if (String.IsNullOrEmpty(cryptedString))
            {
                throw new ArgumentNullException("The string which needs to be decrypted can not be null.");
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream
              (Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
              cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }
    }
}
