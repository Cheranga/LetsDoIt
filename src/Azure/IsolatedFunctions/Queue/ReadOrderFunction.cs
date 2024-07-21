using IsolatedFunctions.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IsolatedFunctions.Queue;

public class ReadOrderFunction(ILogger<ReadOrderFunction> logger)
{
    [Function(nameof(ReadOrderFunction))]
    public async Task Run([QueueTrigger(queueName: "%Source:Queue%", Connection = "SourceConnection")] CreateOrderRequest message)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        logger.LogInformation("Create order request received {@CreateOrderRequest}", message);
    }
}
