using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Movies.Api.ErrorHandling;
using Movies.Application;
using Movies.Infrastructure;
using Movies.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        // Validation errors are keyed by parameter name; make them "pageSize", like the query string.
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

// The OpenAPI document is generated from these options.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Movies API";
        document.Info.Description =
            "Search, filter, sort and page through the Kaggle '9000+ Movies' dataset. " +
            "Cast data comes from TMDB. This product uses TMDB and the TMDB APIs but is not endorsed, certified, " +
            "or otherwise approved by TMDB.";
        return Task.CompletedTask;
    });

    // Query parameters bind case-insensitively, so document them in camelCase like the JSON.
    options.AddOperationTransformer((operation, _, _) =>
    {
        foreach (var parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            if (parameter.In == ParameterLocation.Query && !string.IsNullOrEmpty(parameter.Name))
            {
                parameter.Name = JsonNamingPolicy.CamelCase.ConvertName(parameter.Name);
            }
        }

        return Task.CompletedTask;
    });

    // Enums (sortBy, sortDirection) are sent as text, e.g. sortBy=releaseDate.
    options.AddSchemaTransformer((schema, context, _) =>
    {
        var type = context.JsonTypeInfo.Type;
        var underlying = Nullable.GetUnderlyingType(type);
        if ((underlying ?? type).IsEnum)
        {
            schema.Type = underlying is null ? JsonSchemaType.String : JsonSchemaType.String | JsonSchemaType.Null;
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<MoviesDbContext>("database");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Movies API v1");
    options.DocumentTitle = "Movies API";
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

if (app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value.InitializeOnStartup)
{
    await app.Services.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping);
}

await app.RunAsync();
