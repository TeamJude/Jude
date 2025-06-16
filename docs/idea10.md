# Jude AI Claims Adjudication System - Implementation Plan

## Overview
Jude is an AI-powered claims adjudication system that automates the decision-making process for medical insurance claims. The system integrates with CIMAS API to pull claims, processes them through an intelligent pipeline combining rules engine, fraud detection, RAG-based policy analysis, and Semantic Kernel AI agents to make adjudication decisions.

## Core Workflow
```
CIMAS API → Claim Ingestion → AI Adjudication Pipeline → Human Review → Final Decision → CIMAS API
```

## Detailed Folder Structure

```
Jude.Server/
├── Domains/
│   ├── Claims/
│   │   ├── Models/
│   │   │   ├── InternalClaim.cs
│   │   │   ├── ClaimStatus.cs
│   │   │   ├── ClaimMetadata.cs
│   │   │   └── ClaimHistory.cs
│   │   ├── Services/
│   │   │   ├── ClaimIngestionService.cs
│   │   │   ├── ClaimMappingService.cs
│   │   │   ├── ClaimValidationService.cs
│   │   │   └── ClaimSyncService.cs
│   │   ├── Providers/
│   │   │   └── CIMAS/
│   │   │       ├── CIMASProvider.cs
│   │   │       └── CIMASContracts.cs
│   │   ├── Repository/
│   │   │   └── ClaimsRepository.cs
│   │   └── ClaimsController.cs
│   │
│   ├── Adjudication/
│   │   ├── Models/
│   │   │   ├── AdjudicationRequest.cs
│   │   │   ├── AdjudicationDecision.cs
│   │   │   ├── AdjudicationResult.cs
│   │   │   ├── ReasoningLog.cs
│   │   │   ├── RiskAssessment.cs
│   │   │   ├── PolicyCitation.cs
│   │   │   └── HumanReview.cs
│   │   ├── Services/
│   │   │   ├── AdjudicationOrchestrator.cs
│   │   │   ├── ReviewQueueService.cs
│   │   │   ├── DecisionService.cs
│   │   │   └── NotificationService.cs
│   │   ├── Pipeline/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAdjudicationStep.cs
│   │   │   │   └── IPipelineContext.cs
│   │   │   ├── Steps/
│   │   │   │   ├── ClaimValidationStep.cs
│   │   │   │   ├── EligibilityCheckStep.cs
│   │   │   │   ├── RulesProcessingStep.cs
│   │   │   │   ├── FraudDetectionStep.cs
│   │   │   │   ├── PolicyAnalysisStep.cs
│   │   │   │   ├── AIDecisionStep.cs
│   │   │   │   ├── RiskAssessmentStep.cs
│   │   │   │   └── HumanReviewStep.cs
│   │   │   ├── PipelineOrchestrator.cs
│   │   │   └── PipelineContext.cs
│   │   ├── Repository/
│   │   │   ├── AdjudicationRepository.cs
│   │   │   └── ReviewRepository.cs
│   │   └── AdjudicationController.cs
│   │
│   ├── Rules/
│   │   ├── Models/
│   │   │   ├── Rule.cs
│   │   │   ├── RuleSet.cs
│   │   │   ├── RuleCondition.cs
│   │   │   ├── RuleAction.cs
│   │   │   ├── RuleContext.cs
│   │   │   └── RuleExecutionResult.cs
│   │   ├── Engine/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IRulesEngine.cs
│   │   │   │   ├── IRuleEvaluator.cs
│   │   │   │   └── IRuleValidator.cs
│   │   │   ├── RulesEngine.cs
│   │   │   ├── RuleEvaluator.cs
│   │   │   ├── RuleValidator.cs
│   │   │   └── RuleExpressionParser.cs
│   │   ├── Types/
│   │   │   ├── EligibilityRules.cs
│   │   │   ├── CoverageRules.cs
│   │   │   ├── LimitRules.cs
│   │   │   ├── ExclusionRules.cs
│   │   │   └── FraudRules.cs
│   │   ├── Repository/
│   │   │   └── RulesRepository.cs
│   │   ├── Services/
│   │   │   ├── RuleManagementService.cs
│   │   │   └── RuleTestingService.cs
│   │   └── RulesController.cs
│   │
│   ├── Intelligence/
│   │   ├── Models/
│   │   │   ├── AgentRequest.cs
│   │   │   ├── AgentResponse.cs
│   │   │   ├── ReasoningStep.cs
│   │   │   ├── Confidence.cs
│   │   │   ├── FraudSignal.cs
│   │   │   └── PolicyDocument.cs
│   │   ├── Agent/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAdjudicationAgent.cs
│   │   │   │   ├── IFraudDetectionAgent.cs
│   │   │   │   └── IPolicyAnalysisAgent.cs
│   │   │   ├── Kernel/
│   │   │   │   ├── SemanticKernelSetup.cs
│   │   │   │   ├── KernelPlugins.cs
│   │   │   │   └── KernelMemory.cs
│   │   │   ├── Plugins/
│   │   │   │   ├── ClaimAnalysisPlugin.cs
│   │   │   │   ├── PolicyLookupPlugin.cs
│   │   │   │   ├── FraudDetectionPlugin.cs
│   │   │   │   ├── RulesEnginePlugin.cs
│   │   │   │   └── CalculationPlugin.cs
│   │   │   ├── Agents/
│   │   │   │   ├── ClaimAnalysisAgent.cs
│   │   │   │   ├── FraudDetectionAgent.cs
│   │   │   │   ├── PolicyAnalysisAgent.cs
│   │   │   │   └── RiskAssessmentAgent.cs
│   │   │   └── Prompts/
│   │   │       ├── SystemPrompts.cs
│   │   │       ├── ClaimAnalysisPrompts.cs
│   │   │       ├── FraudDetectionPrompts.cs
│   │   │       └── PolicyAnalysisPrompts.cs
│   │   ├── RAG/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IVectorStore.cs
│   │   │   │   ├── IDocumentRetriever.cs
│   │   │   │   └── IEmbeddingService.cs
│   │   │   ├── Services/
│   │   │   │   ├── VectorStoreService.cs
│   │   │   │   ├── PolicyRetriever.cs
│   │   │   │   ├── ContextBuilder.cs
│   │   │   │   └── EmbeddingService.cs
│   │   │   ├── Models/
│   │   │   │   ├── DocumentChunk.cs
│   │   │   │   ├── RetrievalContext.cs
│   │   │   │   ├── SearchResult.cs
│   │   │   │   └── VectorSearchQuery.cs
│   │   │   └── Configuration/
│   │   │       └── RAGConfiguration.cs
│   │   ├── Fraud/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IFraudDetector.cs
│   │   │   │   └── IFraudAnalyzer.cs
│   │   │   ├── Detectors/
│   │   │   │   ├── DuplicateClaimDetector.cs
│   │   │   │   ├── PatternAnomalyDetector.cs
│   │   │   │   ├── ProviderFraudDetector.cs
│   │   │   │   └── MemberBehaviorDetector.cs
│   │   │   ├── Models/
│   │   │   │   ├── FraudIndicator.cs
│   │   │   │   ├── FraudScore.cs
│   │   │   │   └── FraudPattern.cs
│   │   │   └── Services/
│   │   │       ├── FraudAnalysisService.cs
│   │   │       └── FraudReportingService.cs
│   │   ├── Services/
│   │   │   ├── IntelligenceOrchestrator.cs
│   │   │   └── ModelManagementService.cs
│   │   └── IntelligenceController.cs
│   │
│   ├── Documents/
│   │   ├── Models/
│   │   │   ├── PolicyDocument.cs
│   │   │   ├── DocumentMetadata.cs
│   │   │   ├── DocumentVersion.cs
│   │   │   └── DocumentIndex.cs
│   │   ├── Services/
│   │   │   ├── DocumentManagementService.cs
│   │   │   ├── DocumentIndexingService.cs
│   │   │   ├── DocumentVersioningService.cs
│   │   │   └── DocumentSearchService.cs
│   │   ├── Storage/
│   │   │   ├── Interfaces/
│   │   │   │   └── IDocumentStorage.cs
│   │   │   ├── BlobDocumentStorage.cs
│   │   │   └── LocalDocumentStorage.cs
│   │   ├── Repository/
│   │   │   └── DocumentRepository.cs
│   │   └── DocumentsController.cs
│   │
│   └── Audit/
│       ├── Models/
│       │   ├── AuditLog.cs
│       │   ├── DecisionAudit.cs
│       │   ├── UserAction.cs
│       │   └── SystemEvent.cs
│       ├── Services/
│       │   ├── AuditService.cs
│       │   ├── ComplianceReportingService.cs
│       │   └── AuditQueryService.cs
│       ├── Repository/
│       │   └── AuditRepository.cs
│       └── AuditController.cs
│
├── Core/
│   ├── Helpers/
│   └── Constants/
│       ├── AdjudicationConstants.cs
│       ├── FraudConstants.cs
│       └── PolicyConstants.cs
│
├── Data/
│   ├── Models/
│   │   ├── ClaimEntity.cs
│   │   ├── AdjudicationEntity.cs
│   │   ├── RuleEntity.cs
│   │   ├── PolicyDocumentEntity.cs
│   │   └── AuditLogEntity.cs
│   └── Repository/
│
├── Config/
│   ├── SemanticKernelConfig.cs
│   ├── RAGConfig.cs
│   └── IntelligenceConfig.cs
│
└── Extensions/
    ├── SemanticKernelExtensions.cs
    ├── IntelligenceExtensions.cs
    └── RAGExtensions.cs
```

