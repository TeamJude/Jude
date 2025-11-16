import { ClaimsTable } from "@/components/claims/claims-table";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/__app/claims/")({
	component: RouteComponent,
});

function RouteComponent() {
	return (
		<main className="h-full py-6 px-4">
			<div className="w-full mx-auto px-4 space-y-6 h-full min-h-0 flex flex-col">
				<div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
					<div>
						<h1 className="text-2xl font-semibold text-gray-900">Claims</h1>
						<p className="text-sm text-gray-500">
							Claims processing and adjudication
						</p>
					</div>
				</div>
				<div className="flex-1 min-h-0">
					<ClaimsTable />
				</div>
			</div>
		</main>
	);
}
