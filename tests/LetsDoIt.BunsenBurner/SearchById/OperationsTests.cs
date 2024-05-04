using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using ToDo.Api.Features.SearchById;
using ToDo.Api.Infrastructure.DataAccess;
using static BunsenBurner.Aaa;

namespace LetsDoIt.BunsenBurner.SearchById;

public static class OperationsTests
{
    [Fact(DisplayName = "There is no task by the provided task id")]
    public static async ValueTask NoTaskForProvidedId() =>
        await Arrange(() =>
            {
                TodoDataModel? dataModel = null;
                var mockedQueryHandler = new Mock<IQueryHandler<SearchByIdQuery, TodoDataModel>>();
                mockedQueryHandler
                    .Setup(x => x.QueryAsync(It.IsAny<SearchByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dataModel);

                return mockedQueryHandler;
            })
            .Act(async qh =>
                await Operations.ExecuteAsync(qh.Object, Mock.Of<ILogger<Program>>(), "666", It.IsAny<CancellationToken>())
            )
            .Assert(response => { response.Result.Should().BeOfType<NoContent>(); });

    [Fact(DisplayName = "Task is available")]
    public static async ValueTask TaskIsAvailable() =>
        await Arrange(() =>
            {
                var mockedQueryHandler = new Mock<IQueryHandler<SearchByIdQuery, TodoDataModel>>();
                mockedQueryHandler
                    .Setup(x => x.QueryAsync(It.IsAny<SearchByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Fixture().Create<TodoDataModel>());

                return mockedQueryHandler;
            })
            .Act(async qh =>
                await Operations.ExecuteAsync(qh.Object, Mock.Of<ILogger<Program>>(), "666", It.IsAny<CancellationToken>())
            )
            .Assert(response =>
            {
                var todoResponse = response.Result switch
                {
                    Ok<TodoResponse> r => r.Value,
                    _ => null
                };
                todoResponse.Should().NotBeNull();
            });

    [Fact(DisplayName = "When searching for task error occurs in database query")]
    public static async ValueTask ErrorWhenGettingTaskFromDatabase() =>
        await Arrange(() =>
            {
                var mockedQueryHandler = new Mock<IQueryHandler<SearchByIdQuery, TodoDataModel>>();
                mockedQueryHandler
                    .Setup(x => x.QueryAsync(It.IsAny<SearchByIdQuery>(), It.IsAny<CancellationToken>()))
                    .Throws(new Exception("error!"));

                return mockedQueryHandler;
            })
            .Act(async qh =>
                await Operations.ExecuteAsync(qh.Object, Mock.Of<ILogger<Program>>(), "666", It.IsAny<CancellationToken>())
            )
            .Assert(response =>
            {
                var problemDetails = response.Result switch
                {
                    ProblemHttpResult p => p.ProblemDetails,
                    _ => null
                };
                problemDetails!.Detail.Should().Be("error occurred when searching task by id");
            });
}