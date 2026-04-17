using Azure.Core;
using Chargeback.Api.Models;
using Microsoft.Agents.A365.Observability.Runtime.Tracing.Contracts;
using Microsoft.Agents.A365.Observability.Runtime.Tracing.Scopes;
using A365Request = Microsoft.Agents.A365.Observability.Runtime.Tracing.Contracts.Request;

namespace Chargeback.Api.Services;

/// <summary>
/// Agent365 Observability service wrapper — creates InvokeAgent and Inference scopes.
/// Uses lightweight identity (ClientAppId as agent ID) without provisioned Agentic Users.
/// Wraps all scope creation in try/catch to ensure observability failures never break request flow.
/// </summary>
public interface IAgent365ObservabilityService
{
    IDisposable? StartInvokeAgentScope(string clientAppId, string tenantId, string? clientDisplayName, string? correlationId, string? promptContent = null);
    IDisposable? StartInferenceScope(LogIngestRequest request, string? clientDisplayName);
}

/// <summary>
/// Concrete implementation of A365 observability service using SDK scope APIs.
/// Creates InvokeAgentScope for request entry points and InferenceScope for LLM calls.
/// Returns null on any failure to ensure resilience.
/// </summary>
public sealed class Agent365ObservabilityService : IAgent365ObservabilityService
{
    private readonly TokenCredential _credential;
    private readonly ILogger<Agent365ObservabilityService> _logger;

    public Agent365ObservabilityService(
        TokenCredential credential,
        ILogger<Agent365ObservabilityService> logger)
    {
        _credential = credential;
        _logger = logger;
    }

    public IDisposable? StartInvokeAgentScope(
        string clientAppId,
        string tenantId,
        string? clientDisplayName,
        string? correlationId,
        string? promptContent = null)
    {
        try
        {
            var agentDetails = new AgentDetails(
                agentId: clientAppId,
                agentName: clientDisplayName ?? clientAppId);

            var endpoint = new Uri("https://apim.example.com"); // Placeholder endpoint
            var invokeAgentDetails = new InvokeAgentDetails(
                details: agentDetails,
                endpoint: endpoint,
                sessionId: correlationId ?? Guid.NewGuid().ToString());

            var tenantDetails = new TenantDetails(Guid.Parse(tenantId));

            var request = promptContent != null 
                ? new A365Request(content: promptContent) 
                : null;

            return InvokeAgentScope.Start(
                invokeAgentDetails: invokeAgentDetails,
                tenantDetails: tenantDetails,
                request: request,
                conversationId: correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start A365 InvokeAgentScope — continuing without observability");
            return null;
        }
    }

    public IDisposable? StartInferenceScope(LogIngestRequest request, string? clientDisplayName)
    {
        try
        {
            var agentDetails = new AgentDetails(
                agentId: request.ClientAppId,
                agentName: clientDisplayName ?? request.ClientAppId);

            var inferenceDetails = new InferenceCallDetails(
                operationName: InferenceOperationType.Chat,
                model: request.ResponseBody?.Model ?? "unknown",
                providerName: "AzureOpenAI",
                inputTokens: (int?)(request.ResponseBody?.Usage?.PromptTokens),
                outputTokens: (int?)(request.ResponseBody?.Usage?.CompletionTokens));

            var tenantDetails = new TenantDetails(Guid.Parse(request.TenantId));

            return InferenceScope.Start(
                inferenceDetails,
                agentDetails,
                tenantDetails);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start A365 InferenceScope — continuing without observability");
            return null;
        }
    }
}

/// <summary>
/// No-op implementation when A365 observability is disabled.
/// </summary>
public sealed class NoOpAgent365ObservabilityService : IAgent365ObservabilityService
{
    public IDisposable? StartInvokeAgentScope(string clientAppId, string tenantId, string? clientDisplayName, string? correlationId, string? promptContent = null) => null;
    public IDisposable? StartInferenceScope(LogIngestRequest request, string? clientDisplayName) => null;
}
