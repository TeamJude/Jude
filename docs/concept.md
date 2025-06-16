# Project Jude: AI-Powered Claims Adjudication Platform

## 1. Introduction & Vision

Project Jude is an advanced, AI-powered claims adjudication platform designed for CIMAS, a medical insurance company in Zimbabwe. The core vision is to streamline and enhance the accuracy, consistency, and efficiency of the medical claims processing lifecycle. Jude will leverage cutting-edge AI, including Large Language Models (LLMs) and intelligent agent frameworks, to make informed decisions and recommendations on claims, which are then subject to human review and approval. This "human-in-the-loop" approach not only ensures oversight but also provides valuable feedback for continuous model improvement.

## 2. Goals

- **Automate Claim Adjudication:** Significantly reduce manual effort in processing claims by an AI agent.
- **Improve Decision Accuracy:** Leverage historical data, company policies, and dynamic rules for more precise claim assessments.
- **Ensure Consistency:** Apply adjudication rules and policies uniformly across all claims.
- **Enhance Efficiency:** Speed up the claims processing turnaround time.
- **Continuous Learning:** Implement a feedback loop where human review results are used to fine-tune the AI model, making it progressively better.
- **Transparency:** Provide clear reasoning (where possible) for AI agent decisions to human reviewers.
- **Data-Driven Insights:** Enable CIMAS to better understand claim patterns and policy effectiveness.

## 3. System Architecture & Process Flow

The system revolves around an AI Agent that processes claims received from the CIMAS API. This agent leverages several data sources and tools to make its decisions.

**(Based on the provided diagram, with the ingestion modification)**

```mermaid
graph TD
    subgraph ExternalSystems["External Systems"]
        CIMAS_API["CIMAS API"]
    end

    subgraph JudePlatform["Jude Platform"]
        A[Ingest & Pre-process Claim from CIMAS API] --> B{AI Agent (Semantic Kernel)};

        subgraph KnowledgeSources["Knowledge & Rule Sources"]
            CIMAS_POLICY["CIMAS POLICY and <br/> Fraud Criteria <br/> (via Kernel Memory)"] -->|Use Claim Policies| B;
            CLAIMS_RULES["Claims Processing Rules <br/> (Dynamic, Admin Configured)"] -->|Apply Dynamic Rules| B;
            CIMAS_API_HISTORICAL["CIMAS API <br/> (Historical Data)"] -->|Get Historical Data| B;
        end

        B -->|Agent Decision & Recommendation| D[Human Review Queue];
        E[Model (Azure OpenAI GPT-4.1 - Fine-tuned)] -->|Provides Intelligence| B;
        D --> F[Human Review Interface];
        F -->|Human Approved/Modified Decision| G[Finalized Claim Decision];
        F -->|Review Results & Feedback| H[Model Fine-Tuning Process];
        H --> E;
    end

    CIMAS_API_HISTORICAL_FT[Historical Claims Data <br/> (from CIMAS)] --> H;
```

**Process Flow Details:**

1.  **Claim Ingestion:**

    - Jude directly interfaces with the **CIMAS API** to receive new medical claims.
    - Initial data extraction and structuring from the API payload occurs.
    - Basic data verification might be performed.

2.  **AI Agent Adjudication (Powered by Semantic Kernel):**

    - The AI Agent is triggered for each new claim.
    - **Policy Consultation:** The Agent queries **Kernel Memory**, which has ingested CIMAS policy documents and fraud criteria. This allows the Agent to "talk to" the policy data and understand relevant guidelines for the specific claim.
    - **Rule Application:** The Agent applies a set of **dynamic claim processing rules**. These rules can be configured by administrators via a dashboard, allowing for flexibility and quick adaptation to changing requirements.
    - **Historical Context:** The Agent can access **historical claims data** ( via the CIMAS API or a synchronized internal store) to understand precedents, typical award amounts for similar cases, and member history.
    - **Membership & Subscription Analysis:** The Agent considers the claimant's membership level and subscription benefits.
    - **Decision Making:** Based on all the above inputs, the Agent, powered by a fine-tuned Azure OpenAI GPT-4.1 model, formulates a decision or recommendation (e.g., amount to be awarded, approval/rejection, request for more information).

3.  **Human Review & Approval:**

    - The Agent's decision, along with supporting reasoning, is sent to a **Human Review Queue**.
    - Human adjudicators access these claims via a **React-based client interface**.
    - They review the Agent's recommendation, the claim details, and can approve, modify, or reject the Agent's decision.

