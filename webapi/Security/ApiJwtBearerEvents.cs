using Amazon.CognitoIdentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;

namespace webapi.Security
{
    public class ApiJwtBearerEvents : JwtBearerEvents
    {
        CognitoAWSCredentials Creds { get; init; }
        public ApiJwtBearerEvents(CognitoAWSCredentials creds)
        {
            Creds = creds;
            
        }

        public override Task TokenValidated(TokenValidatedContext context)
        {
            JwtSecurityToken accessToken = context.SecurityToken as JwtSecurityToken;
            if (accessToken is not null)
            {
                string shortIssuer = context.SecurityToken.Issuer;
                Creds.AddLogin(shortIssuer, accessToken.RawData);               
            }           
            return Task.CompletedTask;
        }
    }
}