## Technology Stack Integration

### Semantic Kernel Components
```csharp
// Semantic Kernel setup for AI agents
public class SemanticKernelSetup
{
    public static Kernel CreateKernel(IConfiguration config)
    {
        var builder = Kernel.CreateBuilder();
        
        // Add OpenAI chat completion
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4",
            apiKey: config["OpenAI:ApiKey"]
        );
        
        // Add plugins
        builder.Plugins.AddFromType<ClaimAnalysisPlugin>();
        builder.Plugins.AddFromType<PolicyLookupPlugin>();
        builder.Plugins.AddFromType<FraudDetectionPlugin>();
        builder.Plugins.AddFromType<RulesEnginePlugin>();
        
        // Add memory for RAG
        builder.AddOpenAITextEmbeddingGeneration(
            modelId: "text-embedding-ada-002",
            apiKey: config["OpenAI:ApiKey"]
        );
        
        return builder.Build();
    }
}
```

### Core Models and Interfaces

#### Adjudication Models
```csharp
// Adjudication/Models/AdjudicationRequest.cs
public class AdjudicationRequest
{
    public Guid ClaimId { get; set; }
    public InternalClaim Claim { get; set; }
    public Member Member { get; set; }
    public Provider Provider { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public AdjudicationSettings Settings { get; set; }
}

// Adjudication/Models/AdjudicationDecision.cs
public class AdjudicationDecision
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public DecisionType Decision { get; set; } // Approve, Deny, Review
    public decimal? ApprovedAmount { get; set; }
    public decimal? DeniedAmount { get; set; }
    public string Reasoning { get; set; }
    public List<ReasoningStep> ReasoningSteps { get; set; }
    public List<PolicyCitation> Citations { get; set; }
    public RiskAssessment RiskAssessment { get; set; }
    public double ConfidenceScore { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ProcessedBy { get; set; } // AI Agent name
}

// Adjudication/Models/ReasoningLog.cs
public class ReasoningLog
{
    public Guid Id { get; set; }
    public List<ReasoningStep> Steps { get; set; }
    public string FinalReasoning { get; set; }
    public Dictionary<string, object> Evidence { get; set; }
}

public class ReasoningStep
{
    public int StepNumber { get; set; }
    public string Category { get; set; } // Rules, Policy, Fraud, Risk
    public string Description { get; set; }
    public string Evidence { get; set; }
    public double Impact { get; set; } // -1 to 1 (negative to positive impact)
}
```

