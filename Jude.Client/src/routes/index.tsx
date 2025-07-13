import { authState } from "@/lib/state/auth.state";
import { waitForAuthState } from "@/lib/utils/loaders";
import { Button } from "@heroui/react";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
	component: App,
	loader: async () => {
		try {
			await waitForAuthState();

			const { isAuthenticated } = authState.state;
			if (isAuthenticated) {
				throw redirect({
					to: "/dashboard",
				});
			} else {
				throw redirect({
					to: "/auth/login",
				});
			}
		} catch (error) {
			throw redirect({
				to: "/auth/login",
			});
		}
	},
});

function App() {
	return (
		<div className="text-center font-bold">
			<p className="font-bold text-3xl">test</p>
			<Button variant="solid" color="primary">
				Hello
			</Button>
		</div>
	);
}
