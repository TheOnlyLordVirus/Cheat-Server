using CheatServer.Database;
using CheatServer.Transports;
using CheatServer.Transports.Response;
using CheatServer.Utilitys.Security;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

namespace CheatServer.Controllers;

[ApiController]
[Route("Api/Charlie")]
public sealed class CheatsController : ControllerBase
{
    private static readonly Dictionary<CheatCommand, Func<DatabaseContext, CheatRequest, CancellationToken, Task<Response<CheatResponse>>>>
        CommandFunctions;

    static CheatsController()
    {
        CommandFunctions = new()
        {
            { CheatCommand.GetCheat, GetCheat }
        };
    }

    /// <summary>
    /// Post command input parser.
    /// </summary>
    [HttpPost("Input")]
    public async Task<IActionResult> CheatRequestManager(
        [FromServices] DatabaseContext databaseContext,
        [FromBody] EncryptedRequest request,
        CancellationToken cancellationToken)
    {
        ushort statusCode = 500;
        EncryptedResponse? response = default;
        Response<CheatResponse>? cheatResponse = default;
        Request<CheatRequest>? cheatRequest = default;

        try
        {
            cheatRequest = new Request<CheatRequest>(request.EncryptedData);
            cheatRequest = await cheatRequest.DecryptAsync(cancellationToken)
                .ConfigureAwait(false);

            if (cheatRequest.HasErrors)
                throw new Exception("Failed to decrypt request!");

            cheatResponse = await CommandFunctions[cheatRequest!.RequestObject!.Command]
                (databaseContext, cheatRequest!.RequestObject!, cancellationToken);

            if (cheatResponse.HasErrors)
                throw new Exception("Response failed from server!");

            cheatResponse = await cheatResponse.EncyrptAsync(cancellationToken)
                .ConfigureAwait(false);

            if (cheatResponse.HasErrors)
                throw new Exception("Response failed from server!");

            response = new EncryptedResponse()
            {
                EncryptedData = cheatResponse.EncryptedData ?? string.Empty
            };

            statusCode = 200;
        }

        catch (Exception ex)
        {
            statusCode = 418;
            return StatusCode(statusCode, cheatResponse);
        }

        finally
        {
            // Log request and log any errors in the database to monitor what data people are sending.
        }

        return StatusCode(statusCode, response);
    }

    private async static Task<Response<CheatResponse>> GetCheat(
        DatabaseContext databaseContext,
        CheatRequest request,
        CancellationToken cancellationToken)
    {
        Response<CheatResponse>? cheatResponse = default;
        string[] errors = Array.Empty<string>();

        try
        {
            if (request.UserId == Guid.Empty ||
                request.GameId == Guid.Empty)
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

            var userCheatData = await databaseContext
                .UserCheats
                    .Include (x => x.Game)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.GameId == request.GameId, cancellationToken)
                .ConfigureAwait(false);

            if (userCheatData is null ||
                userCheatData.AuthenticationEndDate < DateTime.Now)
                throw new Exception("This user does not have privileged access to this cheat!");


            var cheatData = await databaseContext
                .CheatBinaries
                .AsNoTracking()
                .FirstOrDefaultAsync(
                x => x.GameId == userCheatData.GameId && 
                x.AccessLevelId == userCheatData.AccessLevelId)
                .ConfigureAwait(false);

            if (cheatData is null)
                throw new Exception("No cheat for this game with this access level is available at this time! Please contact a cheat administrator to resolve this issue!");

            cheatResponse = new Response<CheatResponse>(new CheatResponse()
            {
                GameId = userCheatData.GameId,
                CheatBinaryId = cheatData.CheatId,
                CheatBinary = cheatData.Cheat, // Base64 binary
                GameProcessName = userCheatData.Game.GameProcessName,
                GameName = userCheatData.Game.GameName,
                GameVersion = userCheatData.Game.GameVersion
            });
        }

        catch (Exception ex)
        {
            errors = new string[]
            {
                string.Concat("Message: ", ex.Message)
            };

            cheatResponse = new Response<CheatResponse>(errors);
        }

        return cheatResponse;
    }
}
