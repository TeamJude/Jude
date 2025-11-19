import {
	ClaimsDashboardPeriod,
	ClaimStatus,
	ClaimSearchField,
    type GetClaimDetailResponse,
	type ClaimReview,
	type ClaimSummary,
	type CreateClaimReviewRequest,
	type UpdateClaimReviewRequest,
	type SubmitHumanReviewRequest,
	type SubmitHumanReviewResponse,
	type GetClaimAuditLogsResponse
} from "../types/claim";
import { apiRequest, type ApiResponse } from "../utils/api";

const getClaims = async (data: {
	page?: number;
	pageSize?: number;
	status?: ClaimStatus[];
	search?: string;
	searchField?: ClaimSearchField;
}): Promise<
	ApiResponse<{
		claims: ClaimSummary[];
		totalCount: number;
	}>
> => {
	const searchParams = new URLSearchParams();
	Object.entries(data).forEach(([key, value]) => {
		if (value !== undefined && value !== null) {
			if (Array.isArray(value)) {
				value.forEach((item) => {
					if (item !== undefined && item !== null) {
						if (key === "status") {
							searchParams.append(key, ClaimStatus[item as ClaimStatus]);
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

const getClaim = async (claimId: string): Promise<ApiResponse<GetClaimDetailResponse>> => {
    return apiRequest<GetClaimDetailResponse>(`/api/claims/${claimId}`, {
		method: "GET",
	});
};

const getClaimsDashboard = async (period: ClaimsDashboardPeriod) => {
	return apiRequest<{
		totalClaims: number;
		autoApprovedRate: number;
		claimsPendingReview: number;
		claimsFlagged: number;
		totalClaimsChangePercent: number;
		autoApprovedRateChangePercent: number;
		claimsPendingReviewChangePercent: number;
		claimsFlaggedChangePercent: number;
		claimsActivity: Array<{
			day: string;
			newClaims: number;
			processed: number;
			approved: number;
			rejected: number;
		}>;
	}>(`/api/claims/dashboard?period=${period}`, {
		method: "GET",
	});
};

const createReview = async (data: CreateClaimReviewRequest): Promise<ApiResponse<ClaimReview>> => {
	return apiRequest<ClaimReview>("/api/claims/reviews", {
		method: "POST",
		body: JSON.stringify(data),
	});
};

const updateReview = async (reviewId: string, data: UpdateClaimReviewRequest): Promise<ApiResponse<ClaimReview>> => {
	return apiRequest<ClaimReview>(`/api/claims/reviews/${reviewId}`, {
		method: "PUT",
		body: JSON.stringify(data),
	});
};

const submitReview = async (reviewId: string): Promise<ApiResponse<boolean>> => {
	return apiRequest<boolean>(`/api/claims/reviews/${reviewId}/submit`, {
		method: "POST",
	});
};

const getUserReviewForClaim = async (claimId: string): Promise<ApiResponse<ClaimReview | null>> => {
	return apiRequest<ClaimReview | null>(`/api/claims/${claimId}/my-review`, {
		method: "GET",
	});
};

const submitHumanReview = async (
	claimId: string,
	data: SubmitHumanReviewRequest
): Promise<ApiResponse<SubmitHumanReviewResponse>> => {
	return apiRequest<SubmitHumanReviewResponse>(`/api/claims/${claimId}/human-review`, {
		method: "POST",
		body: JSON.stringify(data),
	});
};

const getClaimAuditLogs = async (
	claimId: string
): Promise<ApiResponse<GetClaimAuditLogsResponse>> => {
	return apiRequest<GetClaimAuditLogsResponse>(`/api/claims/${claimId}/audit-logs`, {
		method: "GET",
	});
};

export { createReview, getClaim, getClaims, getClaimsDashboard, getClaimAuditLogs, getUserReviewForClaim, submitReview, submitHumanReview, updateReview };

