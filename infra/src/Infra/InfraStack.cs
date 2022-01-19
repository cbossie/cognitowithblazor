using System.Net.Mime;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.SSM;


namespace Infra;

public class InfraStack : Stack
{
    internal InfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        string clientUrl = $"{this.Node.TryGetContext("client_url")}";
        string tableName = "TestTable";

        Table ddbTable = new(this, "datatable", new TableProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Type = AttributeType.STRING,
                Name = "Id"
            },
            TableName = tableName,
        });

        // Role
        Role role = new(this, "ddbRole", new RoleProps
        {
            Description = "Role allows DynamoDB Access",
            AssumedBy = new Amazon.CDK.AWS.IAM.AccountPrincipal(this.Account)
        });

        ddbTable.GrantReadWriteData(role);

        //User Pool;
        string userPoolName = "api-pool";
        UserPool pool = new(this, "apiPool", new UserPoolProps{
            RemovalPolicy =RemovalPolicy.DESTROY,
            UserPoolName = userPoolName,
            SelfSignUpEnabled = true,
            StandardAttributes = new Amazon.CDK.AWS.Cognito.StandardAttributes{
                 Email = new Amazon.CDK.AWS.Cognito.StandardAttribute
                 {
                     Required = true
                 }
            },
            SignInAliases = new SignInAliases{
                Email = true,
                Username = false,
                Phone = false,
                PreferredUsername = false
            },
            Mfa = Mfa.OPTIONAL,
            MfaSecondFactor = new MfaSecondFactor
            {
                Otp = true,
                Sms = false
            },
            AccountRecovery = AccountRecovery.EMAIL_ONLY
        });
        var cfnPool = pool.Node.DefaultChild as CfnUserPool;

        var domain = pool.AddDomain("cognitoDomain", new UserPoolDomainOptions{
            CognitoDomain = new CognitoDomainOptions {
                DomainPrefix = userPoolName
            }
        });

        CfnUserPoolGroup Group = new(this, "group", new CfnUserPoolGroupProps
        {
            GroupName = "ApiUsers",
            RoleArn = role.RoleArn,
            Description = "Group for normal users",
            UserPoolId = pool.UserPoolId
        });


       var client = pool.AddClient("apiClient", new UserPoolClientOptions
        {
            AuthFlows = new AuthFlow{
                Custom = true,
                UserSrp = true
            },
            RefreshTokenValidity = Duration.Days(30),
            AccessTokenValidity = Duration.Minutes(60),
            IdTokenValidity = Duration.Minutes(60),
            EnableTokenRevocation = true,
            PreventUserExistenceErrors = true,
            UserPoolClientName = "blazor-wasm"
        });

        var cfnClient = client.Node.DefaultChild as CfnUserPoolClient;
        cfnClient.CallbackUrLs = new string []{
            $"{clientUrl}",
            $"{clientUrl}/authentication/login-callback"
        };
        cfnClient.LogoutUrLs = new string[]{
            $"{clientUrl}/authentication/logout-callback"
        };
        cfnClient.SupportedIdentityProviders = new string []{"COGNITO"};

        cfnClient.AllowedOAuthFlows = new string []{
            "implicit", "code"
        };

        cfnClient.AllowedOAuthScopes = new string[]{
            "email",
            "openid",
            "phone",
            "profile"
        };


        //Parameters
        StringParameter clientIdParamater = new (this, "clientIdParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/clientid",
            StringValue = client.UserPoolClientId
        });

        StringParameter userPoolId = new (this, "userPoolIdParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/userpoolid",
            StringValue = pool.UserPoolId
        });

        StringParameter poolIdParameter =  new (this, "poolIdParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/metadataurl",
            StringValue = $"https://cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}/.well-known/openid-configuration"
        });
        
        StringParameter validIssueParameter =  new (this, "issuerParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/validissuer",
            StringValue = $"https://cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}"
        });
        
        StringParameter jwksparameter =  new (this, "jwksParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/jwks",
            StringValue = $"https://cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}/.well-known/jwks.json"
        });

        StringParameter redirectUriTemplate =  new (this, "redirectUriTemplate", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/redirecturitemplate",
            StringValue = "https://{0}/authentication/login-callback"
        });

        StringParameter logpoutRedirectUriTemplate =  new (this, "logoutRedirectUriTemplate", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/postlogoutredirecturitemplate",
            StringValue = "https://{0}/authentication/logout-callback"
        });

        StringParameter poolDomainParameter =  new (this, "PoolDomainParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/authority",
            StringValue = $"{domain.BaseUrl()}/{cfnPool.Ref}"
        });

        StringParameter responseTypeParameter =  new (this, "responseTypeParameter", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/responseType",
            StringValue = "code"
        });

        StringParameter tableNameParameter =  new (this, "tableNameParameter", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = "/cognitoapi/tableName",
            StringValue = tableName
        });        


        //Outputs
        new CfnOutput(this, "clientIdOutput", new CfnOutputProps{
            ExportName = "ClientIdOutput",
            Description = "Client Id For User Pool",
            Value = client.UserPoolClientId
        });

        new CfnOutput(this, "poolId", new CfnOutputProps{
            ExportName = "PoolId",
            Description = "User pool Id",
            Value = cfnPool.Ref
        });

        new CfnOutput(this, "poolUrl", new CfnOutputProps{
            ExportName = "PoolDomain",
            Description = "Pool Domain",
            Value = domain.BaseUrl()
        });

    }
}

