import { ClaimDetailHeader } from "@/components/claims/claim-detail-header";
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

	// Fetch claim details
	const {
		data: claimResponse,
		isLoading,
		error,
	} = useQuery({
		queryKey: ["claim", claimId],
		queryFn: () => getClaim(claimId),
	});

	if (isLoading) {
		return (
			<main className="min-h-screen py-6 px-4">
				<div className="max-w-7xl mx-auto px-4 space-y-6">
					<div className="flex items-center justify-center p-8">
						<Spinner label="Loading claim details..." />
					</div>
				</div>
			</main>
		);
	}

	if (error || !claimResponse?.success) {
		return (
			<main className="min-h-screen py-6 px-4">
				<div className="max-w-7xl mx-auto px-4 space-y-6">
					<div className="flex flex-col items-center justify-center p-8">
						<AlertCircle className="w-8 h-8 text-danger mb-2" />
						<p className="text-danger">Failed to load claim details</p>
					</div>
				</div>
			</main>
		);
	}
	const claim = claimResponse.data;

	return (
		<main className="min-h-screen py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				{/* Header with Title and Actions */}
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
					<ClaimDetailHeader
						claimId={claim.id}
						memberName={`${claim.patientFirstName} ${claim.patientSurname}`}
						memberId={claim.data?.membershipNumber || "N/A"}
						providerName={claim.data?.providerPractice || "N/A"}
						providerId={claim.data?.providerId || "N/A"}
						dateReceived={claim.ingestedAt}
						dateOfService={claim.data?.dateOfService || claim.ingestedAt}
						amount={claim.totalClaimAmount}
						status={claim.status}
					/>

					<div className="mt-6 border border-zinc-200 rounded-lg p-4 bg-white">
						<ClaimTabs claimId={claim.id} />
					</div>
				</div>
			</div>
		</main>
	);
}
