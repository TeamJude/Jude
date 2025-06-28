export enum Permission {
	Read ,
	Write 
}

export interface UserRole {
	name: string;
	permissions: Record<string, Permission>;
}

export interface User {
	id: string;
	email: string;
	username: string | null;
	avatarUrl: string | null;
	createdAt: string;
	role: UserRole;
}

export interface AuthResponse {
	token: string;
	userData: User;
}

export interface LoginRequest {
	userIdentifier: string;
	password: string;
}

export interface RegisterRequest {
	username: string;
	email: string;
	password: string;
	roleName: string;
	permissions?: Record<string, Permission>;
}
