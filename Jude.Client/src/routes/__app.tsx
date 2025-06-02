import AppSidebar from "@/components/layout/app-sidebar";
import { createFileRoute, Outlet } from "@tanstack/react-router";

export const Route = createFileRoute("/__app")({
	component: RouteComponent,
});

function RouteComponent() {
	return (
		<main className="h-screen w-full overflow-hidden flex flex-row bg-zinc-50">
			<AppSidebar />
			<div className="w-full overflow-y-auto rounded-xl shadow-sm bg-[#F8F9FA] border border-zinc-200 m-1">
				<Outlet />
			</div>
		</main>
	);
}
