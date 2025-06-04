using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizAppBackend.Data;
using QuizAppBackend.Hubs;
using QuizAppBackend.Models;
using QuizAppBackend.Services;
using System.Text;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using QuizAppBackend.Middleware; // Added for ExceptionMiddleware
// No need for: using Microsoft.AspNetCore.SignalR;
// No need for: using Microsoft.AspNetCore.Http.Connections;

var builder = WebApplication.CreateBuilder(args);

// Lägg till tjänster till containern.

// Konfigurera Entity Framework Core med databasen
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfigurera Identity för användarhantering
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Konfigurera JWT-autentisering
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // true i produktion, men MAUI apps often connect locally
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        // Fixed CS8604: Use null-forgiving operator as we expect these to be configured in appsettings.json
        ValidAudience = builder.Configuration["JwtSettings:Audience"]!,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"]!,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
    };
    // Konfigurera SignalR att använda JWT
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Om begäran kommer från SignalR-hubben
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/gamehub")))
            {
                // Lägg till token till kontexten
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Configure a single, comprehensive CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowQuizAppClients",
        policy =>
        {
            // IMPORTANT: For development, use the actual URL/IP of your MAUI app.
            // When deploying, list ONLY the specific URLs where your MAUI app will connect from.
            // Example for MAUI running on emulator/local device:
            policy.WithOrigins(
                "http://localhost:5000", // For development on Windows machine/emulator
                "http://192.168.0.49:5000" // Example: Replace with your actual development device IP (e.g., your computer's local IP)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // REQUIRED for SignalR with authentication (for sending cookies/auth headers)
        });
});


// Lägg till Controllers
builder.Services.AddControllers();

// Lägg till Swagger/OpenAPI för att utforska API:t
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Lägg till dina egna tjänster
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FriendService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<UserService>();

// Lägg till SignalR
builder.Services.AddSignalR();


var app = builder.Build();

// Konfigurera HTTP Request Pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Global exception handling middleware for production
    app.UseMiddleware<ExceptionMiddleware>();
}

app.UseHttpsRedirection();

// Add UseRouting *before* CORS, Authentication, Authorization
app.UseRouting();

// Apply the comprehensive CORS policy for all API and SignalR endpoints
app.UseCors("AllowQuizAppClients");

app.UseAuthentication();
app.UseAuthorization();

// Mappa controllers
app.MapControllers();

// Mappa SignalR-hubben (no specific CORS configuration here anymore, it's handled by UseCors)
app.MapHub<GameHub>("/gamehub");

app.Run();