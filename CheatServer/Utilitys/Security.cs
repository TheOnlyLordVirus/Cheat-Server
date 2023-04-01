using CheatServer.Transports;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CheatServer.Utilitys.Security
{
    public static class Security
    {
        private static readonly string _encryptionKey = "Cba321";
        private static readonly string _decryptionKey = "Abc123";

        public static bool TryDecrypt<TRequest>(this Request encryptedRequest, out TRequest? request)
        {
            request = default;
            bool result = true;
            string[] errors = Array.Empty<string>();

            try
            {
                string unencryptedString = Security.DecryptString(encryptedRequest.Data);
                request = JsonConvert.DeserializeObject<TRequest?>(unencryptedString);

                if(request is null)
                    result = false;
            }

            catch(Exception ex)
            {
                result = false;
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message),
                    string.Concat("Inner Exception: ", ex.InnerException)
                };
            }

            return result;
        }

        public static string DecryptString(string cipherText)
        {
            SHA256 SHA256 = SHA256.Create();
            byte[] key = SHA256.ComputeHash(Encoding.ASCII.GetBytes(_decryptionKey));

            byte[] iv = new byte[16]
            {
                0x86, 0xc0, 0x71, 0x97, 0x16, 0x9a, 0x25, 0x7b,
                0x70, 0xf0, 0x9c, 0x3e, 0x8f, 0x61, 0x4d, 0xb4
            };

            Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
            encryptor.Key = key;
            encryptor.IV = iv;
            encryptor.Padding = PaddingMode.PKCS7;

            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);
            
            string plainText = String.Empty;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                cryptoStream.FlushFinalBlock();

                byte[] plainBytes = memoryStream.ToArray();

                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }

            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }

            return plainText;
        }

        public static string EncryptString(string plainText)
        {
            SHA256 SHA256 = SHA256.Create();
            byte[] key = SHA256.ComputeHash(Encoding.ASCII.GetBytes(_encryptionKey));

            byte[] iv = new byte[16]
            {
                    0x19, 0xe7, 0x22, 0xd4, 0xc5, 0x54, 0x92, 0xd3,
                    0xd8, 0xd1, 0xc2, 0xca, 0x05, 0x5b, 0x45, 0xdc
            };

            Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
            encryptor.Key = key;
            encryptor.IV = iv;
            encryptor.Padding = PaddingMode.PKCS7;

            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

            string cipherText = string.Empty;

            try
            {
                byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();

                byte[] cipherBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();

                cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
            }

            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }

            return cipherText;
        }

        public static string HashPassword(string password)
        {
            SHA512 hasher = SHA512.Create();
            byte[] hashedPassword = hasher.ComputeHash(Encoding.ASCII.GetBytes(password));
            return Convert.ToBase64String(hashedPassword);
        }
    }
}
