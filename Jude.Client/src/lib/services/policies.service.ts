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

export { getPolicies };