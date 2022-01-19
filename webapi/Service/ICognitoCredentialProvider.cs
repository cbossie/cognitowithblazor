using Amazon.Runtime;

namespace webapi.Service;

public interface ICognitoCredentialProvider
{
    Task<AWSCredentials> GetCredentials();
}
