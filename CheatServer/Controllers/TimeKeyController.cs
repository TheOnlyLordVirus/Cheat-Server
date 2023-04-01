using Microsoft.AspNetCore.Mvc;
using CheatServer.Utilitys.Security;
using Microsoft.EntityFrameworkCore;
using CheatServer.Database;
using CheatServer.Transports;

namespace CheatServer.Controllers
{
    [ApiController]
    [Route("Api/Beta")]
    public sealed class TimeKeyController : ControllerBase
    {
        private readonly Dictionary<TimeKeyCommand, Func<DatabaseContext, TimeKeyRequest, CancellationToken, Task<Response<TimeKeyResponse>>>>
            CommandFunctions;

        public TimeKeyController()
        {
            CommandFunctions = new ()
            {
                { TimeKeyCommand.Create, CreateNewKey },
                { TimeKeyCommand.Redeem, RedeemKey }
            };
        }

        /// <summary>
        /// Post command input parser.
        /// </summary>
        [HttpPost("Input")]
        public async Task<IActionResult> UserRequestManager(
            [FromServices] DatabaseContext databaseContext,
            [FromBody] Request request,
            CancellationToken cancellationToken)
        {
            ushort statusCode = 500;
            Response<TimeKeyResponse> response = new Response<TimeKeyResponse>();

            try
            {
                if (!request.TryDecrypt(out TimeKeyRequest? serializedRequest))
                {
                    statusCode = 418;
                    throw new Exception("Invalid Request!");
                }

                response = await Task.Run(() => CommandFunctions[serializedRequest!.Command](databaseContext, serializedRequest!, cancellationToken));

                if (response.HasErrors)
                    throw new Exception("One or more errors have been thrown while executing command!");

                statusCode = 200;
            }

            catch (Exception ex)
            {
                List<string> errorAppender = new List<string>();

                errorAppender.Add(Security.EncryptString(string.Concat("Command parser exception: ", ex.Message)));

                if (response.ErrorMessages is not null)
                    errorAppender.AddRange(response.ErrorMessages);

                response.ErrorMessages = errorAppender.ToArray();

                return StatusCode(statusCode, response);
            }

            finally
            {
                // Log request and log any errors in the database to monitor what data people are sending.
            }

            return StatusCode(statusCode, response);
        }


        private async Task<Response<TimeKeyResponse>> CreateNewKey(
            DatabaseContext databaseContext,
            TimeKeyRequest request,
            CancellationToken cancellationToken)
        {
            Response<TimeKeyResponse> timeKeyResponse = new Response<TimeKeyResponse>();
            string[] errors = Array.Empty<string>();

            try
            {
                if (String.IsNullOrEmpty(request.UserId) ||
                    String.IsNullOrEmpty(request.GameId))
                    throw new Exception("One or more of the requested feilds were null.");


                timeKeyResponse = new Response<TimeKeyResponse>(new TimeKeyResponse()
                {
                    
                });
            }

            catch (Exception ex)
            {
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message)
                };

                timeKeyResponse = new Response<TimeKeyResponse>(errors);
            }

            return timeKeyResponse;
        }

        private async Task<Response<TimeKeyResponse>> RedeemKey(
            DatabaseContext databaseContext,
            TimeKeyRequest request,
            CancellationToken cancellationToken)
        {
            Response<TimeKeyResponse> timeKeyResponse = new Response<TimeKeyResponse>();
            string[] errors = Array.Empty<string>();

            try
            {
                if (String.IsNullOrEmpty(request.UserId) ||
                    String.IsNullOrEmpty(request.Key))
                    throw new Exception("One or more of the requested feilds were null.");



                timeKeyResponse = new Response<TimeKeyResponse>(new TimeKeyResponse()
                {

                });
            }

            catch (Exception ex)
            {
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message)
                };

                timeKeyResponse = new Response<TimeKeyResponse>(errors);
            }

            return timeKeyResponse;
        }
    }
}
