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
        string tablePrefix = "Test";
        string parameterPrefix = "/cognitoapi/";

        Table ddbTable = new(this, "datatable", new TableProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Type = AttributeType.STRING,
                Name = "Id"
            },
            TableName = $"{tablePrefix}Table",
        });



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


        // IdentityPool
         CfnIdentityPool identityPool = new CfnIdentityPool(this, "api-identity-pool", new CfnIdentityPoolProps
         {
             AllowUnauthenticatedIdentities = false,
             IdentityPoolName = "api-identity-pool",

             CognitoIdentityProviders = new CfnIdentityPool.CognitoIdentityProviderProperty[] { new CfnIdentityPool.CognitoIdentityProviderProperty(){
                 ClientId = client.UserPoolClientId,
                 ProviderName = $"cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}",
                 ServerSideTokenCheck = true
             }}
         });



        // Update Role

        var prin = new FederatedPrincipal("cognito-identity.amazonaws.com",
        new Dictionary<string, object>
        {
            {
                "StringEquals", new Dictionary<string,string>
                {
                    {
                         "cognito-identity.amazonaws.com:aud", identityPool.Ref
                    }
                }
            },
            {
                "ForAnyValue:StringLike", new Dictionary<string,string>
                {
                    {
                        "cognito-identity.amazonaws.com:amr", "authenticated"
                    }
                }
            }
        }, "sts:AssumeRoleWithWebIdentity");


        // Role
        Role role = new(this, "ddbRole", new RoleProps
        {
            Description = "Role allows DynamoDB Access",
            AssumedBy = prin
        });


        // Add the role to the 
        ddbTable.GrantReadWriteData(role);

        var stmt = new PolicyStatement(new PolicyStatementProps
        {
            Actions = new string[]{ "dynamodb:DescribeTable" },
            Effect = Effect.ALLOW,
            Resources = new string[]{ ddbTable.TableArn }
        });

        role.AttachInlinePolicy( new Policy(this, "allowDescribeTablePolicy", new PolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new PolicyStatement[]
                {
                   stmt
                }
            })
        } ));





        CfnIdentityPoolRoleAttachment identityPoolRoleAttachment = new CfnIdentityPoolRoleAttachment(this, "api-identity-pool-att", new CfnIdentityPoolRoleAttachmentProps
        {
            IdentityPoolId = identityPool.Ref,
            Roles = new Dictionary<string, string>{
                { "authenticated", role.RoleArn  }
            }
        });





        StringParameter identityPoolParameter = new (this, "identityPoolParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}identitypoolid",
            StringValue = identityPool.Ref
        });

        //Parameters
        StringParameter clientIdParameter = new (this, "clientIdParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}clientid",
            StringValue = client.UserPoolClientId
        });

        StringParameter userPoolId = new (this, "userPoolIdParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}userpoolid",
            StringValue = pool.UserPoolId
        });

        StringParameter poolIdParameter =  new (this, "poolIdParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}metadataurl",
            StringValue = $"https://cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}/.well-known/openid-configuration"
        });

        StringParameter validIssueParameter =  new (this, "issuerParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}validissuer",
            StringValue = $"https://cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}"
        });

        StringParameter jwksparameter =  new (this, "jwksParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}jwks",
            StringValue = $"https://cognito-idp.{this.Region}.amazonaws.com/{pool.UserPoolId}/.well-known/jwks.json"
        });

        StringParameter redirectUriTemplate =  new (this, "redirectUriTemplate", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}redirecturitemplate",
            StringValue = "https://{0}/authentication/login-callback"
        });

        StringParameter logpoutRedirectUriTemplate =  new (this, "logoutRedirectUriTemplate", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}postlogoutredirecturitemplate",
            StringValue = "https://{0}/authentication/logout-callback"
        });

        StringParameter poolDomainParameter =  new (this, "PoolDomainParam", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}authority",
            StringValue = $"{domain.BaseUrl()}/{cfnPool.Ref}"
        });

        StringParameter responseTypeParameter =  new (this, "responseTypeParameter", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}responseType",
            StringValue = "code"
        });

        StringParameter tablePrefixParameter =  new (this, "tablePrefixParameter", new StringParameterProps{
            DataType = ParameterDataType.TEXT,
            ParameterName = $"{parameterPrefix}tablePrefix",
            StringValue = tablePrefix
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

