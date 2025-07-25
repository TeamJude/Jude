using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;

namespace Jude.Server.Domains.Policies;

public interface IPolicyContext
{
    Task<Result<string>> Ingest(Stream document, TagCollection tags);
    Task<SearchResult> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public class PolicyContext : IPolicyContext
{
    private readonly MemoryServerless _memory;
    private readonly ILogger<PolicyContext> _logger;

    public PolicyContext(ILogger<PolicyContext> logger)
    {
        _logger = logger;
        _memory = new KernelMemoryBuilder()
            .WithSimpleQueuesPipeline()
            .With(new KernelMemoryConfig { DefaultIndexName = "jude" })
            .WithPostgresMemoryDb(AppConfig.VectorDbUrl)
            .WithSimpleFileStorage("C:\\Users\\devma\\Desktop\\policies")
            .WithAzureOpenAITextEmbeddingGeneration(
                new()
                {
                    Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                    APIKey = AppConfig.CustomAzureAI.ApiKey,
                    APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                    Deployment = AppConfig.CustomAzureAI.ModelId,
                    Endpoint = AppConfig.CustomAzureAI.Endpoint,
                    EmbeddingDimensions = 1536,
                }
            )
            .WithAzureOpenAITextGeneration(
                new()
                {
                    Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                    APIKey = AppConfig.Azure.AI.ApiKey,
                    APIType = AzureOpenAIConfig.APITypes.TextCompletion,
                    Deployment = AppConfig.Azure.AI.ModelId,
                    Endpoint = AppConfig.Azure.AI.Endpoint,
                    MaxTokenTotal = 16768,
                }
            )
            .WithSearchClientConfig(new() { AnswerTokens = 16768, MaxAskPromptSize = 8000 })
            .Build<MemoryServerless>();
    }

    public async Task<Result<string>> Ingest(Stream document, TagCollection tags)
    {
        _logger.LogInformation("Starting document ingestion with tags: {Tags}", tags);
        var docId = await _memory.ImportDocumentAsync(document, tags: tags);

        if (docId == null)
        {
            _logger.LogError("Failed to ingest document.");
            return Result.Fail("Failed to ingest document.");
        }

        _logger.LogInformation("Document ingested successfully. DocumentId: {DocumentId}", docId);
        return Result.Ok(docId);
    }

    public async Task<SearchResult> SearchAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Searching policies with query: {Query}", query);
        var result = await _memory.SearchAsync(query, cancellationToken: cancellationToken);
        _logger.LogInformation("Search completed for query: {Query}", query);
        return result;
    }
}
