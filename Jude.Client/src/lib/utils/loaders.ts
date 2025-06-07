import { redirect } from "@tanstack/react-router";
import { authState } from "@/lib/state/auth.state";

const AUTH_CHECK_TIMEOUT = 10000; // 5 seconds timeout

async function waitForAuthState(): Promise<void> {
	return new Promise((resolve, reject) => {
		const timeout = setTimeout(() => {
			reject(new Error("Auth state check timed out"));
		}, AUTH_CHECK_TIMEOUT);

		// Check immediately first
		if (!authState.state.isLoading) {
			clearTimeout(timeout);
			resolve();
			return;
		}

		const unsubscribe = authState.subscribe((state) => {
			if (!state.currentVal.isLoading) {
				clearTimeout(timeout);
				unsubscribe();
				resolve();
			}
		});

		return () => {
			clearTimeout(timeout);
			unsubscribe();
		};
	});
}

/**
 * A loader function that checks if the user is authenticated
 * and redirects to the dashboard if they are
 */
export const publicOnlyLoader = async () => {
	try {
		await waitForAuthState();

		const { isAuthenticated } = authState.state;
		if (isAuthenticated) {
			throw redirect({
				to: "/dashboard",
			});
		}

		return null;
	} catch (error) {
		// If timeout or other error, allow access to public route
		return null;
	}
};

/**
 * A loader function that checks if the user is authenticated
 * and redirects to the sign-in page if not
 */
export const protectedLoader = async () => {
	try {
		await waitForAuthState();

		const { isAuthenticated } = authState.state;
		if (!isAuthenticated) {
			throw redirect({
				to: "/auth/login",
			});
		}

		return null;
	} catch (error) {
		// If timeout or other error, redirect to sign-in as a fallback
		throw redirect({
			to: "/auth/login",
		});
	}
};
