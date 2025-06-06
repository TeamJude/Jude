import { authState } from "@/lib/state/auth.state";
import { redirect } from "@tanstack/react-router";

export const publicOnlyLoader = async () => {
	if (authState.state.isLoading) 
		await new Promise<void>((res) => {
			const unsub = authState.subscribe((s) => {
				if (!s.currentVal.isLoading) {
					unsub();
					res();
				}
			});
		});
	if (authState.state.isAuthenticated) throw redirect({ to: "/dashboard" });
	return null;
};
