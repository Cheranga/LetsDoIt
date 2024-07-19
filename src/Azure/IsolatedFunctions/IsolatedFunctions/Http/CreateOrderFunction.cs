﻿using System.Net;
using IsolatedFunctions.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace IsolatedFunctions.Http;

public class CreateOrderFunction
{
    private readonly ILogger<CreateOrderFunction> _logger;

    public CreateOrderFunction(ILogger<CreateOrderFunction> logger) => _logger = logger;

    [Function(nameof(CreateOrderFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        var dtoRequest = await request.ReadFromJsonAsync<CreateOrderRequest>();
        if (dtoRequest == null)
        {
            _logger.LogWarning("Create order request does not contain any data to proceed");
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        _logger.LogInformation("{@CreateOrderRequest} request received", dtoRequest);
        var dtoResponse = new OrderAcceptedResponse(dtoRequest.OrderId,
            dtoRequest.ReferenceId, DateTimeOffset.UtcNow);
        
        
        var httpResponse = request.CreateResponse();
        await httpResponse.WriteAsJsonAsync(dtoResponse, HttpStatusCode.Accepted);
        
        return httpResponse;
    }
}