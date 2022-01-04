using System.Net.Mime;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Cognito;

namespace Infra;

public class InfraStack : Stack
{
    internal InfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        Table ddbTable = new(this, "datatable", new TableProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Type = AttributeType.STRING,
                Name = "Id"
            },
            TableName = "TestTable",
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
            "https://localhost:7226",
            "https://localhost:7226/authentication/login-callback"
        };
        cfnClient.LogoutUrLs = new string[]{
            "https://localhost:7226/authentication/logout-callback"
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

