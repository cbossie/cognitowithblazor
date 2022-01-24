using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;

namespace webapi.Service;

public class CognitoCredentialProvider : ICognitoCredentialProvider
{
    private IHttpContextAccessor HttpContextAccessor { get; init; }
    private IAmazonCognitoIdentity Cognito { get; init; }

    private CognitoAWSCredentials Credentials { get; init; }

    private HttpContext CurrentContext => HttpContextAccessor?.HttpContext ?? throw new Exception("no context!?!");

    private IIdentity User => CurrentContext.User.Identity;

    private async Task<string?> GetBearerToken() => await CurrentContext.GetTokenAsync("Bearer", "access_token").ConfigureAwait(false);
    

    public CognitoCredentialProvider(IHttpContextAccessor httpContextAccessor, IAmazonCognitoIdentity cognito, CognitoAWSCredentials creds)
    {
        HttpContextAccessor = httpContextAccessor;
        Cognito = cognito;
        Credentials = creds;

    }
    public async Task<AWSCredentials> GetCredentials()
    {
        var bearer = await GetBearerToken();

        if(!User.IsAuthenticated || string.IsNullOrWhiteSpace(bearer))
        {
            return new AnonymousAWSCredentials();
        }
        return Credentials;
        //var resp = await Cognito.GetCredentialsForIdentityAsync(new GetCredentialsForIdentityRequest
        //{
        //    IdentityId = bearer            
        //}).ConfigureAwait(false);

        

        
  
    }

}
    