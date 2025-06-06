import { me } from "@/lib/services/auth.service";
import { authState } from "@/lib/state/auth.state";
import { Outlet, createRootRoute } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

export const Route = createRootRoute({
	component: () => (
		<main>
			<Outlet />
			<TanStackRouterDevtools />
		</main>
	),
	loader: async () => {
		const response = await me();
		if (response.success) 
			authState.setState(()=>({
				user: response.data,
				isAuthenticated: true,
				isLoading: false,
			}));
		else authState.setState(()=>({
			user: null,
			isAuthenticated: false,
			isLoading: false,
		}));
		return null;
	},
});
