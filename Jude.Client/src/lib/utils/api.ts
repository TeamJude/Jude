import { config } from "../config";

export type ApiResponse<T = null> = {
	data: T;
	success: true;
} | {
	success: false;
	errors: string[];
};

export const apiRequest = async <T>(
	endpoint: string,
	options: RequestInit = {},
): Promise<ApiResponse<T>> => {
	try {
		const url = `${config.url}${endpoint}`;
		const response = await fetch(url, {
			...options,
			credentials: "include",
			headers: {
				"Content-Type": "application/json",
				...options.headers,
			},
		});

		const data = await response.json();
		return data;

	} catch (error) {
		return {
			success: false,
			errors: [(error as Error).message || "API request failed"],
		};
	}
};
