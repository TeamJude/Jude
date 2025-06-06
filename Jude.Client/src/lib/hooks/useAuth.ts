import { useStore } from "@tanstack/react-store";
import authState from "../state/auth.state";
import { Permission, type User } from "../types/user";

type PermissionSet = Record<string, Permission>;

export function useAuth() {
    const state = useStore(authState);

    const hasPermission = (feature: string, permission: Permission): boolean => {
        if (!state.isAuthenticated || !state.user) return false;

        return state.user.role.permissions.some((permissionSet: PermissionSet) => {
            const featurePermission = permissionSet[feature];
            if (!featurePermission) return false;

            // Write permission implies Read permission
            if (permission === Permission.Read && featurePermission === Permission.Write) return true;
            
            return featurePermission === permission;
        });
    };

    const hasAnyPermission = (feature: string): boolean => {
        if (!state.isAuthenticated || !state.user) return false;

        return state.user.role.permissions.some((permissionSet: PermissionSet) => 
            feature in permissionSet
        );
    };

    const getFeaturePermissions = (feature: string): Permission[] => {
        if (!state.isAuthenticated || !state.user) return [];

        const permissions: Permission[] = [];
        state.user.role.permissions.forEach((permissionSet: PermissionSet) => {
            const permission = permissionSet[feature];
            if (permission) {
                permissions.push(permission);
            }
        });

        return [...new Set(permissions)];
    };

    return {
        user: state.user,
        isAuthenticated: state.isAuthenticated,
        isLoading: state.isLoading,
        hasPermission,
        hasAnyPermission,
        getFeaturePermissions,
        userRole: state.isAuthenticated ? state.user.role.name : null,
        isAdmin: state.isAuthenticated && state.user.role.name === "Admin"
    };
} 