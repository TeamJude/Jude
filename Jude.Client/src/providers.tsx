import { HeroUIProvider, ToastProvider } from "@heroui/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "@tanstack/react-router";

const queryClient = new QueryClient();


export default function Providers({ children }: { children: ReactNode }) {
	return <QueryClientProvider client={queryClient}>
		<HeroUIProvider>
			<ToastProvider/>
			{children}
		</HeroUIProvider>
	</QueryClientProvider>
}
