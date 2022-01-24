using Amazon.DynamoDBv2.DataModel;

namespace webapi.Model
{
    [DynamoDBTable("Table")]
    public class DynamoDataItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? ItemName { get; set; }

        public decimal ItemAmount { get; set; } = 0m;
    }
}
