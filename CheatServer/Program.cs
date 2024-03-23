using Microsoft.EntityFrameworkCore;

using CheatServer.Database;

var builder = WebApplication.CreateBuilder(args);

// Load configuration.
builder.Configuration.AddJsonFile("appsettings.json").AddUserSecrets<Program>();

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<ConfigurationManager>(builder.Configuration);
builder.Services.AddDbContext<DatabaseContext>
(   
    options =>
    {
        options.UseMySql
        (
            builder.Configuration["ConnectionString"],
            ServerVersion.AutoDetect(builder.Configuration["ConnectionString"])
        );
    }
);

//var httpClientHandler = new HttpClientHandler();
//httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => { return true; };
//builder.Services.AddScoped(sp => new HttpClient(httpClientHandler));

var app = builder.Build();

app.MapControllers();

app.Run();


// Override automatic validation of SSL server certificates.
//ServicePointManager.ServerCertificateValidationCallback =
//           ValidateServerCertficate;

///// <summary>
///// Validates the SSL server certificate.
///// </summary>
///// <param name="sender">An object that contains state information for this
///// validation.</param>
///// <param name="cert">The certificate used to authenticate the remote party.</param>
///// <param name="chain">The chain of certificate authorities associated with the
///// remote certificate.</param>
///// <param name="sslPolicyErrors">One or more errors associated with the remote
///// certificate.</param>
///// <returns>Returns a boolean value that determines whether the specified
///// certificate is accepted for authentication; true to accept or false to
///// reject.</returns>
//static bool ValidateServerCertficate(
//        object sender,
//        X509Certificate? cert,
//        X509Chain? chain,
//        SslPolicyErrors sslPolicyErrors)
//{
//    if (sslPolicyErrors == SslPolicyErrors.None)
//    {
//        // Good certificate.
//        return true;
//    }

//    log.DebugFormat("SSL certificate error: {0}", sslPolicyErrors);

//    bool certMatch = false; // Assume failure
//    byte[] certHash = cert.GetCertHash();
//    if (certHash.Length == apiCertHash.Length)
//    {
//        certMatch = true; // Now assume success.
//        for (int idx = 0; idx < certHash.Length; idx++)
//        {
//            if (certHash[idx] != apiCertHash[idx])
//            {
//                certMatch = false; // No match
//                break;
//            }
//        }
//    }

//    // Return true => allow unauthenticated server,
//    //        false => disallow unauthenticated server.
//    return certMatch;
//}