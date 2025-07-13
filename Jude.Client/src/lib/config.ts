export const config = {
	url: import.meta.env.VITE_SERVER_URL as string,
	environment: import.meta.env.MODE as "development" | "production",
} as const;
