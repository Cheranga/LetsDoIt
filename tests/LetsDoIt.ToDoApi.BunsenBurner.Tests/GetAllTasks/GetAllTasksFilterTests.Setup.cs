using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ToDo.Api.Features.GetAll;
using ToDo.Api.Infrastructure.DataAccess;

namespace LetsDoIt.ToDoApi.BunsenBurner.Tests.GetAllTasks;

public partial class GetAllTasksFilterTests
{
    private static void SetupInMemoryDataStore(IServiceCollection services)
    {
        var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));
        if (dbContextDescriptor != null)
            services.Remove(dbContextDescriptor);

        services.AddDbContext<TodoDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
        });
    }

    private static void SetupMockedCache(IServiceCollection services, params List<TodoDataModel>[] results)
    {
        var mockedCache = new Mock<IDistributedCache>();
        var setupResult = mockedCache.SetupSequence(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        foreach (var result in results)
        {
            setupResult.ReturnsAsync(JsonSerializer.SerializeToUtf8Bytes(result, Constants.SerializerOptions));
        }

        services.AddSingleton(mockedCache.Object);
    }

    private static async Task<TodoListResponse?> GetToDoListResponse(HttpResponseMessage httpResponse)
    {
        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var todoListResponse = JsonSerializer.Deserialize<TodoListResponse>(responseContent, Constants.SerializerOptions);
        return todoListResponse;
    }
}
