import { apiRequest, type ApiResponse } from "../utils/api";

export interface AgentReviewResult {
	id: string;
	reviewedAt: string;
	decision: 1 | 2; // 1 for Approve, 2 for Reject
	recommendation: string;
	reasoning: string;
	confidenceScore: number;
}

const testAgent = async (claimData: string): Promise<ApiResponse<AgentReviewResult>> => {
    return apiRequest<AgentReviewResult>("/api/agents", {
		method: "POST",
		body: JSON.stringify(claimData),
	});
};

export interface ExtractResponse {
    content: string;
}

const extractClaim = async (file: File): Promise<ApiResponse<ExtractResponse>> => {
    const formData = new FormData();
    formData.append("file", file);
    return apiRequest<ExtractResponse>("/api/agents/extract", {
        method: "POST",
        // Important: do not set Content-Type so browser can set multipart boundary
        headers: {},
        body: formData as unknown as BodyInit,
    });
};

export interface UploadClaimResponse {
    id: string;
    transactionNumber: string;
    patientFirstName: string;
    patientSurname: string;
    medicalSchemeName: string;
    totalClaimAmount: number;
    status: number;
    source: number;
    ingestedAt: string;
}

const uploadClaim = async (file: File): Promise<ApiResponse<UploadClaimResponse>> => {
    const formData = new FormData();
    formData.append("file", file);
    return apiRequest<UploadClaimResponse>("/api/agents/upload", {
        method: "POST",
        // Important: do not set Content-Type so browser can set multipart boundary
        headers: {},
        body: formData as unknown as BodyInit,
    });
};

export { testAgent, extractClaim, uploadClaim };
