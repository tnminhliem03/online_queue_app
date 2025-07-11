using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using OnlineQueueAPI.Config;
using OnlineQueueAPI.Data;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Permission;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.DL.AccountDL;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//Thiết lập data
builder.Services.AddDbContext<OnlineQueueDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnectionMySQL"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnectionMySQL"))
    ));

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(30),
                QueueLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddSignalR();

//Thiết lập JWT Authentication
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

//Kiểm tra và lấy SecretKey từ biến môi trường
var secretKeyFromEnv = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (!string.IsNullOrEmpty(secretKeyFromEnv))
    jwtSettings.SecretKey = secretKeyFromEnv;

if (string.IsNullOrEmpty(jwtSettings.SecretKey))
    throw new Exception("JWT_SECRET_KEY is not set or missing in environment variables or configuration.");

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<WebSocketService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });


//Phân quyền
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddScoped(typeof(IBaseDL<>), typeof(BaseDL<>));
builder.Services.AddScoped<IOrganizationDL, OrganizationDL>();
builder.Services.AddScoped<IAccountDL, AccountDL>();
builder.Services.AddScoped<IAppointmentDL, AppointmentDL>();
builder.Services.AddScoped<IQueueDL, QueueDL>();


builder.Services.AddScoped(typeof(IBaseBL<>), typeof(BaseBL<>));
builder.Services.AddScoped<IAccountBL, AccountBL>();
builder.Services.AddScoped<IUserOrganizationRoleBL, UserOrganizationRoleBL>();
builder.Services.AddScoped<IOrganizationBL, OrganizationBL>();
builder.Services.AddScoped<IServiceBL, ServiceBL>();
builder.Services.AddScoped<IAppointmentBL, AppointmentBL>();
builder.Services.AddScoped<INotificationBL, NotificationBL>();
builder.Services.AddScoped<IOtpBL, OtpBL>();
builder.Services.AddScoped<IUserValidator, UserValidator>();
builder.Services.AddScoped<IOperatingTimeUpdaterService, OperatingTimeUpdaterService>();


builder.Services.AddScoped<IAuthorizationHandler, OwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DynamicAuthorizationHandler>();


builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddHostedService<OperatingTimeUpdaterHostedService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Owner", policy =>
        policy.Requirements.Add(new OwnerRequirement()));

    options.AddPolicy("Admin", policy =>
        policy.RequireRole(UserRole.Admin.ToString()));

    options.AddPolicy("Dynamic", policy =>
        policy.Requirements.Add(new DynamicAuthorizationRequirement()));
});




//Thiết lập Json
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
    });


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", (context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = context.WebSockets.AcceptWebSocketAsync().Result;
        var webSocketService = context.RequestServices.GetRequiredService<WebSocketService>();
        return webSocketService.HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400; // Bad Request
        return Task.CompletedTask;
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
