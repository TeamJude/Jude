import type {
	Claim,
	ClaimStatus,
	ClaimSummary,
	FraudRiskLevel,
} from "../types/claim";
import { apiRequest, type ApiResponse } from "../utils/api";

const getClaims = async (data: {
	page?: number;
	pageSize?: number;
	status?: ClaimStatus[];
	riskLevel?: FraudRiskLevel[];
	requiresHumanReview?: boolean;
	search?: string;
}): Promise<
	ApiResponse<{
		claims: ClaimSummary[];
		totalCount: number;
	}>
> => {
	const searchParams = new URLSearchParams();
	Object.entries(data).forEach(([key, value]) => {
		if (value !== undefined && value !== null && value !== "") {
			if (Array.isArray(value)) {
				value.forEach((item) => {
					if (item !== undefined && item !== null) {
						// Convert enum values to their string names for the API
						if (key === "status") {
							searchParams.append(key, ClaimStatus[item as ClaimStatus]);
						} else if (key === "riskLevel") {
							searchParams.append(key, FraudRiskLevel[item as FraudRiskLevel]);
						} else {
							searchParams.append(key, item.toString());
						}
					}
				});
			} else {
				searchParams.set(key, value.toString());
			}
		}
	});
	const queryString = searchParams.toString();
	const url = queryString ? `/api/claims?${queryString}` : "/api/claims";
	return apiRequest<{
		claims: ClaimSummary[];
		totalCount: number;
	}>(url, {
		method: "GET",
	});
};

const getClaim = async (claimId: string): Promise<ApiResponse<Claim>> => {
	return apiRequest<Claim>(`/api/claims/${claimId}`, {
		method: "GET",
	});
};

export { getClaim, getClaims };
