using System.Security.Cryptography;
using System.Text;

namespace Login.Microservice.Services.TokenServices
{
    public static class SecurityBAL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "SCS0006:Weak hashing function", Justification = "<Pending>")]
        public static string Encrypt(string s)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            SHA1 sha = new SHA1CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(s));
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string EncryptSHA512(string s)
        {
            SHA512 sha512 = new SHA512CryptoServiceProvider();
            sha512.ComputeHash(ASCIIEncoding.ASCII.GetBytes(s));
            byte[] result = sha512.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("x2"); // hex format
            }
            return (sbinary);
        }
        [Obsolete]
        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
