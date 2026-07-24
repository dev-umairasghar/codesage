using CodeSage.Api.Endpoints;
using CodeSage.Api.Extensions;
using CodeSage.Api.Middleware;
using CodeSage.Application;
using CodeSage.Infrastructure;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Application starting");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(CodeSage.Infrastructure.DependencyInjection.ConfigureSerilog);

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiServices();

    var app = builder.Build();

    // Exception middleware first so later pipeline failures still return ProblemDetails.
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = static (httpContext, elapsed, exception) =>
        {
            if (exception is not null || httpContext.Response.StatusCode >= 500)
            {
                return LogEventLevel.Error;
            }

            if (httpContext.Response.StatusCode >= 400)
            {
                return LogEventLevel.Warning;
            }

            return elapsed > 3000 ? LogEventLevel.Warning : LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("Endpoint", httpContext.GetEndpoint()?.DisplayName);
        };

        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseResponseCompression();
    app.UseCors("LocalWeb");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeSage API v1");
            options.DocumentTitle = "CodeSage API";
            options.DisplayRequestDuration();
        });
    }

    // Optional in local HTTP-only scenarios; enabled by default for production-quality hosting.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHttpsRedirection();
    }

    app.MapCodeSageApiV1();

    CodeSage.Infrastructure.DependencyInjection.LogStartupDiagnostics(app.Services, Log.Logger);

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("Application stopping");
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}

/// <summary>
/// Partial Program class so integration tests can use WebApplicationFactory&lt;Program&gt;.
/// </summary>
public partial class Program;
