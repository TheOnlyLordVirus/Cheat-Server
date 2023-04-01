using Microsoft.AspNetCore.Mvc;
using CheatServer.Utilitys.Security;
using Microsoft.EntityFrameworkCore;
using CheatServer.Database;
using CheatServer.Transports;

namespace CheatServer.Controllers
{
    [ApiController]
    [Route("Api/Alpha")]
    public sealed class UserController : ControllerBase
    {
        private readonly Dictionary<UserCommand, Func<DatabaseContext, UserRequest, CancellationToken, Task<Response<UserResponse>>>> 
            CommandFunctions;

        public UserController()
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
            [FromBody] Request request,
            CancellationToken cancellationToken)
        {
            ushort statusCode = 500;
            Response<UserResponse> response = new Response<UserResponse>();

            try
            {
                if (!request.TryDecrypt(out UserRequest? serializedRequest))
                {
                    statusCode = 418;
                    throw new Exception("Invalid Request!");
                }

                response = await Task.Run(() => CommandFunctions[serializedRequest!.UserCommand](databaseContext, serializedRequest!, cancellationToken));

                if (response.HasErrors)
                    throw new Exception("One or more errors have been thrown while executing command!");

                statusCode = 200;
            }

            catch (Exception ex)
            {
                List<string> errorAppender = new List<string>();

                errorAppender.Add(Security.EncryptString(string.Concat("Command parser exception: ", ex.Message)));

                if(response.ErrorMessages is not null)
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


        private async Task<Response<UserResponse>> CreateUser(
            DatabaseContext databaseContext,
            UserRequest request,
            CancellationToken cancellationToken)
        {
            Response<UserResponse> userResponse = new Response<UserResponse>();
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

                User newUser = new User()
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = Security.HashPassword(request.Password),
                    CreationDate = DateTime.UtcNow,
                    RegistrationIp = ipAddress,
                    RecentIp = ipAddress,
                    Active = true,
                    HardwareId = request.HardwareId,
                    Admin = false
                };

                await databaseContext
                    .AddAsync(newUser, cancellationToken)
                    .ConfigureAwait(false);

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

        private async Task<Response<UserResponse>> AuthenticateUser(
            DatabaseContext databaseContext,
            UserRequest request,
            CancellationToken cancellationToken)
        {
            Response<UserResponse> userResponse = new Response<UserResponse>();
            string[] errors = Array.Empty<string>();

            try
            {
                if (String.IsNullOrEmpty(request.Name) ||
                    String.IsNullOrEmpty(request.Password))
                    throw new Exception("One or more of the requested feilds were null.");

                string passwordHash = Security.HashPassword(request.Password);

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