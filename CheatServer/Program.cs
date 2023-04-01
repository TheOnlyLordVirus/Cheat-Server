using CheatServer.Database;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load configuration.
builder.Host.ConfigureAppConfiguration
(
    options =>
    {
        options.AddJsonFile("appsettings.json");

        #if DEBUG
            options.AddUserSecrets<Program>();
        #endif
    }
);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<ConfigurationManager>(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
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

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


