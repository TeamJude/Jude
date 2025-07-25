using Jude.Server.Config;
using Jude.Server.Core.Helpers;
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

    public PolicyContext()
    {
        _memory = new KernelMemoryBuilder()
            .WithSimpleVectorDb()
            .WithSimpleQueuesPipeline()
            .With(new KernelMemoryConfig { DefaultIndexName = "jude" })
            .WithAzureBlobsDocumentStorage(
                new()
                {
                    Auth = AzureBlobsConfig.AuthTypes.ConnectionString,
                    ConnectionString = AppConfig.Azure.Blob.ConnectionString,
                }
            )
            .WithAzureAISearchMemoryDb(
                new()
                {
                    Auth = AzureAISearchConfig.AuthTypes.APIKey,
                    APIKey = AppConfig.Azure.AI.ApiKey,
                    Endpoint = AppConfig.Azure.AI.Endpoint,
                    UseHybridSearch = false,
                }
            )
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
            .Build<MemoryServerless>(
                //this is fine because i am not storing any documents at the moment
                new() { AllowMixingVolatileAndPersistentData = true }
            );
    }

    public async Task<Result<string>> Ingest(Stream document, TagCollection tags)
    {
        var docId = await _memory.ImportDocumentAsync(document, tags: tags);

        if (docId == null)
        {
            return Result.Fail("Failed to ingest document.");
        }

        return Result.Ok(docId);
    }

    public async Task<SearchResult> SearchAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        return await _memory.SearchAsync(query, cancellationToken: cancellationToken);
    }
}