4.  **Model Fine-Tuning (Continuous Improvement):**
    - **Initial Fine-Tuning:** A large corpus of historical claims data from previous years (provided by CIMAS) will be used for initial fine-tuning of the Azure OpenAI GPT-4.1 foundation model. This customizes the model specifically for CIMAS's claims processing nuances.
    - **Ongoing Fine-Tuning:** The results and feedback from the human review stage (i.e., how human adjudicators corrected or confirmed the AI's decisions) are collected. This data is periodically used to further fine-tune the AI model, enabling it to learn from human expertise and improve its accuracy over time ("dogfooding" the AI's performance).

## 4. Technology Stack

- **Backend:**
  - **Language/Framework:** .NET9 C#
  - **AI Agent Framework:** Semantic Kernel
  - **Knowledge Base/RAG:** Kernel Memory (for ingesting and querying policy documents)
  - **AI Models:** Azure OpenAI Service (specifically GPT-4.1 or similar, with fine-tuning capabilities)
  - **Database:** (To be determined - e.g., PostgreSQL, SQL Server for storing application data, queue states, audit logs)
  - **Messaging Queue:** (To be determined - e.g., Dotnet Channels,for the human review queue)
- **Frontend:**
  - **Framework:** React
  - **State Management & Data Fetching:** Tanstack Query, Tanstack Router (or other Tanstack libraries as appropriate)
  - **UI Components:** ( e.g., Hero UI Tailwind CSS)
- **DevOps & Hosting:**
  - **Source Control:** Git (e.g., GitHub)
  - **CI/CD:** (To be determined - e.g., GitHub Actions, Azure DevOps)
  - **Hosting:** Azure (App Service, Azure Functions, Azure OpenAI, Azure SQL/PostgreSQL, Azure Service Bus)

## 5. Proposed Folder Structures

The proposed folder structures aim for a Vertical Slice Architecture, promoting feature-based organization for better cohesion and maintainability.

### 5.1. Client (jude.client)

```
jude.client/
├── public/                     # Static assets served directly
├── src/
│   ├── main.tsx                # App entry point (renders App.tsx)
│   ├── App.tsx                 # Root component: RouterProvider, global context providers (wraps with AppProviders)
│   ├── vite-env.d.ts           # Vite environment types
│   │
│   ├── assets/                 # Static assets like images, fonts (imported by components)
│   │
│   ├── components/             # Globally reusable UI components (NOT feature-specific)
│   │   ├── ui/                 # Primitive/atomic UI elements (e.g., Button.tsx, Input.tsx, Card.tsx)
│   │   ├── layout/             # Page shell layouts (e.g., DashboardLayout.tsx, AuthLayout.tsx)
│   │   └── common/             # More complex shared components (e.g., DataTable.tsx, UserAvatar.tsx, Notification.tsx)
│   │
│   ├── core/                   # SHARED/GLOBAL foundational code
│   │   ├── config/             # Application-wide configuration
│   │   │   └── appConfig.ts    # (e.g., API_BASE_URL, feature flags, default settings)
│   │   ├── hooks/              # Global, reusable custom React hooks (e.g., useAuth, useTheme)
│   │   ├── lib/                # Core libraries, instances, and low-level shared services
│   │   │   ├── apiClient.ts    # Configured HTTP client instance (e.g., Axios/fetch with interceptors)
│   │   │   ├── queryClient.ts  # Tanstack Query client instance and global config
│   │   │   └── i18n.ts         # Internationalization setup (if needed)
│   │   ├── providers/          # Global React Context providers wrapper component
│   │   │   └── AppProviders.tsx # Combines ThemeProvider, AuthProvider, QueryClientProvider etc.
│   │   ├── types/              # Global TypeScript definitions (shared across features)
│   │   │   └── index.ts        # (e.g., ApiError, PaginatedResponse, UserProfile)
│   │   └── utils/              # Global utility functions (not tied to a specific domain)
│   │       └── formatters.ts   # (e.g., currencyFormatter, dateFormatter)
│   │       └── validators.ts   # (e.g., emailValidator - often better within features)
│   │
│   ├── features/               # Feature-specific modules (Vertical Slices)
│   │   ├── auth/
│   │   │   ├── components/     # Auth-specific UI (e.g., LoginForm.tsx, RegistrationForm.tsx)
│   │   │   ├── hooks/          # e.g., useLogin.ts (wrapper around useMutation), useUserSession.ts
│   │   │   ├── services/       # authApi.ts (functions for login, logout using apiClient)
│   │   │   ├── store/          # authStore.ts (e.g., Zustand/Jotai for user session, auth state if not fully in React Query)
│   │   │   ├── types/          # index.ts (auth-related types, e.g., UserCredentials, AuthResponse)
│   │   │   └── utils/          # authUtils.ts (e.g., token management)
│   │   ├── claimsAdjudication/
│   │   │   ├── components/     # ClaimQueueItem.tsx, ClaimDetailView.tsx, AdjudicationForm.tsx
│   │   │   ├── hooks/          # useClaimsQueue.ts, useClaimDetails.ts, useSubmitAdjudication.ts
│   │   │   ├── services/       # claimsApi.ts (fetch claims, submit adjudication)
│   │   │   ├── types/          # index.ts (Claim, AdjudicationDecision, ClaimStatus types)
│   │   │   └── utils/          # claimDisplayUtils.ts
│   │   ├── knowledgeBaseAdmin/ # For admins to manage policy docs (upload, view status)
│   │   │   ├── components/     # DocumentUpload.tsx, DocumentList.tsx
│   │   │   ├── hooks/          # useKnowledgeDocuments.ts, useUploadDocument.ts
│   │   │   ├── services/       # knowledgeApi.ts
│   │   │   └── types/          # index.ts (DocumentMetadata)
│   │   ├── rulesEngineAdmin/   # For admins to manage dynamic processing rules
│   │   │   ├── components/     # RuleEditor.tsx, RuleList.tsx
│   │   │   ├── hooks/          # useRules.ts, useSaveRule.ts
│   │   │   ├── services/       # rulesApi.ts
│   │   │   └── types/          # index.ts (RuleDefinition)
│   │
│   ├── routes/                 # Tanstack Router file-based routing configuration
│   │   ├── __root.tsx          # Root layout component (Tanstack Router convention)
│   │   ├── _auth.layout.tsx    # Layout for authentication routes
│   │   │   └── login.route.tsx
│   │   │   └── register.route.tsx (if applicable)
│   │   ├── _protected.layout.tsx # Layout for routes requiring authentication
│   │   │   ├── index.route.tsx       # Dashboard landing page (e.g., maps to '/')
│   │   │   ├── claims-queue/
│   │   │   │   ├── index.route.tsx   # /protected/claims-queue (list)
│   │   │   │   └── $claimId.route.tsx # /protected/claims-queue/:claimId (detail)
│   │   │   ├── knowledge-base/
│   │   │   │   └── index.route.tsx
│   │   │   ├── rules-admin/
│   │   │   │   └── index.route.tsx
│   │   │   └── ...
│   │   ├── 404.tsx             # Catch-all for not found routes
│   │   └── index.tsx           # Optional: Public landing page if any (or redirect to login)
│
├── vite.config.ts
├── tsconfig.json
├── package.json
└── tailwind.config.js (if using Tailwind)
```