#### Intelligence Models
```csharp
// Intelligence/Models/AgentRequest.cs
public class AgentRequest
{
    public string AgentType { get; set; }
    public InternalClaim Claim { get; set; }
    public Member Member { get; set; }
    public List<Rule> ApplicableRules { get; set; }
    public List<PolicyDocument> RelevantPolicies { get; set; }
    public Dictionary<string, object> Context { get; set; }
}

// Intelligence/Models/AgentResponse.cs
public class AgentResponse
{
    public string AgentId { get; set; }
    public DecisionType RecommendedDecision { get; set; }
    public string Reasoning { get; set; }
    public List<ReasoningStep> ReasoningSteps { get; set; }
    public double Confidence { get; set; }
    public RiskAssessment Risk { get; set; }
    public List<PolicyCitation> Citations { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

### Semantic Kernel Plugins

#### Claim Analysis Plugin
```csharp
// Intelligence/Agent/Plugins/ClaimAnalysisPlugin.cs
public class ClaimAnalysisPlugin
{
    [KernelFunction("analyze_claim_eligibility")]
    [Description("Analyzes claim eligibility based on member benefits and policy rules")]
    public async Task<string> AnalyzeEligibility(
        [Description("The claim to analyze")] string claimData,
        [Description("Member benefit information")] string memberBenefits,
        [Description("Applicable policy rules")] string policyRules
    )
    {
        // Implementation
    }

