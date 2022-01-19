using webapi.Model;

namespace webapi.Service;

public interface IDataService
{
    public Task AddDataItem(DynamoDataItem item);

    public Task<DynamoDataItem> GetDataItem(string itemId);

}
