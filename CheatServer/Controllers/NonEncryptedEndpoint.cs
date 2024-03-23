using CheatServer.Database;
using CheatServer.Transports;
using CheatServer.Utilitys.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheatServer.Controllers;

[ApiController]
[Route("Api/Debug")]
public class NonEncryptedEndpoint : ControllerBase
{
    [HttpPost("User/Create")]
    public async Task<IActionResult> CreateUser(
        [FromServices] DatabaseContext databaseContext,
        [FromBody] UserRequest request,
        CancellationToken cancellationToken)
    {
        ushort statusCode = 500;
        Response<UserResponse>? userResponse = default;
        string[] errors = Array.Empty<string>();

        try
        {
            if (String.IsNullOrEmpty(request.Name) ||
                String.IsNullOrEmpty(request.Email) ||
                String.IsNullOrEmpty(request.Password))
                throw new Exception("One or more of the requested feilds were null.");

            if (HttpContext.Connection.RemoteIpAddress is null)
                throw new Exception("Unable to retrieve remote ip address!");

            string ipAddress = HttpContext.Connection.RemoteIpAddress!.ToString();

            User? checkEntity = await databaseContext
                .Users
                .FirstOrDefaultAsync(x => x.Name == request.Name, cancellationToken)
                .ConfigureAwait(false);

            if (checkEntity is not null)
                throw new Exception("This username is already taken!");

            checkEntity = await databaseContext
                .Users
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken)
                .ConfigureAwait(false);

            if (checkEntity is not null)
                throw new Exception("This user email is already taken!");

            if (!Security.HashPassword(request.Password, out string passwordHash))
                throw new Exception("Failed to hash password!");

            User newUser = new User()
            {
                Name = request.Name,
                Email = request.Email,
                Password = passwordHash,
                CreationDate = DateTime.UtcNow,
                RegistrationIp = ipAddress,
                RecentIp = ipAddress,
                Active = true,
                HardwareId = request.HardwareId,
                Admin = false
            };

            databaseContext.Add(newUser);

            int savedResults = await databaseContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            if (savedResults != 1)
                throw new Exception("Failed to save user info!");

            User? newEntity = await databaseContext
                .Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == request.Name, cancellationToken)
                .ConfigureAwait(false);

            if (newEntity is null)
                throw new Exception("Failed to retrieve new entity!");

            userResponse = new Response<UserResponse>(new UserResponse()
            {
                Name = newEntity.Name,
                Email = newEntity.Email,
                Active = newEntity.Active,
                Admin = newEntity.Admin,
                CreationDate = newEntity.CreationDate,
                RecentIp = newEntity.RecentIp,
                RegistrationIp = newEntity.RegistrationIp
            });
        }

        catch (Exception ex)
        {
            statusCode = 404;

            errors =
            [
                string.Concat("Message: ", ex.Message)
            ];

            userResponse = new Response<UserResponse>(errors);
        }

        return StatusCode(200, userResponse);
    }

    [HttpPost("User/Authenticate")]
    public async Task<IActionResult> AuthenticateUser(
        [FromServices] DatabaseContext databaseContext,
        [FromBody] UserRequest request,
        CancellationToken cancellationToken)
    {
        Response<UserResponse>? userResponse = default;
        string[] errors = Array.Empty<string>();

        try
        {
            if (String.IsNullOrEmpty(request.Name) ||
                String.IsNullOrEmpty(request.Password))
                throw new Exception("One or more of the requested feilds were null.");

            if (!Security.HashPassword(request.Password, out string passwordHash))
                throw new Exception("Failed to hash password!");

            User? checkEntity = await databaseContext
                .Users
                .FirstOrDefaultAsync(x => x.Name == request.Name, cancellationToken)
                .ConfigureAwait(false);

            if (checkEntity is null)
                throw new Exception("This username is Doesn't exist!");

            checkEntity = await databaseContext
                .Users
                .FirstOrDefaultAsync(x => x.Password == passwordHash, cancellationToken)
                .ConfigureAwait(false);

            if (checkEntity is null)
                throw new Exception("Invalid password for this username.");

            User? userEntity = await databaseContext
                .Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == request.Name && x.Password == passwordHash, cancellationToken)
                .ConfigureAwait(false);

            if (userEntity is null)
                throw new Exception("Failed to retrieve the user entity!");

            userResponse = new Response<UserResponse>(new UserResponse()
            {
                Name = userEntity.Name,
                Email = userEntity.Email,
                Active = userEntity.Active,
                Admin = userEntity.Admin,
                CreationDate = userEntity.CreationDate,
                RecentIp = userEntity.RecentIp,
                RegistrationIp = userEntity.RegistrationIp
            });
        }

        catch (Exception ex)
        {
            errors = new string[]
            {
                    string.Concat("Message: ", ex.Message)
            };

            userResponse = new Response<UserResponse>(errors);
        }

        return StatusCode(200, userResponse);
    }
}