    [KernelFunction("calculate_benefit_amount")]
    [Description("Calculates the benefit amount based on coverage rules")]
    public async Task<decimal> CalculateBenefitAmount(
        [Description("Claim amount")] decimal claimAmount,
        [Description("Coverage percentage")] decimal coveragePercentage,
        [Description("Deductible amount")] decimal deductible,
        [Description("Annual limit remaining")] decimal annualLimitRemaining
    )
    {
        // Implementation
    }
}

// Intelligence/Agent/Plugins/PolicyLookupPlugin.cs
public class PolicyLookupPlugin
{
    private readonly IDocumentRetriever _documentRetriever;

    [KernelFunction("search_policy_documents")]
    [Description("Searches policy documents for relevant coverage information")]
    public async Task<string> SearchPolicyDocuments(
        [Description("Search query for policy documents")] string query,
        [Description("Document type filter")] string documentType = "all"
    )
    {
        var results = await _documentRetriever.SearchAsync(query, documentType);
        return JsonSerializer.Serialize(results);
    }
}
```

### Pipeline Implementation

#### Pipeline Orchestrator
```csharp
// Adjudication/Pipeline/PipelineOrchestrator.cs
public class PipelineOrchestrator
{
    private readonly List<IAdjudicationStep> _steps;
    private readonly ILogger<PipelineOrchestrator> _logger;
    private readonly IAuditService _auditService;

    public async Task<AdjudicationResult> ProcessAsync(AdjudicationRequest request)
    {
        var context = new PipelineContext(request);
        
        try
        {
            foreach (var step in _steps)
            {
                _logger.LogInformation("Executing step: {StepName}", step.GetType().Name);
                
                context = await step.ExecuteAsync(context);
                
                // Audit each step
                await _auditService.LogStepExecution(
                    context.ClaimId, 
                    step.GetType().Name, 
                    context.CurrentDecision,
                    context.StepResults.LastOrDefault()
                );
                
                if (context.ShouldTerminate)
                {
                    _logger.LogInformation("Pipeline terminated at step: {StepName}", step.GetType().Name);
                    break;
                }
            }
            
            return context.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline execution failed for claim {ClaimId}", request.ClaimId);
            throw;
        }
    }
}
```

#### AI Decision Step with Semantic Kernel
```csharp
// Adjudication/Pipeline/Steps/AIDecisionStep.cs
public class AIDecisionStep : IAdjudicationStep
{
    private readonly Kernel _kernel;
    private readonly ILogger<AIDecisionStep> _logger;

