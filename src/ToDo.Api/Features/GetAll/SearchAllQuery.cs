using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ToDo.Api.Infrastructure.DataAccess;

namespace ToDo.Api.Features.GetAll;

public record SearchAllQuery : IQuery
{
    private static readonly string CacheKey = $"{typeof(SearchAllQuery).FullName}-GetAll";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
    };

    internal record Handler(
        TodoDbContext Context,
        IDistributedCache Cache) : IQueryHandler<SearchAllQuery, List<TodoDataModel>>
    {
        public async ValueTask<List<TodoDataModel>?> QueryAsync(SearchAllQuery query, CancellationToken token)
        {
            var tasks = new List<TodoDataModel>();
            var rawData = await Cache.GetAsync(CacheKey, token);
            if (rawData != null)
            {
                using var memoryStream = new MemoryStream(rawData);
                tasks = await JsonSerializer.DeserializeAsync<List<TodoDataModel>>(memoryStream, SerializerOptions, token);
            }

            if (tasks == null || !tasks.Any())
            {
                tasks = await Context.Todos.AsNoTracking().ToListAsync(token);
            }

            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, tasks, SerializerOptions, token);
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            var bytes = reader.ReadBytes((int) stream.Length);
            await Cache.SetAsync(CacheKey, bytes, CacheOptions, token);

            return tasks;
        }
    }
}