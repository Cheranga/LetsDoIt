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
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

* Create `host.json` file with below content

```json
{
  "version": "2.0"
}
```

* Set both `local.settings.json` and `host.json` file properties as below
  * `CopyToOutputDirectory` as `PreserveNewest`
  * `CopyToPublishDirectory` as `false`

## HTTP Trigger

