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
    hasPermission: (feature: keyof User["role"]["permissions"], permission: Permission) => {
        const state = authState.state;
        if (!state.user) return false;
        
        const featurePermissions = state.user.role.permissions[feature];
        if (!Array.isArray(featurePermissions)) return false;
        
        return featurePermissions.includes(permission);
    },
    canAccessFeature: (feature: keyof User["role"]["permissions"]) => {
        const state = authState.state;
        if (!state.user) return false;
        
        const featurePermissions = state.user.role.permissions[feature];
        return Array.isArray(featurePermissions) && featurePermissions.length > 0;
    },
    getFeaturePermissions: (feature: keyof User["role"]["permissions"]) => {
        const state = authState.state;
        if (!state.user) return [];
        
        const featurePermissions = state.user.role.permissions[feature];
        return Array.isArray(featurePermissions) ? featurePermissions : [];
    }
};

