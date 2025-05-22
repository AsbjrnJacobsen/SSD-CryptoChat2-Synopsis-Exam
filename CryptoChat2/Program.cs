using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using CryptoChat2.Data;
//using CryptoChat2.Security;
using CryptoChat2.Services;




var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=cryptochat.db"));

builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CryptoChat2", Version = "v1" });

    // JWT Auth setup
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }},
            new string[] {}
        }
    });
});

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Issuer"],
            ValidAudience = config["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration."))
            )
        };
    });

builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<FriendshipService>();
builder.Services.AddScoped<FriendChatService>();
builder.Services.AddScoped<PasswordHasherService>();
//builder.Services.AddSingleton<GroupKeyStoreService>();
builder.Services.AddScoped<GroupKeyStoreDbService>();
builder.Services.AddScoped<GroupKeyStoreDbService>();
builder.Services.AddScoped<PublicChatService>();


var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoChat2 v1");
        c.RoutePrefix = string.Empty; 
    });
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();




app.MapControllers();


app.Run();