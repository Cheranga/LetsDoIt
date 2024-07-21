using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IsolatedFunctions.Blob;

public class FileCopyFunction(ILogger<FileCopyFunction> logger)
{
    [Function(nameof(FileCopyFunction))]
    public async Task Run(
        [BlobTrigger("%Source:Container%/{name}", Connection = "SourceConnection")] Stream sourceStream,
        string name
    )
    {
        using var reader = new StreamReader(sourceStream);
        var content = await reader.ReadToEndAsync();

        logger.LogInformation("{FileName} was read successfully", name);
    }
}
