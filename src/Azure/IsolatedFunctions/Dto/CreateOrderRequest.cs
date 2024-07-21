namespace IsolatedFunctions.Dto;

public record CreateOrderRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
}
