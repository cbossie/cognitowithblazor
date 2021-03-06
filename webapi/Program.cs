using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.DynamoDBv2;
using Amazon.CognitoIdentity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using webapi.Model;
using webapi.Service;
using System.Security.Principal;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Extensions.Configuration;
using webapi.Security;

var builder = WebApplication.CreateBuilder(args);

var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddSingleton(awsOptions);
builder.Configuration.AddSystemsManager(configSource => {
    configSource.Path = "/cognitoapi/";
    configSource.AwsOptions = awsOptions;
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("CorsPolicy", opt => opt
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});


CognitoSettings cognito = new();
builder.Configuration.Bind(cognito);
builder.Services.AddSingleton(cognito);

DynamoDbConfig ddb = new();
builder.Configuration.Bind(ddb);
builder.Services.AddSingleton(ddb);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
        options.Authority = cognito.ValidIssuer;
        options.RequireHttpsMetadata = false;
        options.EventsType = typeof(ApiJwtBearerEvents);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPrincipal>(provider => provider?.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? throw new Exception("No Identity"));
builder.Services.AddTransient<IDataService, DataService>();

// AWS Services
builder.Services.AddAWSService<IAmazonCognitoIdentity>();

builder.Services.AddScoped<ApiJwtBearerEvents>();
builder.Services.AddScoped<ICognitoCredentialProvider, CognitoCredentialProvider>();
builder.Services.AddScoped(_ => new CognitoAWSCredentials(cognito.IdentityPoolId, awsOptions.Region));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors("CorsPolicy");

app.Run();


