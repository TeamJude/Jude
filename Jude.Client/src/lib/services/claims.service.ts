import { apiRequest } from "../utils/api";

export interface Claim {
    id: string;
    transactionNumber: string;
    patientName: string;
    membershipNumber: string;
    claimAmount: number;
    currency: string;
    approvedAmount?: number;
    providerPractice: string;
    status: "Pending" | "Processing" | "PendingReview" | "Approved" | "Rejected";
    source: "CIMAS" | "Manual";
    ingestedAt: string;
    updatedAt: string;
    submittedAt?: string;
    processedAt?: string;
    agentRecommendation?: string;
    agentReasoning?: string;
    agentConfidenceScore?: number;
    agentProcessedAt?: string;
    isFlagged: boolean;
    fraudIndicators?: string[];
    fraudRiskLevel: "Low" | "Medium" | "High" | "Critical";
    requiresHumanReview: boolean;
    finalDecision?: "Approve" | "Reject" | "RequestMoreInfo" | "Escalate";
    reviewerComments?: string;
    rejectionReason?: string;
    reviewedAt?: string;
    reviewedById?: string;
}

export interface ProcessingResult {
    totalIngested: number;
    totalProcessed: number;
    successful: number;
    failed: number;
    ingestedClaims: Array<{
        claimId: string;
        transactionNumber: string;
        patientName: string;
        claimAmount: number;
        status: string;
    }>;
    processingResults: Array<{
        claimId: string;
        success: boolean;
        recommendation?: string;
        confidence?: number;
        error?: string;
        processingTimeMs: number;
    }>;
}

export const claimsService = {
    async getInternalClaims(take: number = 10) {
        return apiRequest<Claim[]>(`/api/claims/internal?take=${take}`);
    },

    async getInternalClaim(id: string) {
        return apiRequest<Claim>(`/api/claims/internal/${id}`);
    },

    async ingestAndProcessClaims(practiceNumber: string = "0856207", maxClaims: number = 3) {
        return apiRequest<ProcessingResult>(`/api/orchestration/demo/ingest-and-process?practiceNumber=${practiceNumber}&maxClaims=${maxClaims}`, {
            method: "POST"
        });
    },

    async processSingleClaim(claimId: string) {
        return apiRequest(`/api/orchestration/process-claim/${claimId}`, {
            method: "POST"
        });
    },

    async getOrchestrationStats(fromDate?: string) {
        const query = fromDate ? `?fromDate=${fromDate}` : "";
        return apiRequest(`/api/orchestration/stats${query}`);
    }
}; 