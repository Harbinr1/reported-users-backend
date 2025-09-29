using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReportedUsersSystem.Data;
using ReportedUsersSystem.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables (Railway injects them)
builder.Configuration.AddEnvironmentVariables();

// Configure MongoDB from settings or environment
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb")
);
builder.Services.AddSingleton<MongoDbContext>();

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReportedUserService>();

// JWT configuration

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not set");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ReportedUsersSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ReportedUsersSystemUsers";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Reported Users System", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // Allow all origins for production
    });
});

var app = builder.Build();

// Middleware
// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reported Users System v1");
    c.RoutePrefix = string.Empty; // Makes Swagger the default page
});

app.UseCors("AllowAnyOrigin");
// Only use HTTPS redirection in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
