using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using webapi.Model;

namespace webapi.Service;

public class DataService : IDataService
{
    ICognitoCredentialProvider CredentialProvider { get; init; }
    private DynamoDBContext? DBContext { get; set; }



    public DataService(ICognitoCredentialProvider credProvider)
    {
        CredentialProvider = credProvider;

    }

    private async Task<DynamoDBContext> GetContext()
    {
        if(DBContext is not null)
        {
            return DBContext;
        }
        var creds = await CredentialProvider.GetCredentials();
        AmazonDynamoDBClient client = new(creds);
        DBContext = new DynamoDBContext(client);
        return DBContext;
    }

    public async Task AddDataItem(DynamoDataItem item)
    {
        var cli = await GetContext();
        await cli.SaveAsync(item);
    }

    public async Task<DynamoDataItem> GetDataItem(string itemId)
    {
        var cli = await GetContext();
        var item = await cli.LoadAsync<DynamoDataItem>(itemId);
        return item;
 
    }
}
