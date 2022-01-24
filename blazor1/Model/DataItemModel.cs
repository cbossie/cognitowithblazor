namespace blazor1.Model
{
    public class DataItemModel
    {
        [JsonPropertyName("Id")]
        public string ItemId { get; set; } = Guid.NewGuid().ToString();

        public string? ItemName { get; set; }

        public decimal ItemAmount { get; set; } = 0m;
    }
}
