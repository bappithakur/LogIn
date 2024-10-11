using Database.Data;
using Login.Microservice.Handler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Utilities.SharedModel;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config
// Add services to the container.
//builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"C:\tempkeys\"))
//                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
//                {
//                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
//                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
//                });
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    //builder.WithOrigins("http://localhost:3000/", "http://localhost:3600/")
    //       .AllowAnyMethod()
    //       .AllowAnyHeader()
    //       .AllowCredentials();

    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));
builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
        });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Login Microservice",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
   {
     new OpenApiSecurityScheme
     {
       Reference = new OpenApiReference
       {
         Type = ReferenceType.SecurityScheme,
         Id = "Bearer"
       }
      },
      new string[] { }
    }
  });
});
#region Startup Class Work
//services.AddCors();
builder.Services.AddControllers();

builder.Services.AddDbContext<AgroTechContext>(
  option => option.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(
   option =>
   {
       option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
       option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   }).AddJwtBearer(options =>
   {
       options.SaveToken = true;
       options.RequireHttpsMetadata = true;
       //ValidAudience = Configuration["Jwt:Site"],
       //ValidIssuer = Configuration["Jwt:Site"],
       options.TokenValidationParameters = new TokenValidationParameters()
       {
           ValidateIssuer = false,
           ValidateLifetime = true,
           ValidateAudience = false,
           ValidateIssuerSigningKey = true,
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]))
       };
   });
//to get token from http
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// Register Handler
builder.Services.AddSingleton<IAuthorizationHandler, RequirementHandler>();
// Register Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ContentsEditor", policy =>
    {
        policy.AddAuthenticationSchemes("Bearer");
        policy.RequireAuthenticatedUser();
    });
});


//builder.Services.AddSingleton< RequirementHandler>();
#endregion
var app = builder.Build();
app.UseStatusCodePages(async context =>
{
    APIResponse response = new();
    if (context.HttpContext.Response.StatusCode == 401 || context.HttpContext.Response.StatusCode == 403)
    {
        response.success = false;
        response.data = JsonConvert.SerializeObject(Array.Empty<string>());
        response.message = "Session Is Expired!!";
        await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
});

app.UseHttpsRedirection();
app.UseCors("MyPolicy");
//app.UseRequestSwaggerAuth();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
}
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI(c => c.SwaggerEndpoint("./swagger/v1/swagger.json", "v1"));
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("./swagger/v1/swagger.json", "v1");
        c.RoutePrefix = "swagger";
    });
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
