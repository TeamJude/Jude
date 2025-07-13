import type { FraudIndicator, IndicatorStatus } from "../types/fraud";
import { apiRequest, type ApiResponse } from "../utils/api";

const createFraudIndicator = async (data: {
	name: string;
	description: string;
	status: IndicatorStatus;
}): Promise<ApiResponse<FraudIndicator>> => {
	return apiRequest<FraudIndicator>("/api/fraud", {
		method: "POST",
		body: JSON.stringify(data),
	});
};

const getFraudIndicators = async (data: {
	page: number;
	pageSize: number;
}): Promise<
	ApiResponse<{ fraudIndicators: FraudIndicator[]; totalCount: number }>
> => {
	return apiRequest<{ fraudIndicators: FraudIndicator[]; totalCount: number }>(
		`/api/fraud?page=${data.page}&pageSize=${data.pageSize}`,
		{
			method: "GET",
		},
	);
};

export { createFraudIndicator, getFraudIndicators };