### 5.2. Server (jude.server)

```
jude.server/
├── jude.server.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Migrations/
│
├── Config/
│   └── AppConfig.cs
│   └── CorsConfig.cs
│   └── JwtConfig.cs
│
├── Data/
│   ├── Models/
│   │   └── UserModel.cs
│   │   └── ClaimModel.cs
│   │   └── ...
│   └── RepositoryContext.cs
│
├── Domains/
│   ├── Auth/
│   │   ├── AuthController.cs
│   │   ├── AuthService.cs
│   │   ├── IAuthService.cs
│   │   ├── Contracts/
│   │   │   ├── LoginRequest.cs
│   │   │   └── TokenResponse.cs
│   │   ├── Validators/
│   │   │   └── LoginRequestValidator.cs
│   │   └── Policies/
│   ├── Claims/
│   │   ├── ClaimsController.cs
│   │   ├── ClaimsService.cs
│   │   ├── IClaimsService.cs
│   │   ├── Contracts/
│   │   │   ├── SubmitClaimRequest.cs
│   │   │   └── ClaimDto.cs
│   │   ├── Validators/
│   │   └── ...
│   ├── Users/
│   │   └── ...
│   └── KnowledgeBase/
│       └── ...
│
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
│
├── Helpers/
│   └── Result.cs
│   └── PasswordHasher.cs
│
├── Middleware/
│   └── GlobalExceptionHandler.cs
│
└── Properties/
    └── launchSettings.json
```

**Rationale for Vertical Slice:**
This structure groups code by feature rather than by technical layer (e.g., all controllers in one folder, all services in another). This promotes:

- **High Cohesion:** Code related to a single feature is located together, making it easier to understand and modify.
- **Low Coupling:** Features are more independent, reducing the ripple effect of changes.(but dont go crazy trying to de couple it becomes impractical)
- **Better Scalability:** Easier for multiple developers/teams to work on different features concurrently.
- **Improved Discoverability:** Finding all code related to "Claims Adjudication" is straightforward.
