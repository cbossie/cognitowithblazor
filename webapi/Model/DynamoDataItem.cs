using Amazon.DynamoDBv2.DataModel;

namespace webapi.Model
{
    [DynamoDBTable("TestTable")]
    public class DynamoDataItem
    {
        [DynamoDBHashKey]
        public string? Id { get; set; }
    }
}
