﻿using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using IsolatedFunctions.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace IsolatedFunctions.Http;

public class CreateOrderFunction(ILogger<CreateOrderFunction> logger)
{
    [Function(nameof(CreateOrderFunction))]
    public async Task<OrderAcceptedResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData request
    )
    {
        var dtoRequest = await request.ReadFromJsonAsync<CreateOrderRequest>();
        if (dtoRequest == null)
        {
            logger.LogWarning("Create order request does not contain any data to proceed");
            return await OrderAcceptedResponse.EmptyRequest(request);
        }

        logger.LogInformation("Input request {CreateOrderRequest} request received", JsonSerializer.Serialize(dtoRequest));

        return await OrderAcceptedResponse.Success(request, dtoRequest);
    }
}
