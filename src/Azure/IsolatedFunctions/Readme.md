# Isolated Azure Functions

## Vanilla Project Structure

* Create a console application

* Install the below nuget packages
  * `Microsoft.Azure.Functions.Worker`
  * `Microsoft.Azure.Functions.Worker.Sdk`

* Set the `AzureFunctionsVersion` to `v4` in the `csproj` file

```xml
  <PropertyGroup>
<!--    Rest of the settings-->
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>    
  </PropertyGroup>
```

* Create `local.settings.json` file with the below content

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "FUNCTIONS_WORKER_RUNTIME_VERSION": "8.0"
  }
}
```

* Create `host.json` file with below content

```json
{
  "version": "2.0",
  "logging": {
    "logLevel": {
      "default": "Information"
    }
  }
}
```

* Set both `local.settings.json` and `host.json` file properties as below
  * `CopyToOutputDirectory` as `PreserveNewest`
  * `CopyToPublishDirectory` as `false`

## HTTP Trigger

* Install `Microsoft.Azure.Functions.Worker.Extensions.Http` nuget package
* Create a basic function as below

```csharp
public class CreateOrderFunction(ILogger<CreateOrderFunction> logger)
{
    [Function(nameof(CreateOrderFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData request)
    {
        var dtoRequest = await request.ReadFromJsonAsync<CreateOrderRequest>();
        if (dtoRequest == null)
        {
            logger.LogWarning("Create order request does not contain any data to proceed");
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        logger.LogInformation("{@CreateOrderRequest} request received", dtoRequest);
        var dtoResponse = new OrderAcceptedResponse(dtoRequest.OrderId,
            dtoRequest.ReferenceId, DateTimeOffset.UtcNow);
        
        
        var httpResponse = request.CreateResponse();
        await httpResponse.WriteAsJsonAsync(dtoResponse, HttpStatusCode.Accepted);
        
        return httpResponse;
    }
}
```


## Running the project locally

* Create a run configuration as shown below
 
![Setting run configuration for Azure function](../../../../Images/function_run_configuration.png "Azure Function Run Configuration")

