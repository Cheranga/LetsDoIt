using System.Net;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace IsolatedFunctions.Dto;

public record CreateOrderDataModel : ITableEntity
{
    public string OrderId { get; set; }
    public string ReferenceId { get; set; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public record OrderAcceptedResponse
{
    public HttpResponseData HttpResponse { get; set; }

    [QueueOutput(queueName: "%Source:Queue%", Connection = "SourceConnection")]
    public string[] Messages { get; set; }

    [TableOutput(tableName:"%Source:Table%", Connection = "SourceConnection")]
    public CreateOrderDataModel DataModel { get; set; }

    public static async Task<OrderAcceptedResponse> EmptyRequest(HttpRequestData request)
    {
        var httpResponse = request.CreateResponse(HttpStatusCode.BadRequest);
        await httpResponse.WriteStringAsync("Invalid request, and does not adhere to the schema of the request");
        return new OrderAcceptedResponse
        {
            HttpResponse = httpResponse
        };
    }

    public static async Task<OrderAcceptedResponse> Success(HttpRequestData httpRequest, CreateOrderRequest dtoRequest)
    {
        var httpResponse = httpRequest.CreateResponse();
        await httpResponse.WriteAsJsonAsync(dtoRequest, HttpStatusCode.Accepted);

        var serialized = JsonSerializer.Serialize(dtoRequest);
        var dataModel = new CreateOrderDataModel
        {
            OrderId = dtoRequest.OrderId,
            ReferenceId = dtoRequest.ReferenceId,
            PartitionKey = $"{dtoRequest.OrderId}_{dtoRequest.ReferenceId}",
            RowKey = Guid.NewGuid().ToString("N"),
            Timestamp = DateTimeOffset.UtcNow
        };
        

        return new OrderAcceptedResponse
        {
            HttpResponse = httpResponse,
            Messages = [serialized],
            DataModel = dataModel
        };
    }
}