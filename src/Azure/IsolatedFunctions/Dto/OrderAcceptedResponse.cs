using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace IsolatedFunctions.Dto;

public record OrderAcceptedResponse
{
    public HttpResponseData HttpResponse { get; set; }

    [QueueOutput(queueName: "sample-work", Connection = "AzureWebJobsSourceConnection")]
    public string[] Messages { get; set; }

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
        

        return new OrderAcceptedResponse
        {
            HttpResponse = httpResponse,
            Messages = [serialized, serialized, serialized]
        };
    }
}