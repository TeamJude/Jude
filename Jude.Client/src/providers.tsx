import { HeroUIProvider } from "@heroui/react";
import type { ReactNode } from "@tanstack/react-router";

export default function Providers({ children }: { children: ReactNode }) {
	return <HeroUIProvider>{children}</HeroUIProvider>;
}
