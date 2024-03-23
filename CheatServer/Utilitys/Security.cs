using CheatServer.Transports;

using Newtonsoft.Json;

using System.Security.Cryptography;
using System.Text;

namespace CheatServer.Utilitys.Security
{
    public static class Security
    {
        // TODO: add numbers to this, add more chars to each value.
        private static readonly Dictionary<char, string> _substitutionCypherChars = new() 
        {
            { 'a', "=A" }, { 'b', "e" }, { 'c', "^" },  { 'd', "Bsa" },  { 'e', "R" },
            { 'f', "n2" }, { 'g', "f" }, { 'h', "P" },  { 'i', "$" },  { 'j', ">" },
            { 'k', "z32" }, { 'l', "]" }, { 'm', ";" },  { 'n', "d" },  { 'o', "u" },
            { 'p', "%a" }, { 'q', ",ff" }, { 'r', "|" },  { 's', "F" },  { 't', "m" },
            { 'u', "Z43" }, { 'v', "Q" }, { 'w', "}" },  { 'x', "i" },  { 'y', "t" },
            { 'z', "N00" }, { 'A', "W" }, { 'B', "cw" },  { 'C', "-" },  { 'D', "+e" },
            { 'E', "[9a" }, { 'F', "i7%x" }, { 'G', "@" },  { 'H', "~" },  { 'I', "&" },
            { 'J', "p5" }, { 'K', "_" }, { 'L', "{" },  { 'M', "\'" }, { 'N', "*" },
            { 'O', "\"\"a" }, { 'P', "G" }, { 'Q', "g" },  { 'R', "<" },  { 'S', "Mr" },
            { 'T', "Xd" }, { 'U', "S" }, { 'V', "y" },  { 'W', "!" },  { 'X', "rh" },
            { 'Y', "J" }, { 'Z', "?" }, { '~', "C" },  { '`', "Lf" },  { '!', "v" },
            { '@', "V" }, { '#', "Al" }, { '$', "wa" },  { '%', ")7" },  { '^', "E" },
            { '&', "a" }, { '*', "ql" }, { '(', "o" },  { ')', "b" },  { '_', "Y" },
            { '-', "`" }, { '+', "hnop" }, { '=', "\\" }, { '{', "(" },  { '[', ":%" },
            { '}', "jT" }, { ']', "j" }, { '|', "H" },  { '\\', "3d#" }, { ':', "#D" },
            { ';', "K" }, { '"', "/j" }, { '\'', "hO" }, { '<', "I" },  { ',', "U" },
            { '>', "kg" }, { '.', "s" }, { '?', "l" },  { '/', "." },
            { '0', "dX6" }, { '1', "f3" }, { '2', "#k" }, { '3', "l2" }, { '4', "7jnh" }, 
            { '5', "Vb7" }, { '6', "1!2" }, { '7', "]\\6" }, { '8', "%3d" }, { '9', "h3@3" }
        };

        private static readonly Aes _aes = Aes.Create();
        private static readonly SHA256 _sha256 = SHA256.Create();
        private static readonly SHA512 _passwordHasher = SHA512.Create();

        // TODO: Get this from IConfiguration?
        private static readonly string _encryptionKey = "Cba321";
        private static readonly byte[] _encryptIV = new byte[16]
        {
            0x19, 0xe7, 0x22, 0xd4, 0xc5, 0x54, 0x92, 0xd3,
            0xd8, 0xd1, 0xc2, 0xca, 0x05, 0x5b, 0x45, 0xdc
        };

        private static readonly string _decryptionKey = "Abc123";
        private static readonly byte[] _decryptIV = new byte[16]
        {
            0x86, 0xc0, 0x71, 0x97, 0x16, 0x9a, 0x25, 0x7b,
            0x70, 0xf0, 0x9c, 0x3e, 0x8f, 0x61, 0x4d, 0xb4
        };

        public static async Task<Response<TResponse>> EncyrptAsync<TResponse>(
            this Response<TResponse> unencryptedResponse, 
            CancellationToken cancellationToken = default)
        {
            Response<TResponse> encryptedResponse = unencryptedResponse;
            string encryptedResponseData = string.Empty;
            string[] errors = Array.Empty<string>();

            try
            {
                string serializedResponse = JsonConvert.SerializeObject(unencryptedResponse.ResponseObject);
                encryptedResponseData = await Internal_AesEncryptStringAsync(serializedResponse, cancellationToken)
                    .ConfigureAwait(false);

                if (encryptedResponse.Equals(string.Empty))
                    throw new Exception("Failed to serialize and encrypt this response!");

                encryptedResponse = new Response<TResponse>(encryptedResponseData);
            }

            catch (Exception ex)
            {
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message),
                    string.Concat("Inner Exception: ", ex.InnerException)
                };

