export type User = {
	id: string;
	username: string;
	email: string;
	avatarUrl: string;
	authProvider: AuthProvider;
};

export type AuthProvider = "email" | "google";
