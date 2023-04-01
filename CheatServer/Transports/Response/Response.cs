using CheatServer.Utilitys.Security;
using Newtonsoft.Json;

namespace CheatServer.Transports
{
    public class Response<TResponse>
    {
        public string ResponseData { get; set; } = string.Empty;

        public bool IsSuccessful => HasErrors == false;

        public bool HasErrors { get; set; }

        public string[]? ErrorMessages { get; set; }

        public Response(TResponse responseData)
        {
            HasErrors = !TryEncyrpt(responseData, out string[] errors, out string encryptedResponse);
            ErrorMessages = EncryptErrors(errors);

            if(!HasErrors)
                ResponseData = encryptedResponse;
        }

        public Response(string[]? errors = null)
        {
            HasErrors = (errors?.Length ?? 0) > 0;

            if(errors is not null)
                ErrorMessages = EncryptErrors(errors);
        }

        private static bool TryEncyrpt(TResponse unencryptedResponse, out string[] errors, out string encryptedResponse)
        {
            encryptedResponse = string.Empty;
            bool result = true;
            errors = Array.Empty<string>();

            try
            {
                string serializedResponse = JsonConvert.SerializeObject(unencryptedResponse);
                encryptedResponse = Security.EncryptString(serializedResponse);

                if (encryptedResponse.Equals(string.Empty))
                    result = false;
            }

            catch (Exception ex)
            {
                result = false;
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message),
                    string.Concat("Inner Exception: ", ex.InnerException)
                };
            }

            finally
            {
                // Log request and log any errors in the database to monitor what data people are sending.
            }

            return result;
        }


        private static string[] EncryptErrors(string[] errors)
        {
            for(int i = 0; i < errors.Count(); ++i)
            {
                errors[i] = Security.EncryptString(errors[i]);
            }

            return errors;
        }
    }
}
