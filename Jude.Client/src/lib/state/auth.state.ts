import type { User } from "@lib/types/user";
import { Store } from "@tanstack/react-store";

type AuthState = {
    user: User;
    isAuthenticated: true;
    isLoading: boolean;
} | {
    user: null;
    isAuthenticated: false;
    isLoading: boolean;
};

const authState = new Store<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
});

export default authState;