                encryptedResponse = new Response<TResponse>(errors);
            }

            finally
            {
                // Log request and log any errors in the database to monitor what data people are sending.
            }

            return encryptedResponse;
        }

        public static async Task<Request<TRequest>> DecryptAsync<TRequest>(
            this Request<TRequest> encryptedRequest, 
            CancellationToken cancellationToken = default)
        {
            TRequest? unecryptedRequestObject = default;
            Request<TRequest>? unecryptedRequest = default;
            string[] errors = Array.Empty<string>();

            try
            {
                if (string.IsNullOrEmpty(encryptedRequest?.EncryptedData))
                    throw new Exception("The EncryptedRequest data was null or empty!");

                string unencryptedString = await Internal_AesDecryptStringAsync(encryptedRequest.EncryptedData!, cancellationToken)
                    .ConfigureAwait(false);

                unecryptedRequestObject = JsonConvert.DeserializeObject<TRequest?>(unencryptedString!);

                if (unecryptedRequestObject is null)
                    throw new Exception("Failed to deserialize and decrypt this request!");

                unecryptedRequest = new Request<TRequest>(unecryptedRequestObject);
            }

            catch(Exception ex)
            {
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message),
                    string.Concat("Inner Exception: ", ex.InnerException)
                };

                unecryptedRequest = new Request<TRequest>(errors);
            }

            finally
            {
                // Log request and log any errors in the database to monitor what data people are sending.
            }

            return unecryptedRequest;
        }

        private static async Task<string> Internal_AesDecryptStringAsync(string encryptedText, CancellationToken cancellationToken = default)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            string decryptedText = string.Empty;

            byte[] key = _sha256.ComputeHash(Encoding.ASCII.GetBytes(_decryptionKey));
            _aes.Mode = CipherMode.CBC;
            _aes.Key = key;
            _aes.IV = _decryptIV;
            _aes.Padding = PaddingMode.PKCS7;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, _aes.CreateDecryptor(), CryptoStreamMode.Write);

            try
            {
                await cryptoStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length, cancellationToken);
                await cryptoStream.FlushFinalBlockAsync(cancellationToken);

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

        private static async Task<string> Internal_AesEncryptStringAsync(string plainText, CancellationToken cancellationToken = default)
        {
            string encryptedText;

            byte[] key = _sha256.ComputeHash(Encoding.ASCII.GetBytes(_encryptionKey));
            _aes.Mode = CipherMode.CBC;
            _aes.Key = key;
            _aes.IV = _encryptIV;
            _aes.Padding = PaddingMode.PKCS7;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, _aes.CreateEncryptor(), CryptoStreamMode.Write);

            try
            {
                byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);
                await cryptoStream.WriteAsync(plainBytes, 0, plainBytes.Length, cancellationToken);
                await cryptoStream.FlushFinalBlockAsync(cancellationToken);

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

        public static bool HashPassword(string password, out string passwordHashAsBase64)
        {
            passwordHashAsBase64 = string.Empty;
            Span<byte> hashedPassword = stackalloc byte[64];

            if (!Internal_HashPassword(password, hashedPassword, out int bytesWritten))
                return false;

            hashedPassword = hashedPassword[..bytesWritten];

            passwordHashAsBase64 = Convert.ToBase64String(hashedPassword);

            return true;
        }

        private static void Internal_SubstitutionCypher(ReadOnlySpan<char> password, Span<char> passwordOutput, out int length)
        {
            int index = 0;
            for (var i = 0; i < password.Length; i++)
            {
                var current = password[i];
                if (!_substitutionCypherChars.ContainsKey(current))
                    continue;

                ReadOnlySpan<char> replaceValue = _substitutionCypherChars[current];
                replaceValue.CopyTo(passwordOutput.Slice(index, replaceValue.Length));
                index += replaceValue.Length;
            }

            length = index;
        }

        private static bool Internal_HashPassword(string password, Span<byte> result, out int bytesWritten)
        {
            Span<char> outputPassword = stackalloc char[128];
            Span<byte> outputBytes = stackalloc byte[128];

            Internal_SubstitutionCypher(password, outputPassword, out var length);

            var utfBytesWritten = Encoding.UTF8.GetBytes(outputPassword[..length], outputBytes);

            return _passwordHasher.TryComputeHash(outputBytes[..utfBytesWritten], result, out bytesWritten);
        }
    }
}
