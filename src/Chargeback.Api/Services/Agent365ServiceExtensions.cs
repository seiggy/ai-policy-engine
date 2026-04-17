using Azure.Core;

namespace Chargeback.Api.Services;

/// <summary>
/// Extension methods for configuring Agent365 Observability SDK integration.
/// Uses OpenTelemetry with optional A365 exporter (controlled by env var).
/// </summary>
public static class Agent365ServiceExtensions
{
    /// <summary>
    /// Adds Agent365 Observability SDK with OTel integration.
    /// Exporter is opt-in via ENABLE_A365_OBSERVABILITY_EXPORTER env var.
    /// When enabled, configures OpenTelemetry tracing and registers observability service.
    /// </summary>
    public static IHostApplicationBuilder AddAgent365Observability(
        this IHostApplicationBuilder builder)
    {
        var enabled = bool.TryParse(
            builder.Configuration["ENABLE_A365_OBSERVABILITY_EXPORTER"],
            out var value) && value;

        if (!enabled)
        {
            // A365 not configured — register no-op service
            builder.Services.AddSingleton<IAgent365ObservabilityService, NoOpAgent365ObservabilityService>();
            return builder;
        }

        // Configure OpenTelemetry with A365 exporter
        // Note: SDK 0.1.75-beta requires manual OpenTelemetry configuration
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource("Microsoft.Agents.A365.*");
                // A365 exporter will be automatically picked up by the SDK's internal configuration
            });

        // Register observability service with real implementation
        builder.Services.AddSingleton<IAgent365ObservabilityService, Agent365ObservabilityService>();

        return builder;
    }
}
