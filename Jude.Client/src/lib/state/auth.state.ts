import { Store } from "@tanstack/react-store";
import type { Permission, User } from "../types/user";

export type AuthState = {
	user: User | null;
	isAuthenticated: boolean;
	isLoading: boolean;
};

export const authState = new Store<AuthState>({
	user: null,
	isAuthenticated: false,
	isLoading: true,
});

export const authActions = {
	setUser: (user: User | null) => {
		authState.setState((state) => ({
			...state,
			user,
			isAuthenticated: !!user,
			isLoading: false,
		}));
	},
	clearUser: () => {
		authState.setState((state) => ({
			...state,
			user: null,
			isAuthenticated: false,
			isLoading: false,
		}));
	},
	setLoading: (isLoading: boolean) => {
		authState.setState((state) => ({
			...state,
			isLoading,
		}));
	},
};

export const authSelectors = {
	hasPermission: (feature: string, permission: Permission) => {
		const state = authState.state;
		if (!state.user) return false;

		const userPermission = state.user.role.permissions[feature];
		if (userPermission === undefined) return false;

		return userPermission >= permission;
	},
	canAccessFeature: (feature: string) => {
		const state = authState.state;
		if (!state.user) return false;

		return feature in state.user.role.permissions;
	},
	getFeaturePermission: (feature: string) => {
		const state = authState.state;
		if (!state.user) return undefined;

		return state.user.role.permissions[feature];
	},
};
