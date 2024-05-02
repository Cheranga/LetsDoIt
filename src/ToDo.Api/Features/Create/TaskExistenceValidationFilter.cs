using FluentValidation;

namespace ToDo.Api.Features.Create;

public class TaskExistenceValidationFilter<T>(IValidator<T> validator, ILogger<TaskExistenceValidationFilter<T>> logger)
    : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.GetArgument<T>(2);
        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());
        logger.LogInformation("Valid request");
        return await next(context);
    }
}