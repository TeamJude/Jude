import AppSidebar from "@/components/layout/app-sidebar";
import { protectedLoader } from "@/lib/utils/loaders";
import { createFileRoute, Outlet } from "@tanstack/react-router";

export const Route = createFileRoute("/__app")({
	component: RouteComponent,
	loader: protectedLoader,
});

function RouteComponent() {
	return (
		<main className="h-screen bg-[#F3F7ED] w-full overflow-hidden flex flex-row">
			<AppSidebar />
			<div className="w-full overflow-y-auto rounded-xl shadow-sm border bg-white border-zinc-200 m-1">
				<Outlet />
			</div>
		</main>
	);
}
	