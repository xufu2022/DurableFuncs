namespace VideoProcessor;

public class ApprovalInfo
{
    public string OrchestrationId { get; set; }
    public string VideoLocation { get; set; }
}

public class Approval
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string OrchestrationId { get; set; }
}