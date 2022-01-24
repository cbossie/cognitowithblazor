using blazor1.Model;

namespace blazor1.Service
{
    public interface IDataService
    {
        public Task<DataItemModel?> GetItem(string itemId);
        public Task<DataItemModel> SaveItem(DataItemModel item);
    }
}
