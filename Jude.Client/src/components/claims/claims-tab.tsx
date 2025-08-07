import { getClaim, getUserReviewForClaim } from "@/lib/services/claims.service";
import { authState } from "@/lib/state/auth.state";
import type { GetClaimDetailResponse } from "@/lib/types/claim";
import {
	Spinner,
	Tab,
	Tabs,
} from "@heroui/react";
import {
	AlertCircle,
	BookOpen,
	CheckSquare,
	ClipboardList,
	History,
} from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import React from "react";
import { ClaimSummaryTab } from "./claim-summary-tab";
import { ClaimAdjudicationTab } from "./claim-adjudication-tab";
import { ClaimAuditTab } from "./claim-audit-tab";

interface ClaimTabsProps {
	claimId: string;
}

export const ClaimTabs: React.FC<ClaimTabsProps> = ({ claimId }) => {
	const { user } = authState.state;
	const [selected, setSelected] = React.useState("summary");

	const {
		data: claimResponse,
		isLoading,
		error,
	} = useQuery({
		queryKey: ["claim", claimId],
		queryFn: () => getClaim(claimId),
	});

	const {
		data: userReviewResponse,
		isLoading: isLoadingUserReview,
	} = useQuery({
		queryKey: ["userReview", claimId, user?.id],
		queryFn: () => getUserReviewForClaim(claimId),
		enabled: !!user?.id,
	});

	const handleSelectionChange = (key: React.Key) => {
		setSelected(key.toString());
	};

	if (isLoading) {
		return (
			<div className="flex items-center justify-center p-8">
				<Spinner label="Loading claim details..." />
			</div>
		);
	}

	if (error || !claimResponse?.success) {
		return (
			<div className="flex flex-col items-center justify-center p-8">
				<AlertCircle className="w-8 h-8 text-danger mb-2" />
				<p className="text-danger">Failed to load claim details</p>
			</div>
		);
	}

	const claim = claimResponse.data;
	const previousReviews: any[] = []; // TODO: Get reviews from separate API call

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
					key="summary"
					title={
						<div className="flex items-center gap-2">
							<ClipboardList width={18} />
							<span>Claim Summary & Agent Output</span>
						</div>
					}
				>
					<ClaimSummaryTab claim={claim} />
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
					<ClaimAdjudicationTab claim={claim} previousReviews={previousReviews} />
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
