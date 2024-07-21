using Azure;
using Azure.Data.Tables;

namespace IsolatedFunctions.Dto;

public record CreateOrderDataModel : ITableEntity
{
    public string OrderId { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
