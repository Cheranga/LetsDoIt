# Bunsen Burner 🔥

A testing library which helps to easily write well-structured test cases 
in both `Arrange, Act, Assert (AAA)`, and `Given, When, Then (BDD)` testing patterns.

## Context :dart:

When it comes to testing .NET applications, there are many options.

I think, as .NET developers, we are privileged to have such a plateau of options to choose.
But, this can be a problem as well, since now we have too many options to choose from 
ranging from packages, conventions, and guides.

Let me introduce you to `Bunsen Burner` where you can write tests easily in a well-structured approach 
regardless of the chosen testing patterns, naming conventions, etc...

## System Under Test :tada:

It's a simple task management API, where you can perform,
- [x] Add task
- [x] Get all tasks
- [x] Get task by id

To keep things simple enough, we are using entity framework's in-memory database for data operations.

To make things interesting `distributed caching` has been introduced when getting all tasks, 
and an `endpoint filter` has been included for validation when adding a task.

We will be using the same `ToDo API` from Microsoft's documentation, with some added extra features.
We have introduced `distributed caching`, and `endpoint filters` for the creation, and getting all task 
endpoints.

### Add Task

We will include an `endpoint filter` to perform validation on the input request.

If the request is invalid, it will be short-circuit and will return a bad request (HTTP 400) 
response back to the caller.

```mermaid
sequenceDiagram
Caller ->> API: add todo request
note over API: Validation filter
alt is invalid?
    API ->> Caller: bad request (HTTP 400)
else
    API ->> CreateTaskCommandHandler: save task to database
    CreateTaskCommandHandler ->> API: response
    alt is successful?
        API ->> Caller: task created response (HTTP 201)
    else
        API ->> Caller: problem response (HTTP 500)
    end
end
```

### Get All Tasks

We have another `endpoint filter` which will use a distributed cache to cache the response.
To keep things simple enough, for the distributed cache, we are using `Microsoft.Extensions.Caching.Memory` 
which is an in-memory implementation of `IDistributedCache`.

```mermaid
sequenceDiagram
Caller ->> API: get all tasks request
API ->> Cache: get tasks from cache
note left of Cache: CachingFilter
Cache ->> API: response
alt is cached data available?
    API ->> Caller: return cached response (HTTP 200)
else
    API ->> SearchAllQueryHandler: get tasks from database
    SearchAllQueryHandler ->> API: response
    alt are tasks available?
        API ->> Cache: save tasks in cache
        API ->> Caller: tasks response (HTTP 200)
    else
        API ->> Caller: no content response (HTTP 204)
    end
end
```

### Search Task by Id

```mermaid
sequenceDiagram
Caller ->> API: search by task id request
API ->> SearchByIdQueryHandler: find task using id in database
SearchByIdQueryHandler ->> API: response
alt task exists?
API ->> Caller: task response (HTTP 200)
else
    API ->> Caller: no content response (HTTP 204)
end
```

## Testing `Get All Tasks` feature

We'll use unit tests to test the internal operations, and an integration test approach
to test the behaviour of this feature.

For unit tests, let's use the `AAA` pattern, and for integration tests the `behavioural` pattern. 

### Unit tests of the `GetAll.Operations` class

> Cache only if tasks are available in database

Without using `Bunsen Burner`




For unit tests I have followed the `AAA` pattern, and for integration tests
I have used the `BDD` approach as they seem to fit with 



To understand the differences, let's focus on how to test the endpoint filter
`GetAllTasksFilter`.

without using Bunsen Burner :frowning:

Let's consider testing the endpoint filter `GetAllTasksFilter`.





### Unit Tests

Let's use the `Arrange, Act, Assert` pattern for unit tests.

### Integration Tests

Let's use the `Given, When, Then` pattern (behavioural driven) for integration tests.

### Kudos :clap:

* [Bunsen Burner in GitHub](https://github.com/bmazzarol/Bunsen-Burner)