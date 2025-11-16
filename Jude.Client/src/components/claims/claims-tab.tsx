import { getUserReviewForClaim } from "@/lib/services/claims.service";
import { authState } from "@/lib/state/auth.state";
import type { GetClaimDetailResponse } from "@/lib/types/claim";
import { Tab, Tabs } from "@heroui/react";
import { BookOpen, CheckSquare, ClipboardList, History } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import React from "react";
import { AgentReviewTab } from "./agent-review-tab";
import { ClaimAdjudicationTab } from "./claim-adjudication-tab";
import { ClaimAuditTab } from "./claim-audit-tab";
import { ClaimOverviewTab } from "./claim-overview-tab";

interface ClaimTabsProps {
	claim: GetClaimDetailResponse;
}

export const ClaimTabs: React.FC<ClaimTabsProps> = ({ claim }) => {
	const { user } = authState.state;
	const [selected, setSelected] = React.useState("overview");

	const { data: userReviewResponse, isLoading: isLoadingUserReview } = useQuery(
		{
			queryKey: ["userReview", claim.id, user?.id],
			queryFn: () => getUserReviewForClaim(claim.id),
			enabled: !!user?.id,
		},
	);

	const handleSelectionChange = (key: React.Key) => {
		setSelected(key.toString());
	};

	return (
		<div className="flex w-full flex-col">
			<Tabs
				aria-label="Claim details tabs"
				selectedKey={selected}
				onSelectionChange={handleSelectionChange}
				color="primary"
				variant="underlined"
				classNames={{
					tabList: "gap-6",
					cursor: "w-full bg-primary",
					tab: "max-w-fit px-0 h-12",
				}}
			>
				<Tab
					key="overview"
					title={
						<div className="flex items-center gap-2">
							<BookOpen width={18} />
							<span>Claim Overview</span>
						</div>
					}
				>
					<ClaimOverviewTab claim={claim} />
				</Tab>
				<Tab
					key="agent-review"
					title={
						<div className="flex items-center gap-2">
							<ClipboardList width={18} />
							<span>Agent Review</span>
						</div>
					}
				>
					<AgentReviewTab claim={claim} />
				</Tab>

				<Tab
					key="adjudication"
					title={
						<div className="flex items-center gap-2">
							<CheckSquare width={18} />
							<span>Human Review & Adjudication</span>
						</div>
					}
				>
					<ClaimAdjudicationTab claim={claim} />
				</Tab>

				<Tab
					key="audit"
					title={
						<div className="flex items-center gap-2">
							<History width={18} />
							<span>Audit Trail</span>
						</div>
					}
				>
					<ClaimAuditTab claim={claim} />
				</Tab>
			</Tabs>
		</div>
	);
};
