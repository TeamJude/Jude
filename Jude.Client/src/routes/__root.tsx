import { config } from "@/lib/config";
import { me } from "@/lib/services/auth.service";
import { authState } from "@/lib/state/auth.state";
import { Outlet, createRootRoute } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

export const Route = createRootRoute({
	component: RootComponent,
	loader: async () => {
		const { isLoading } = authState.state;

		if (isLoading) {
			try {
				const response = await me();

				if (response.success && response.data) {
					authState.setState(() => ({
						user: response.data,
						isAuthenticated: true,
						isLoading: false,
					}));
				} else {
					authState.setState(() => ({
						user: null,
						isAuthenticated: false,
						isLoading: false,
					}));
				}
			} catch (error) {
				console.error("Not authenticated:", error);
				authState.setState(() => ({
					user: null,
					isAuthenticated: false,
					isLoading: false,
				}));
			}
		}
		return null;
	},
});

function RootComponent() {
	return (
		<main>
			<Outlet />
			{config.environment == "development" && <TanStackRouterDevtools />}
		</main>
	);
}
