using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IsolatedFunctions.Blob;

public class FileCopyFunction
{
    private readonly ILogger<FileCopyFunction> _logger;

    public FileCopyFunction(ILogger<FileCopyFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(FileCopyFunction))]
    public async Task Run([BlobTrigger("sample-work/{name}")] Stream sourceStream, string name)
    {
        using var reader = new StreamReader(sourceStream);
        var content = await reader.ReadToEndAsync();
        
        _logger.LogInformation("{FileName} was read successfully", name);
    }
}