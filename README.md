# dotnet-foundation

Provides reusable building blocks for .NET and ASP.NET applications. Copy the parts you need into your project.

| Feature | What it does |
| --- | --- |
| [Requests](#requests) | Sends a request to its handler through a mediator |
| [Request behaviors](#request-behaviors) | Runs logging, authorization, validation, transactions, concurrency checks and caching around the handler |
| [Results and errors](#results-and-errors) | Returns a value or an error instead of throwing, and maps errors to HTTP responses |
| [Endpoints](#endpoints) | Maps minimal API endpoints written as separate classes |
| [Lifetime services](#lifetime-services) | Registers services marked as scoped, singleton or transient with their matching interfaces |
| [Pagination](#pagination) | Returns a page of items from an Entity Framework or MongoDB query |
| [Caching](#caching) | Stores, reads and removes values in a distributed cache |
| [Exception handling](#exception-handling) | Maps unhandled exceptions to problem details responses |
| [Current user](#current-user) | Gets the id of the user of the current request |
| [Entities](#entities) | Provides a base entity and sets its audit, soft delete and version fields when saving changes |
| [Domain events and outbox](#domain-events-and-outbox) | Saves domain events with the changes raising them and publishes them to their handlers in the background |

See [Dependencies](#dependencies) for the packages used.

## Requests

Sends a request to its handler through a mediator. Every request has exactly one handler, checked at startup.

```csharp
public sealed record GetOrder(Guid Id) : IRequest<Result<OrderView, RequestError>>;

internal sealed class GetOrderHandler(AppDbContext dbContext) : IRequestHandler<GetOrder, Result<OrderView, RequestError>>
{
    public async Task<Result<OrderView, RequestError>> HandleAsync(GetOrder request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.FindAsync([ request.Id ], cancellationToken);

        return order is null ? RequestError.ResourceNotFound("order.not_found") : new OrderView(order.Id, order.Name);
    }
}
```

```csharp
services.AddRequestHandlers(assembly)
        .AddRequestMediator(assembly);

var result = await requestMediator.SendAsync(new GetOrder(id), cancellationToken);
```

Requests without a result implement `IRequest`, and their handlers implement `IRequestHandler<TRequest>`.

## Request behaviors

Runs logging, authorization, validation, transactions, concurrency checks and caching around the handler. The behaviors run in the order they are registered, and a request can use several of them at once. Register only the behaviors you need.

```csharp
services.AddLoggingBehavior()
        .AddAuthorizationBehavior(assembly)
        .AddValidationBehavior(assembly)
        .AddTransactionBehavior<AppDbContext>()
        .AddConcurrencyBehavior()
        .AddCachingBehavior();
```

Authorization, validation and concurrency apply only to requests returning `Result<TValue, RequestError>` or `Result<RequestError>`.

### Logging

Logs the duration of the request with its failure result or exception. Applies to every request.

```text
[Information] Handled the request 'GetOrder' in 12.4 ms.
[Warning] Handled the request 'GetOrder' in 3.1 ms with a failure of type 'ResourceNotFound' and code 'order.not_found'.
[Error] An exception occurred while handling the request 'EditOrder' after 8.0 ms.
```

### Authorization

Checks the request with its authorizers and returns `RequestNotAllowed` (403) instead of calling the handler when an authorizer does not allow it. Applies to requests with an authorizer implementing `IRequestAuthorizer<TRequest>`.

```csharp
internal sealed class EditOrderAuthorizer(ICurrentUser currentUser, AppDbContext dbContext) : IRequestAuthorizer<EditOrder>
{
    public Task<bool> IsAuthorizedAsync(EditOrder request, CancellationToken cancellationToken = default)
    {
        return dbContext.Orders.AnyAsync(order => order.Id == request.Id && order.CreatedBy == currentUser.Id, cancellationToken);
    }
}
```

### Validation

Validates the request with its validators and returns `RequestInvalid` (400) with the validation messages instead of calling the handler when the validation fails. Applies to requests with a validator implementing `IValidator<T>`.

```csharp
internal sealed class EditOrderValidator : AbstractValidator<EditOrder>
{
    public EditOrderValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
    }
}
```

### Transaction

Handles the request in a database transaction, committed when the request succeeds and rolled back when it fails. Applies to requests implementing `ITransactionalRequest`.

```csharp
public sealed record EditOrder(Guid Id, string Name, Guid Version) : IRequest<Result<RequestError>>, ITransactionalRequest;
```

### Concurrency

Returns `ResourceConflict` (409) instead of throwing when the request fails with a concurrency conflict, as described in [Entities](#entities). Applies to every request.

### Caching

Returns the cached result of the request instead of calling the handler, and caches the results that are neither `null` nor failure results. Applies to requests implementing `ICacheableRequest`.

```csharp
public sealed record GetOrder(Guid Id) : IRequest<Result<OrderView, RequestError>>, ICacheableRequest
{
    public string CacheKey => $"orders:{Id}";

    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(5);
}
```

Remove the cache key when the cached data changes.

### Several behaviors at once

Applies every behavior that matches the request. A request matches a behavior by implementing its marker interface or by having its authorizer or validator. This request uses every behavior except caching:

```csharp
// Applies the transaction behavior through ITransactionalRequest.
public sealed record EditOrder(Guid Id, string Name, Guid Version) : IRequest<Result<RequestError>>, ITransactionalRequest;

// Applies the authorization behavior through IRequestAuthorizer<EditOrder>.
internal sealed class EditOrderAuthorizer(ICurrentUser currentUser, AppDbContext dbContext) : IRequestAuthorizer<EditOrder>
{
    public Task<bool> IsAuthorizedAsync(EditOrder request, CancellationToken cancellationToken = default)
    {
        return dbContext.Orders.AnyAsync(order => order.Id == request.Id && order.CreatedBy == currentUser.Id, cancellationToken);
    }
}

// Applies the validation behavior through AbstractValidator<EditOrder>.
internal sealed class EditOrderValidator : AbstractValidator<EditOrder>
{
    public EditOrderValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
    }
}

// Applies the concurrency behavior through ExpectVersion.
internal sealed class EditOrderHandler(AppDbContext dbContext) : IRequestHandler<EditOrder, Result<RequestError>>
{
    public async Task<Result<RequestError>> HandleAsync(EditOrder request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.SingleAsync(order => order.Id == request.Id, cancellationToken);

        dbContext.Entry(order).ExpectVersion(request.Version);

        order.Rename(request.Name);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<RequestError>.Success();
    }
}
```

Sending the request runs it through the behaviors in the order they are registered:

```text
Logging                  logs the duration and the result
└─ Authorization         returns 403 when the request is not allowed
   └─ Validation         returns 400 when the request is invalid
      └─ Transaction     commits on success and rolls back on failure
         └─ Concurrency  returns 409 when the order changed meanwhile
            └─ Caching   skips the request, which is not cacheable
               └─ EditOrderHandler
```

A request can also implement several marker interfaces, for example `IRequest<Result<OrderView, RequestError>>, ITransactionalRequest, ICacheableRequest`.

### Your own behavior

Runs your own code around the handler. Register it with `AddRequestBehavior`.

```csharp
internal sealed class MyBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        // Runs before the handler.
        var result = await next();
        // Runs after the handler.
        return result;
    }
}

services.AddRequestBehavior(typeof(MyBehavior<,>));
```

## Results and errors

Returns a value or an error instead of throwing, and maps errors to HTTP responses. `Result<TValue, TError>` holds a value or an error, and `Result<TError>` holds nothing or an error. Values and errors convert to results implicitly.

```csharp
Result<Order, RequestError> found = order;
Result<Order, RequestError> missing = RequestError.ResourceNotFound("order.not_found");

var response = found.Ensure(order => order.Name != "", RequestError.RequestInvalid("order.unnamed"))
                    .Map(order => new OrderView(order.Id, order.Name))
                    .Match(orderView => TypedResults.Ok(orderView), requestError => requestError.ToHttpResponse());
```

| Method | What it does |
| --- | --- |
| `Match` | Maps a success or a failure to one value |
| `Map` | Maps the value of a success to a new value |
| `Bind` | Chains a next step that can also fail |
| `Ensure` | Turns a success into a failure with the given error when the check is false |

`ToHttpResponse()` maps a `RequestError` to a problem details response with `error: { code, issues }`.

| Error | Status |
| --- | --- |
| `RequestInvalid` | 400 |
| `RequestNotAllowed` | 403 |
| `ResourceNotFound` | 404 |
| `ResourceConflict` | 409 |

Results serialize to JSON, which the caching behavior uses. FluentValidation results also have `IsSuccess` and `IsFailure`.

## Endpoints

Maps minimal API endpoints written as separate classes. Every endpoint class is found and mapped at startup.

```csharp
internal sealed class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/orders/{id:guid}", async (Guid id, IRequestMediator requestMediator, CancellationToken cancellationToken) =>
        {
            var result = await requestMediator.SendAsync(new GetOrder(id), cancellationToken);

            return result.Match(orderView => TypedResults.Ok(orderView), requestError => requestError.ToHttpResponse());
        });
    }
}
```

```csharp
services.AddEndpoints(assembly);

app.MapEndpoints();
```

## Lifetime services

Registers services marked as scoped, singleton or transient with their matching interfaces. Mark a class with `IScopedService`, `ISingletonService` or `ITransientService`, and name its interface after it (`OrderNumberGenerator` with `IOrderNumberGenerator`).

```csharp
public interface IOrderNumberGenerator
{
    public string Generate();
}

internal sealed class OrderNumberGenerator : IOrderNumberGenerator, ISingletonService
{
    public string Generate()
    {
        return $"ORD-{Guid.CreateVersion7():N}";
    }
}
```

```csharp
services.AddLifetimeServices(assembly);
```

## Pagination

Returns a page of items from an Entity Framework or MongoDB query. `Page<T>` holds the items, the page number, the page size, the total items, the total pages and whether there is a previous or next page.

```csharp
app.MapGet("/orders", async ([AsParameters] PageRequest pageRequest, AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    return await dbContext.Orders.OrderBy(order => order.CreatedAt)
                                 .ToPageAsync(pageRequest.PageNumber, pageRequest.PageSize, cancellationToken);
});
```

`PageRequest` reads `pageNumber` and `pageSize` from the query string, with defaults of 1 and 10 and values below 1 raised to 1. Do not reuse a MongoDB query after paging it, because paging changes the query.

## Caching

Stores, reads and removes values in a distributed cache.

```csharp
services.AddCachingHandler();

var orderView = await cachingHandler.GetOrSetAsync($"orders:{id}", token => LoadOrderAsync(id, token), TimeSpan.FromMinutes(5), cancellationToken);

await cachingHandler.RemoveAsync($"orders:{id}", cancellationToken);
```

| Method | What it does |
| --- | --- |
| `GetAsync` | Reads a value, or the default value when the key is missing (`null` for nullable types such as `int?`) |
| `GetOrSetAsync` | Reads a value, or creates and stores it when the key is missing (`null` values are not stored) |
| `SetAsync` | Stores a value, with an optional expiration |
| `RemoveAsync` | Removes a value |

The values are stored as JSON in `IDistributedCache`, which is in memory by default. Register another distributed cache, for example Redis, to share the values between instances.

## Exception handling

Maps unhandled exceptions to problem details responses. Every exception is logged and returned as 500, or as 501 for `NotImplementedException`.

```csharp
services.AddExceptionHandler();

app.UseExceptionHandler();
```

## Current user

Gets the id of the user of the current request. The id comes from the `NameIdentifier` or `sub` claim, and is `null` when the user is not authenticated or there is no current request.

```csharp
services.AddCurrentUser();
```

The audit and soft delete interceptors register the current user on their own. Register your own implementation after them, for example for background jobs.

## Entities

Provides a base entity and sets its audit, soft delete and version fields when saving changes. `Entity` has an `Id` (version 7 GUID), equality by type and id, and domain events.

| Interface | Fields | What it does |
| --- | --- | --- |
| `IAuditable` | `CreatedAt`, `UpdatedAt` | Sets the creation and update times |
| `IUserAuditable` | `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` | Sets the creation and update times and users |
| `ISoftDeletable` | `DeletedAt` | Sets the deletion time instead of deleting the row |
| `IUserSoftDeletable` | `DeletedAt`, `DeletedBy` | Sets the deletion time and user instead of deleting the row |
| `IVersionable` | `Version` | Changes the version on every save to detect concurrent changes |

```csharp
public sealed class Order : Entity, IUserAuditable, IUserSoftDeletable, IVersionable
{
    public string Name { get; private set; } = "";

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? CreatedBy { get; private set; }

    public string? UpdatedBy { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public string? DeletedBy { get; private set; }

    public Guid Version { get; private set; }

    public void Rename(string name)
    {
        Name = name;
    }
}
```

```csharp
services.AddSoftDeletableInterceptor()
        .AddAuditableInterceptor()
        .AddVersionableInterceptor();

services.AddDbContext<AppDbContext>((serviceProvider, options) => options.UseSqlServer(connectionString)
                                                                         .AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>()));
```

```csharp
// Runs at the end of OnModelCreating.
modelBuilder.ApplySoftDeletableQueryFilters();
modelBuilder.ApplyVersionableConcurrencyTokens();
```

- The users come from [`ICurrentUser`](#current-user).
- Register the soft delete interceptor first, so a soft delete also sets the update fields and the version.
- Deleted rows are hidden from queries. Include them with `IgnoreQueryFilters([ SoftDeletableExtensions.QueryFilterName ])`.

Set the version the client loaded to stop two people from overwriting each other's changes:

```csharp
dbContext.Entry(order).ExpectVersion(request.Version);
```

Saving then fails when the order changed since that version, and the concurrency behavior returns `ResourceConflict` (409). Return `Version` in your responses so clients can send it back.

## Domain events and outbox

Saves domain events with the changes raising them and publishes them to their handlers in the background. An event is never lost and never published for changes that were not saved.

```csharp
public sealed record OrderPlaced(Guid OrderId) : IDomainEvent;

public sealed class Order : Entity
{
    public string Name { get; private set; } = "";

    public static Order Place(string name)
    {
        var order = new Order { Name = name };

        order.RaiseDomainEvent(new OrderPlaced(order.Id));

        return order;
    }
}

internal sealed class SendOrderConfirmation : IDomainEventHandler<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken = default)
    {
        // Sends the order confirmation.
        return Task.CompletedTask;
    }
}
```

```csharp
services.AddDomainEventHandlers(assembly)
        .AddOutbox<AppDbContext>(options => options.PollingInterval = TimeSpan.FromSeconds(5));

// Runs at the end of OnModelCreating.
modelBuilder.ApplyOutboxMessageConfiguration();
```

The database context needs the interceptors, added as shown in [Entities](#entities).

| Option | Default | What it does |
| --- | --- | --- |
| `PollingInterval` | 10 seconds | Sets how often the outbox is checked for new messages |
| `BatchSize` | 20 | Sets how many messages are loaded at once |
| `MaxAttempts` | 5 | Sets how many times a message is tried before it is given up |
| `RetryDelay` | 30 seconds | Sets the wait before a retry, doubled after each failure |

- Handlers that already succeeded are skipped when a message is retried.
- Handlers can still run more than once, for example after a crash. Make them safe to repeat.
- Run the outbox on one instance only. Processed messages stay in the `OutboxMessage` table.

## Dependencies

- [ASP.NET Core](https://github.com/dotnet/aspnetcore)
- [FluentValidation](https://github.com/FluentValidation/FluentValidation)
- [Microsoft.EntityFrameworkCore](https://github.com/dotnet/efcore)
- [MongoDB.Driver](https://github.com/mongodb/mongo-csharp-driver)
- [Scrutor](https://github.com/khellang/Scrutor)
