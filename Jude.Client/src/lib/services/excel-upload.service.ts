import type { UploadExcelResponse } from "../types/claim";
import { apiRequest, type ApiResponse } from "../utils/api";

export const uploadExcelClaims = async (
	file: File,
): Promise<ApiResponse<UploadExcelResponse>> => {
	const formData = new FormData();
	formData.append("file", file);

	return apiRequest<UploadExcelResponse>("/api/claims/upload-excel", {
		method: "POST",
		headers: {},
		body: formData as unknown as BodyInit,
	});
};

