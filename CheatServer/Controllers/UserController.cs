using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using CheatServer.Utilitys.Security;
using CheatServer.Database;
using CheatServer.Transports;

namespace CheatServer.Controllers
{
    [ApiController]
    [Route("Api/Alpha")]
    public sealed class UserController : ControllerBase
    {
        private delegate Task<Response<UserResponse>> UserCommandDelegate(
            UserController userController, 
            DatabaseContext databaseContext, 
            UserRequest userRequest, 
            CancellationToken cancellationToken);

        private static readonly Dictionary<UserCommand, UserCommandDelegate> 
            CommandFunctions;

        static UserController()
        {
            CommandFunctions = new ()
            {
                { UserCommand.Create, CreateUser },
                { UserCommand.Authenticate, AuthenticateUser }
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
            Response<UserResponse>? userResponse = default;
            Request<UserRequest>? userRequest = default;

            try
            {
                userRequest = new Request<UserRequest>(request.EncryptedData);
                userRequest = await userRequest.DecryptAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (userRequest.HasErrors)
                    throw new Exception("Failed to decrypt request!");

                userResponse = await CommandFunctions[userRequest!.RequestObject!.UserCommand]
                    (this, databaseContext, userRequest!.RequestObject!, cancellationToken);

                if (userResponse.HasErrors)
                    throw new Exception("Response failed from server!");

                userResponse = await userResponse.EncyrptAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (userResponse.HasErrors)
                    throw new Exception("Response failed from server!");

                response = new EncryptedResponse()
                {
                    EncryptedData = userResponse.EncryptedData ?? string.Empty
                };

                statusCode = 200;
            }

            catch (Exception ex)
            {
                statusCode = 418;
                return StatusCode(statusCode, string.Empty);
            }

            finally
            {
                // Log request and log any errors in the database to monitor what data people are sending.
            }

            return StatusCode(statusCode, response);
        }


        private static async Task<Response<UserResponse>> CreateUser(
            UserController controller,
            DatabaseContext databaseContext,
            UserRequest request,
            CancellationToken cancellationToken)
        {
            Response<UserResponse>? userResponse = default;
            string[] errors = Array.Empty<string>();

            try
            {
                if (String.IsNullOrEmpty(request.Name) ||
                    String.IsNullOrEmpty(request.Email) ||
                    String.IsNullOrEmpty(request.Password))
                    throw new Exception("One or more of the requested feilds were null.");

                if (controller.HttpContext.Connection.RemoteIpAddress is null)
                    throw new Exception("Unable to retrieve remote ip address!");

                string ipAddress = controller.HttpContext.Connection.RemoteIpAddress!.ToString();

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
                errors = new string[]
                {
                    string.Concat("Message: ", ex.Message)
                };

                userResponse = new Response<UserResponse>(errors);
            }

            return userResponse;
        }

        private static async Task<Response<UserResponse>> AuthenticateUser(
            UserController controller,
            DatabaseContext databaseContext,
            UserRequest request,
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

            return userResponse;
        }
    }
}