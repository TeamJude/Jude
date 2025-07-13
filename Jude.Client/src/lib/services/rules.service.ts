import type { Rule, RuleStatus } from "../types/rule";
import { apiRequest, type ApiResponse } from "../utils/api";

const createRule = async (data: {
	name: string;
	description: string;
	status: RuleStatus;
	priority: number;
}): Promise<ApiResponse<Rule>> => {
	return apiRequest<Rule>("/api/rules", {
		method: "POST",
		body: JSON.stringify(data),
	});
};

const getRules = async (data: {
	page: number;
	pageSize: number;
}): Promise<ApiResponse<{ rules: Rule[]; totalCount: number }>> => {
	return apiRequest<{ rules: Rule[]; totalCount: number }>(
		`/api/rules?page=${data.page}&pageSize=${data.pageSize}`,
		{
			method: "GET",
		},
	);
};

export { createRule, getRules };
