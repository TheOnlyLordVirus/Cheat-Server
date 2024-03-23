using Microsoft.AspNetCore.Mvc;

using CheatServer.Utilitys.Security;
using CheatServer.Database;
using CheatServer.Transports;
using Microsoft.EntityFrameworkCore;

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
                //{ TimeKeyCommand.Create, CreateNewKey },
                { TimeKeyCommand.Redeem, RedeemKey }
            };
        }

        /// <summary>
        /// Post command input parser.
        /// </summary>
        [HttpPost("Input")]
        public async Task<IActionResult> UserRequestManager(
            [FromServices] DatabaseContext databaseContext,
            [FromBody] EncryptedRequest request,
            CancellationToken cancellationToken)
        {
            ushort statusCode = 500;
            EncryptedResponse? response = default;
            Response<TimeKeyResponse>? timeKeyResponse = default;
            Request<TimeKeyRequest>? timeKeyRequest = default;

            try
            {
                timeKeyRequest = new Request<TimeKeyRequest>(request.EncryptedData);
                timeKeyRequest = await timeKeyRequest.DecryptAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (timeKeyRequest.HasErrors)
                    throw new Exception("Failed to decrypt request!");

                timeKeyResponse = await CommandFunctions[timeKeyRequest!.RequestObject!.Command]
                    (databaseContext, timeKeyRequest!.RequestObject!, cancellationToken);

                if (timeKeyResponse.HasErrors)
                    throw new Exception("Response failed from server!");

                timeKeyResponse = await timeKeyResponse.EncyrptAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (timeKeyResponse.HasErrors)
                    throw new Exception("Response failed from server!");

                response = new EncryptedResponse()
                {
                    EncryptedData = timeKeyResponse.EncryptedData ?? string.Empty
                };

                statusCode = 200;
            }

            catch (Exception ex)
            {
                statusCode = 418;
                return StatusCode(statusCode, timeKeyResponse);
            }

            finally
            {
                // Log request and log any errors in the database to monitor what data people are sending.
            }

            return StatusCode(statusCode, response);
        }


        //private async Task<Response<TimeKeyResponse>> CreateNewKey(
        //    DatabaseContext databaseContext,
        //    TimeKeyRequest request,
        //    CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();

        //    Response<TimeKeyResponse>? timeKeyResponse = default;
        //    string[] errors = Array.Empty<string>();

        //    try
        //    {
        //        if (String.IsNullOrEmpty(request.UserId) ||
        //            String.IsNullOrEmpty(request.GameId))
        //            throw new Exception("One or more of the requested feilds were null.");

        //        timeKeyResponse = new Response<TimeKeyResponse>(new TimeKeyResponse()
        //        {
                    
        //        });
        //    }

        //    catch (Exception ex)
        //    {
        //        errors = new string[]
        //        {
        //            string.Concat("Message: ", ex.Message)
        //        };

        //        timeKeyResponse = new Response<TimeKeyResponse>(errors);
        //    }

        //    return timeKeyResponse;
        //}

        private async Task<Response<TimeKeyResponse>> RedeemKey(
            DatabaseContext databaseContext,
            TimeKeyRequest request,
            CancellationToken cancellationToken)
        {
            Response<TimeKeyResponse>? timeKeyResponse = default;
            string[] errors = Array.Empty<string>();

            try
            {
                if (request.UserId == Guid.Empty ||
                    String.IsNullOrEmpty(request.Key))
                    throw new Exception("One or more of the requested feilds were null.");


                var userResult = await databaseContext
                    .Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
                    .ConfigureAwait(false);

                if (userResult is null)
                    throw new Exception("This user id does not exist!");

                if (!Security.HashPassword(request.UserPassword, out string hashedPassword))
                    throw new Exception("Failed to hash user password!");

                if (userResult.Password != hashedPassword)
                    throw new Exception("Incorrect Password!");

                if (!userResult.Active)
                    throw new Exception("This user has been banned or is no longer an active user!");

                var timeKeyResult = await databaseContext
                    .TimeKeys
                    .Include(x => x.CheatBinary)
                        .ThenInclude(x => x.Game)
                    .FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken)
                    .ConfigureAwait(false);

                if (timeKeyResult is null)
                    throw new Exception("This key does not exist!");


                var userCheat = new UserCheat()
                {
                    GameId = timeKeyResult.CheatBinary.GameId,
                    AccessLevelId = timeKeyResult.CheatBinary.AccessLevelId,
                    UserId = request.UserId,
                    AuthenticationEndDate = DateTime.UtcNow + TimeSpan.FromDays(timeKeyResult.TimeValue),
                };

                timeKeyResult.Active = false;

                databaseContext.Add(userCheat);

                int rowsChanged = await databaseContext.SaveChangesAsync(cancellationToken);

                if (rowsChanged != 2)
                    throw new Exception("Failed to add this key to the database!");

                timeKeyResponse = new Response<TimeKeyResponse>(new TimeKeyResponse()
                {
                    Key = request.Key
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
