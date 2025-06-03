import type { User } from "@lib/types/user";
import { Store } from "@tanstack/react-store";

const authState = new Store<({
  user: User;
  isAuthenticated: true;
  isLoading: boolean;
} | {
  user:null;
  isAuthenticated: false;
  isLoading: boolean;
})>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
});

export default authState;

