using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Extensions.NETCore.Setup;
using webapi.Model;

namespace webapi.Service;

public class DataService : IDataService
{
    ICognitoCredentialProvider CredentialProvider { get; init; }
    private DynamoDBContext? DBContext { get; set; }

    private DynamoDbConfig DbConfig { get; init; }

    private AWSOptions AwsOpts { get; init; }

    public DataService(ICognitoCredentialProvider credProvider, DynamoDbConfig ddb, AWSOptions aws)
    {
        CredentialProvider = credProvider;
        AwsOpts = aws;
        DbConfig = ddb;
    }

    private async Task<DynamoDBContext> GetContext()
    {
        if(DBContext is not null)
        {
            return DBContext;
        }
        var creds = await CredentialProvider.GetCredentials();
        AmazonDynamoDBClient client = new(creds, AwsOpts.Region);
        DBContext = new DynamoDBContext(client);
        return DBContext;
    }



    public async Task<DynamoDataItem> GetDataItem(string itemId)
    {
        var cli = await GetContext();
        var item = await cli.LoadAsync<DynamoDataItem>(itemId, new DynamoDBOperationConfig { OverrideTableName = DbConfig.TablePrefix });
        return item;
 
    }

    public async Task<DynamoDataItem> SaveDataItem(DynamoDataItem item)
    {
        var cli = await GetContext();
        await cli.SaveAsync(item, new DynamoDBOperationConfig { TableNamePrefix = DbConfig.TablePrefix });
        return item;
    }
}
