import { ClaimTabs } from "@/components/claims/claims-tab";
import { getClaim } from "@/lib/services/claims.service";
import { Spinner } from "@heroui/react";
import { useQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { AlertCircle } from "lucide-react";

export const Route = createFileRoute("/__app/claims/$id/")({
	component: RouteComponent,
	validateSearch: (search: Record<string, unknown>) => {
		return search;
	},
});

function RouteComponent() {
	const { id: claimId } = Route.useParams();

	const {
		data: claimResponse,
		isLoading,
		error,
		refetch,
	} = useQuery({
		queryKey: ["claim", claimId],
		queryFn: () => getClaim(claimId),
	});

	const claim = claimResponse?.success ? claimResponse.data : null;

	return (
		<main className="min-h-screen py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				<div className="flex justify-between items-center">
					<div>
						<h1 className="text-2xl font-semibold text-gray-800">
							Claim Details
						</h1>
						<p className="text-sm text-gray-500">
							Real-time overview of claims processing
						</p>
					</div>
				</div>

				<div className="space-y-6">
					<div className="mt-6 border border-zinc-200 rounded-lg p-4 bg-white min-h-[600px]">
						{isLoading ? (
							<div className="flex items-center justify-center h-full min-h-[600px]">
								<Spinner label="Loading claim details..." size="lg" />
							</div>
						) : error || !claim ? (
							<div className="flex flex-col items-center justify-center h-full min-h-[600px]">
								<AlertCircle className="w-8 h-8 text-danger mb-2" />
								<p className="text-danger mb-4">Failed to load claim details</p>
								<button
									onClick={() => refetch()}
									className="px-4 py-2 text-sm bg-primary text-white rounded-md hover:bg-primary/90 transition-colors"
								>
									Retry
								</button>
							</div>
						) : (
							<ClaimTabs claim={claim} />
						)}
					</div>
				</div>
			</div>
		</main>
	);
}
