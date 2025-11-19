import { getClaimsDashboard } from "@/lib/services/claims.service";
import { useQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { DynamicIcon } from "lucide-react/dynamic";
import { useState } from "react";

// Import existing components
import { ClaimsChart } from "@/components/dashboard/claims-chart";
import { MetricsCard } from "@/components/dashboard/metric-card";
import { ClaimsDashboardPeriod } from "@/lib/types/claim";
import {
	Button,
	Card,
	CardBody,
	CardHeader,
	Select,
	SelectItem,
	Spinner,
} from "@heroui/react";
import { AlertCircle } from "lucide-react";

export const Route = createFileRoute("/__app/dashboard/")({
	component: Dashboard,
});

function Dashboard() {
	const [period, setPeriod] = useState<ClaimsDashboardPeriod>(
		ClaimsDashboardPeriod.Last7Days,
	);

	const periodTextMap: Record<ClaimsDashboardPeriod, string> = {
		[ClaimsDashboardPeriod.Last24Hours]: "from the last 24 hours",
		[ClaimsDashboardPeriod.Last7Days]: "from the last 7 days",
		[ClaimsDashboardPeriod.Last30Days]: "from the last 30 days",
		[ClaimsDashboardPeriod.LastQuarter]: "from the last quarter",
	};
	const { data, isLoading, error, refetch } = useQuery({
		queryKey: ["claimsDashboard", period],
		queryFn: () => getClaimsDashboard(period),
	});

	if (isLoading) {
		return (
			<div className="flex items-center justify-center h-full">
				<Spinner label="Loading dashboard..." />
			</div>
		);
	}

	if (error || !data?.success) {
		return (
			<div className="flex flex-col items-center h-full justify-center p-8">
				<AlertCircle className="w-8 h-8 text-danger mb-2" />
				<p className="text-danger">Failed to load dashboard</p>
				<Button
					color="primary"
					variant="flat"
					onPress={() => refetch()}
					className="mt-4"
				>
					Retry
				</Button>
			</div>
		);
	}

	return (
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				<div className="flex justify-between items-center">
					<div>
						<h1 className="text-2xl font-semibold text-gray-800">
							Claims Dashboard
						</h1>
						<p className="text-sm text-gray-500">
							Real-time overview of claims processing
						</p>
					</div>
					<div className="flex items-center gap-3">
						<Select
							aria-label="Date Range"
							className="w-36"
							selectionMode="single"
							selectedKeys={[period.toString()]}
							onSelectionChange={(keys) =>
								setPeriod(Array.from(keys)[0] as ClaimsDashboardPeriod)
							}
						>
							<SelectItem key={ClaimsDashboardPeriod.Last24Hours}>
								Last 24 hours
							</SelectItem>
							<SelectItem key={ClaimsDashboardPeriod.Last7Days}>
								Last 7 Days
							</SelectItem>
							<SelectItem key={ClaimsDashboardPeriod.Last30Days}>
								Last 30 Days
							</SelectItem>
							<SelectItem key={ClaimsDashboardPeriod.LastQuarter}>
								Last Quarter
							</SelectItem>
						</Select>
						<Button
							color="primary"
							startContent={<DynamicIcon name="refresh-cw" size={16} />}
							onPress={() => refetch()}
						>
							Refresh
						</Button>
					</div>
				</div>

				<div className="grid gap-6">
					<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
						<MetricsCard
							title="Total Claims"
							value={data.data?.totalClaims?.toLocaleString()}
							change={data.data?.totalClaimsChangePercent ?? 0}
							icon="file-text"
							color="primary"
							helpText={`Total number of claims received ${periodTextMap[period]}`}
							period={periodTextMap[period]}
						/>
						<MetricsCard
							title="Auto-Approved Rate"
							value={`${data.data?.autoApprovedRate?.toFixed(0) ?? "-"}%`}
							change={data.data?.autoApprovedRateChangePercent ?? 0}
							icon="check-circle"
							color="success"
							helpText={`Percentage of claims automatically approved by the AI system ${periodTextMap[period]}`}
							period={periodTextMap[period]}
						/>
						<MetricsCard
							title="Pending Review"
							value={data.data?.claimsPendingReview?.toLocaleString()}
							change={data.data?.claimsPendingReviewChangePercent ?? 0}
							icon="clipboard-list"
							color="warning"
							helpText={`Claims awaiting human review ${periodTextMap[period]}`}
							period={periodTextMap[period]}
						/>
						<MetricsCard
							title="Claims Flagged"
							value={data.data?.claimsFlagged?.toLocaleString()}
							change={data.data?.claimsFlaggedChangePercent ?? 0}
							icon="alert-triangle"
							color="warning"
							helpText={`Claims flagged for potential fraud or policy violations ${periodTextMap[period]}`}
							period={periodTextMap[period]}
						/>
					</div>

					<div className="w-full">
						<Card className="lg:col-span-2 shadow-none border-zinc-200 border-1">
							<CardHeader className="flex justify-between items-center pb-2">
								<div>
									<h3 className="text-base font-medium">Claims Activity</h3>
									<p className="text-xs text-gray-500">
										Daily claim volume and processing status{" "}
										{periodTextMap[period]}
									</p>
								</div>
							</CardHeader>
							<CardBody>
								<ClaimsChart
									title=""
									data={data.data?.claimsActivity ?? []}
									isLoading={isLoading}
								/>
							</CardBody>
						</Card>
					</div>
				</div>
			</div>
		</main>
	);
}
