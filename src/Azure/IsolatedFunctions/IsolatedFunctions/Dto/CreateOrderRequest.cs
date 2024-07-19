namespace IsolatedFunctions.Dto;

public record CreateOrderRequest(string OrderId, string ReferenceId);

public record OrderAcceptedResponse(
    string OrderId,
    string ReferenceId,
    DateTimeOffset AcceptedOn);
