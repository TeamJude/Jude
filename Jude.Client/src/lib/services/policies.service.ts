import type { Policy } from "../types/policy";
import { apiRequest, type ApiResponse } from "../utils/api";

const getPolicies = async (data: {
	page: number;
	pageSize: number;
}): Promise<ApiResponse<{ policies: Policy[]; totalCount: number }>> => {
	return apiRequest<{ policies: Policy[]; totalCount: number }>(
		`/api/policy?page=${data.page}&pageSize=${data.pageSize}`,
		{
			method: "GET",
		},
	);
};

const addPolicyDocument = async (formData: FormData): Promise<ApiResponse<Policy>> => {
	return apiRequest<Policy>("/api/policy/upload", {
		method: "POST",
		body: formData,
		headers:{}
	});
};

const getPolicyDocumentUrl = async (policyId: number): Promise<ApiResponse<{ url: string }>> => {
	return apiRequest<{ url: string }>(`/api/policy/${policyId}/document-url`, {
		method: "GET",
	});
};

export { addPolicyDocument, getPolicies, getPolicyDocumentUrl };
