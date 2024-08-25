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

> Note:
> As a best practice separate sensitive data in your local development as well
> Adding a user secrets file to include sensitive configurations is a better approach

* Create `host.json` file with below content

> Note: setting the default log level to `Warning` will filter unnecessary logs.
> Set the `Function` log level to `Information` so you could actually see the logs which you want to see from functions
> If you would like to set log levels for individual functions use `Function.[FUNCTION NAME]` syntax

```json
{
  "version": "2.0",
  "logging": {
    "logLevel": {
      "default": "Warning",
      "Function": "Information"
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

## Doing many related actions as part of the request

An API call will do some processing and will return an HTTP response.
In Azure functions integrating with some Azure services has been made by output bindings.

There are few ways you could do this.

### MultiResponse Type
This is a class which contains the HTTP response along with the other output bindings.

### Attribute based Functions



## Running the project locally

* Create a run configuration as shown below
 
![Setting run configuration for Azure function](../../../Images/function_run_configuration.png "Azure Function Run Configuration")

## BLOB Trigger

> Note
> Make sure you have the latest `Azurite` running in your machine
> If you don't have the latest `Azurite` installed in your machine, you'll get an error as below at runtime

![azurite_error](../../../Images/azurite_error.png "Azurite error with incompatible versions")

* To fix this, do the following,
  * Install latest `npm`
  * Install latest `Azurite` through `npm`

```
npm install -g npm@latest
npm install -g azurite@latest
```

* Install `Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs`

* Add a BLOB trigger function as below. It simply reads a file from a BLOB container

```csharp
public class FileCopyFunction
{
    private readonly ILogger<FileCopyFunction> _logger;

    public FileCopyFunction(ILogger<FileCopyFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(FileCopyFunction))]
    public async Task Run([BlobTrigger("sample-work/{name}", Connection = "SourceConnection")] Stream sourceStream, string name)
    {
        using var reader = new StreamReader(sourceStream);
        var content = await reader.ReadToEndAsync();
        
        _logger.LogInformation("{FileName} was read successfully", name);
    }
}
```

* As a best practice, it's best to separate Azure function and application related storages and configurations
  * In `local.settings.json` add `AzureWebJobsSourceConnection` to point to the storage

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "FUNCTIONS_WORKER_RUNTIME_VERSION": "8.0",
    "AzureWebJobsSourceConnection": "UseDevelopmentStorage=true"
  }
}
```

* In the Azure function we use the "SourceConnection" part only to specify the connection string

## Queue Trigger

* Install `Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues` nuget package
* You can bind to [different types](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue?tabs=isolated-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp#binding-types) when binding to the trigger, my favourite is to bind directly to the DTO type
  * `string`
  * `byte`
  * `JSON serializable type`
  * `QueueMessage`
  * `BinaryData`

* Setting storage related configuration it's similar to BLOB triggered function
* Add a queue trigger function as below

```csharp
public class ReadOrderFunction(ILogger<ReadOrderFunction> logger)
{
    [Function(nameof(ReadOrderFunction))]
    public async Task Run([QueueTrigger(queueName: "%Source:Queue%", Connection = "SourceConnection")] CreateOrderRequest message)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        logger.LogInformation("Create order request received {@CreateOrderRequest}", message);
    }
}
```

## Disabling a Function

* You can disable any function using configuration as below

```
"AzureWebJobs.[FUNCTION NAME].Disabled": true
```

## Adding Central Package Management

## CI/CD

### CI (Continuous Integration)

Whenever the code is pushed perform a set of actions which validates the integrity and the quality of the code.
How you would like to validate the integrity and quality is dependent on the company or team you are working with.

#### What can trigger CI activities
Mostly CI activities should be triggered when,

* When a remote branch is created
* When a PR is created
* When the code is merged to `main` branch

There could be other scenarios depending on your team, but the idea of CI is to maintain the quality and integrity of
your solution.

#### What are CI activities?
Commonly these actions are performed as CI activities.

* Build and Restore
* Running Tests

You could perform some additional activities depending on your needs,

* Check code quality using tools like SonarQube
* Check for code formatting
* Check for potential security vulnerabilities
* Etc...

What can these actions be?

* Permissions Required for ServicePrincipal
* Environment Setup and Configuration
* Tagging and Releasing
  * Handing PRs
  * Handling Releases

### Permissions Required for ServicePrincipal

Provisioning Azure resources are done using a Service Principal.
A service principal is credentials with certain authorized permissions.
Each service principal can be different depending on what resources it needs to provision.

In our application we need a service principal who needs permission to perform the below actions in Azure

* To create a resource group
* To provision resources in that resource group
* To assign RBAC to resources

There are inbuilt roles in Azure, with preconfigured permissions associated with them.

* To create a resource group
  * To create resource groups the SP requires `Contributor` access at the subscription level
  
* To provision resources in that resource group
  * `Contributor` is able to create resources within a resource group
  
* To assign RBAC to resources
  * The SP requires `User Access Administrator` access at the subscription level or at the resource group level
  * Since we are planning to create the resource group as part of the deployment this needs to be at the subscription
  level

So in summary the SP will need to have `Contributor` and `User Access Administrator` roles assigned to it.

### Creating a Service Principal

Let's use Azure CLI to create a service principal

Use a command prompt opened as an administrator.

* Login to Azure

```shell
az login
```

This will open a browser for you to login to Azure

* Create a service principal

```shell
az ad sp create-for-rbac --name "[service principal name]" --role Contributor --scopes /subscriptions/[subscription id]
```
This will create a SP with the name you have provided, and it will assign the `Contributor` role for specified 
subscription id.

This will output the below information, and it's crucial that you save these data in a secure approach.

```json
{
  "appId": "[application id]",
  "displayName": "[display name]",
  "password": "[client secret]",
  "tenant": "[tenant id]"
}
```

* Assigning roles

```shell
az role assignment create --assignee "[APP ID]" --role "User Access Administrator" --scope /subscriptions/[SUBSCRIPTION ID]
```

In our case we need the `User Access Administrator` role assigned to our service principal as well. This role is 
assigned at the subscription level.

But you can assign roles in the resource group level as well.

* Verify if roles are assigned correctly

```shell
az role assignment list --assignee "[APP ID]" --output table
```

This will show the assigned roles output as a table

* Optional: You would like to reset the secret

```shell
az ad sp credential reset --id "[APP ID]"
```