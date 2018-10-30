public class MessageMapping
{
    public const string TableName = "SentMessages";
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string OrchestrationId { get; set; }
}