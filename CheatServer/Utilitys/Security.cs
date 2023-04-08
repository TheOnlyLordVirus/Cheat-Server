using CheatServer.Transports;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CheatServer.Utilitys.Security
{
    public static class Security
    {
        /*
            'a',  'b',  'c',  'd',  'e',  'f',  'g',  'h',  'i',  'j',  'k',  'l',  'm',  'n',  'o',  'p',  'q',
            'r',  's',  't',  'u',  'v',  'w',  'x',  'y',  'z',  'A',  'B',  'C',  'D',  'E',  'F',  'G',  'H',
            'I',  'J',  'K',  'L',  'M',  'N',  'O',  'P',  'Q',  'R',  'S',  'T',  'U',  'V',  'W',  'X',  'Y',
            'Z',  '~',  '`',  '!',  '@',  '#',  '$',  '%',  '^',  '&',  '*',  '(',  ')',  '_',  '-',  '+',  '=',
            '{',  '[',  '}',  ']',  '|',  '\\', ':',  ';',  '"',  '\'', '<',  ',',  '>',  '.',  '?',  '/'
        */
        private static readonly Dictionary<char, char> _substitutionCypherChars = new Dictionary<char, char>
        {
            { 'a', '=' }, { 'b', 'e' }, { 'c', '^' },  { 'd', 'B' },  { 'e', 'R' },
            { 'f', 'n' }, { 'g', 'f' }, { 'h', 'P' },  { 'i', '$' },  { 'j', '>' },
            { 'k', 'z' }, { 'l', ']' }, { 'm', ';' },  { 'n', 'd' },  { 'o', 'u' },
            { 'p', '%' }, { 'q', ',' }, { 'r', '|' },  { 's', 'F' },  { 't', 'm' },
            { 'u', 'Z' }, { 'v', 'Q' }, { 'w', '}' },  { 'x', 'i' },  { 'y', 't' },
            { 'z', 'N' }, { 'A', 'W' }, { 'B', 'c' },  { 'C', '-' },  { 'D', '+' },
            { 'E', '[' }, { 'F', 'x' }, { 'G', '@' },  { 'H', '~' },  { 'I', '&' },
            { 'J', 'p' }, { 'K', '_' }, { 'L', '{' },  { 'M', '\'' }, { 'N', '*' },
            { 'O', '"' }, { 'P', 'G' }, { 'Q', 'g' },  { 'R', '<' },  { 'S', 'M' },
            { 'T', 'X' }, { 'U', 'S' }, { 'V', 'y' },  { 'W', '!' },  { 'X', 'r' },
            { 'Y', 'J' }, { 'Z', '?' }, { '~', 'C' },  { '`', 'L' },  { '!', 'v' },
            { '@', 'V' }, { '#', 'A' }, { '$', 'w' },  { '%', ')' },  { '^', 'E' },
            { '&', 'a' }, { '*', 'q' }, { '(', 'o' },  { ')', 'b' },  { '_', 'Y' },
            { '-', '`' }, { '+', 'h' }, { '=', '\\' }, { '{', '(' },  { '[', ':' },
            { '}', 'T' }, { ']', 'j' }, { '|', 'H' },  { '\\', '#' }, { ':', 'D' },
            { ';', 'K' }, { '"', '/' }, { '\'', 'O' }, { '<', 'I' },  { ',', 'U' },
            { '>', 'k' }, { '.', 's' }, { '?', 'l' },  { '/', '.' },
        };

        private static readonly Aes _aes = Aes.Create();
        private static readonly SHA256 _sha256 = SHA256.Create();
        private static readonly SHA512 _passwordHasher = SHA512.Create();

        private static readonly ICryptoTransform _aesEncryptor = _aes.CreateEncryptor();
        private static readonly ICryptoTransform _aesDecryptor = _aes.CreateDecryptor();

        // TODO: Get and set encryption and decryption key from from RSA Key.
        private static readonly string _encryptionKey = "Cba321";
        private static readonly byte[] _encryptIV = new byte[32]
        {
            0x19, 0xe7, 0x22, 0xd4, 0xc5, 0x54, 0x92, 0xd3,
            0xd8, 0xd1, 0xc2, 0xca, 0x05, 0x5b, 0x45, 0xdc,
            0xce, 0x39, 0xdc, 0x0e, 0x2d, 0x05, 0x6a, 0xb4,
            0x64, 0x67, 0xbc, 0x32, 0xf8, 0x06, 0xf3, 0xeb
        };

        private static readonly string _decryptionKey = "Abc123";
        private static readonly byte[] _decryptIV = new byte[32]
        {
            0x86, 0xc0, 0x71, 0x97, 0x16, 0x9a, 0x25, 0x7b,
            0x70, 0xf0, 0x9c, 0x3e, 0x8f, 0x61, 0x4d, 0xb4,
            0xbf, 0xdc, 0xb7, 0xdb, 0x7e, 0xc6, 0x87, 0x06, 
            0x0e, 0xfd, 0x90, 0xae, 0x22, 0x5a, 0xc0, 0x0a
        };

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

        public static string DecryptString(string encryptedText)
        {
            string removedHmac = Task.Run(async () => await Internal_DecryptHMAC(encryptedText)).Result;
            string decrypted = Task.Run(async () => await Internal_AesDecryptString(encryptedText)).Result;

            return decrypted;
        }

        public static string HashPassword(string password)
        {
            byte[] hashedPassword = _passwordHasher
                .ComputeHash
                (
                    Encoding.UTF8.GetBytes
                    (
                        Internal_SubstitutionCypher
                        (
                            Internal_FilterChars(password)
                        )
                    )
                );

            return Convert.ToBase64String(hashedPassword);
        }

        private static async Task<string> Internal_AesDecryptString(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            string decryptedText;

            byte[] key = _sha256.ComputeHash(Encoding.ASCII.GetBytes(_decryptionKey));
            _aes.Mode = CipherMode.CBC;
            _aes.Key = key;
            _aes.IV = _decryptIV;
            _aes.Padding = PaddingMode.PKCS7;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, _aesDecryptor, CryptoStreamMode.Write);

            try
            {
                await cryptoStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
                await cryptoStream.FlushFinalBlockAsync();

                byte[] decryptedBytes = memoryStream.ToArray();

                decryptedText = Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
            }

            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }

            return decryptedText;
        }

        private static async Task<string> Internal_AesEncryptString(string plainText)
        {
            string encryptedText;

            byte[] key = _sha256.ComputeHash(Encoding.ASCII.GetBytes(_encryptionKey));
            _aes.Mode = CipherMode.CBC;
            _aes.Key = key;
            _aes.IV = _encryptIV;
            _aes.Padding = PaddingMode.PKCS7;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, _aesEncryptor, CryptoStreamMode.Write);

            try
            {
                byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);
                await cryptoStream.WriteAsync(plainBytes, 0, plainBytes.Length);
                await cryptoStream.FlushFinalBlockAsync();

                byte[] cipherBytes = memoryStream.ToArray();

                encryptedText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
            }

            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }

            return encryptedText;
        }

        private static async Task<string> Internal_EncryptHMAC()
        {
            return string.Empty;
        }

        private static async Task<string> Internal_DecryptHMAC(string cypherText)
        {
            return string.Empty;
        }

        private static string Internal_FilterChars(ReadOnlySpan<char> text)
        {
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < text.Length; i++)
            {
                if (_substitutionCypherChars.ContainsKey(text[i]))
                    sb.Append(text[i]);
            }

            return sb.ToString();
        }

        private static string Internal_SubstitutionCypher(ReadOnlySpan<char> text)
        {
            StringBuilder sb = new StringBuilder();

            foreach(char c in text)
            {
                sb.Append(_substitutionCypherChars[c]);
            }

            return sb.ToString();
        }
    }
}
