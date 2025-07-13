import { config } from "../config";

export type ApiResponse<T = null> =
	| {
			data: T;
			success: true;
	  }
	| {
			success: false;
			errors: string[];
	  };

export const apiRequest = async <T>(
	endpoint: string,
	options: RequestInit = {},
): Promise<ApiResponse<T>> => {
	try {
		const url =
			config.environment == "production"
				? endpoint
				: `${config.url}${endpoint}`;
		const response = await fetch(url, {
			...options,
			credentials: "include",
			headers: {
				"Content-Type": "application/json",
				...options.headers,
			},
		});

		const data = await response.json();

		if (response.ok) {
			return {
				success: true,
				data: data,
			};
		} else {
			return {
				success: false,
				errors: Array.isArray(data) ? data : [data.message || "Request failed"],
			};
		}
	} catch (error) {
		return {
			success: false,
			errors: [(error as Error).message || "API request failed"],
		};
	}
};
