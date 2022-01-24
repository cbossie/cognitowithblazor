using webapi.Model;

namespace webapi.Service;

public interface IDataService
{
    public Task<DynamoDataItem> SaveDataItem(DynamoDataItem item);

    public Task<DynamoDataItem> GetDataItem(string itemId);

}