    public async Task<PipelineContext> ExecuteAsync(PipelineContext context)
    {
        var prompt = BuildDecisionPrompt(context);
        
        var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments
        {
            ["claim"] = JsonSerializer.Serialize(context.Claim),
            ["member"] = JsonSerializer.Serialize(context.Member),
            ["rules_results"] = JsonSerializer.Serialize(context.RulesResults),
            ["fraud_signals"] = JsonSerializer.Serialize(context.FraudSignals),
            ["policy_context"] = JsonSerializer.Serialize(context.PolicyContext)
        });

        var decision = ParseAIDecision(result.ToString());
        context.AIDecision = decision;
        
        return context;
    }

    private string BuildDecisionPrompt(PipelineContext context)
    {
        return $"""
            You are an expert medical claims adjudicator. Analyze the following claim and make a decision.
            
            CLAIM INFORMATION:
            {{$claim}}
            
            MEMBER INFORMATION:
            {{$member}}
            
            RULES ENGINE RESULTS:
            {{$rules_results}}
            
            FRAUD ANALYSIS:
            {{$fraud_signals}}
            
            RELEVANT POLICY CONTEXT:
            {{$policy_context}}
            
            Based on this information, provide your decision in the following JSON format:
            {{
                "decision": "APPROVE|DENY|REVIEW",
                "approved_amount": number or null,
                "confidence": number between 0 and 1,
                "reasoning": "detailed explanation",
                "reasoning_steps": [
                    {{
                        "category": "eligibility|coverage|fraud|policy",
                        "description": "step description",
                        "evidence": "supporting evidence",
                        "impact": number between -1 and 1
                    }}
                ],
                "risk_score": number between 0 and 1,
                "citations": [
                    {{
                        "document": "policy document name",
                        "section": "relevant section",
                        "text": "quoted text"
                    }}
                ]
            }}
            """;
    }
}
```

## Implementation Phases

### Phase 1: Foundation (Weeks 1-2)
- Set up enhanced folder structure
- Create core models and interfaces
- Implement basic Semantic Kernel setup
- Set up database schema extensions

### Phase 2: Rules Engine (Weeks 3-4)
- Implement rules engine core
- Create rule management system
- Build rule testing framework
- Integrate with fraud detection rules

### Phase 3: Intelligence Foundation (Weeks 5-6)
- Set up Semantic Kernel with plugins
- Implement RAG system with vector storage
- Create document indexing and retrieval
- Build basic AI agents

### Phase 4: Pipeline Implementation (Weeks 7-8)
- Build adjudication pipeline steps
- Implement pipeline orchestrator
- Create human review queue
- Add comprehensive audit logging

### Phase 5: Integration & Testing (Weeks 9-10)
- End-to-end pipeline testing
- CIMAS API integration
- Performance optimization
- Security implementation

### Phase 6: UI & Production (Weeks 11-12)
- Human review interface
- Admin dashboards
- Production deployment
- Monitoring and observability

## Key Configuration Files

### Semantic Kernel Configuration
```json
// appsettings.json
{
  "SemanticKernel": {
    "OpenAI": {
      "ApiKey": "your-openai-key",
      "ChatModel": "gpt-4",
      "EmbeddingModel": "text-embedding-ada-002"
    },
    "Plugins": {
      "ClaimAnalysis": { "Enabled": true },
      "PolicyLookup": { "Enabled": true },
      "FraudDetection": { "Enabled": true },
      "RulesEngine": { "Enabled": true }
    }
  },
  "RAG": {
    "VectorStore": {
      "Provider": "Qdrant",
      "ConnectionString": "http://localhost:6333",
      "CollectionName": "policy_documents"
    },
    "ChunkSize": 1000,
    "ChunkOverlap": 200
  },
  "Adjudication": {
    "Pipeline": {
      "TimeoutMinutes": 5,
      "MaxRetries": 3
    },
    "HumanReview": {
      "RequiredForHighRisk": true,
      "RiskThreshold": 0.7
    }
  }
}
```

This implementation plan provides a comprehensive roadmap for building the AI-powered claims adjudication system using Semantic Kernel as the agent framework, following your vertical slice architecture pattern. 