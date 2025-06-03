import type { User } from "@lib/types/user";
import { apiRequest, type ApiResponse } from "@lib/utils/api";

const login = (data: {
	userIdentifier: string;
	password: string;
}): Promise<ApiResponse<User>> => {
	return apiRequest<User>("/api/auth/login", {
		method: "POST",
		body: JSON.stringify(data),
	});
}

const me = (): Promise<ApiResponse<User>> => {
    return apiRequest<User>("/api/auth/me", {
        method: "GET",
    });
}

export {
	login,
	me
};
