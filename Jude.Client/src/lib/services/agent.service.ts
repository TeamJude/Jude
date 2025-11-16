import { apiRequest, type ApiResponse } from "../utils/api";

export interface AgentReviewResult {
	id: string;
	reviewedAt: string;
	decision: 1 | 2;
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

export { testAgent };
