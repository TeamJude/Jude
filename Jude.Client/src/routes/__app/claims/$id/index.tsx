import { ClaimDetailHeader } from "@/components/claims/claim-detail-header";
import { ClaimTabs } from "@/components/claims/claims-tab";
import { createFileRoute } from "@tanstack/react-router";
import { Card, CardBody } from "@heroui/react";

export const Route = createFileRoute("/__app/claims/$id/")({
	component: RouteComponent,
	validateSearch: (search: Record<string, unknown>) => {
		return search;
	},
	loader: async ({ params }) => {
		// Real claim data from CIMAS
		return {
			claimId: params.id,
			transactionNumber: "TN-20250310141735-3791",
			claimNumber: "test-claim-03",
			memberName: "FLOWER PASSION FRUIT CRIB",
			memberId: "11067105",
			medicalScheme: "HEALTHGUARD",
			providerName: "Demo Practice",
			providerId: "PRV-DEMO",
			dateReceived: "2025-03-10",
			dateOfService: "2025-03-10",
			dateSubmitted: "2025-03-10T14:17:35",
			amount: 1.51,
			currency: "USD",
			status: "Awaiting Review",
			responseCode: "HELD_FOR_REVIEW",
			gender: "F",
			dateOfBirth: "1974-12-01",
			products: [
				{
					number: "1",
					code: "603790",
					description: "GAUZE BANDAGE SELVEDGED 15CMX4.5M*20S",
					amount: 0.81
				},
				{
					number: "2", 
					code: "609150",
					description: "PARAFFIN GAUZE 10CM X 10CM 1S MC4A",
					amount: 0.70
				}
			]
		};
	},
});

function RouteComponent() {
	const claimData = Route.useLoaderData();

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
							AI-powered claims adjudication system
						</p>
					</div>
				</div>
				<div className="space-y-6">
					<ClaimDetailHeader
						claimId={claimData.claimId}
						memberName={claimData.memberName}
						memberId={claimData.memberId}
						providerName={claimData.providerName}
						providerId={claimData.providerId}
						dateReceived={claimData.dateReceived}
						dateOfService={claimData.dateOfService}
						amount={claimData.amount}
						status={claimData.status}
					/>
					<div className="mt-6 border border-zinc-200 rounded-lg p-4 bg-white">
						<ClaimTabs claimData={claimData} />
					</div>
				</div>
			</div>
		</main>
	);
}
