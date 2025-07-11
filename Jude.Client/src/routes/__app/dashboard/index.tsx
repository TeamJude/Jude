import { createFileRoute } from "@tanstack/react-router";
import {
	Card,
	CardBody,
	CardHeader,
	Button,
	Tabs,
	Tab,
	Dropdown,
	DropdownTrigger,
	DropdownMenu,
	DropdownItem,
	Select,
	SelectItem,
} from "@heroui/react";
import { DynamicIcon } from "lucide-react/dynamic";

// Import existing components
import { ClaimsChart } from "@/components/dashboard/claims-chart";
import { MetricsCard } from "@/components/dashboard/metric-card";
import { CircleChartCard } from "@/components/dashboard/circle-chart-card";
import { RecentClaimsTable } from "@/components/dashboard/recent-claims-table";

export const Route = createFileRoute("/__app/dashboard/")({
	component: Dashboard,
});

function Dashboard() {
	const categoryChartData = [
		{
			title: "Claim Categories",
			categories: ["Outpatient", "Inpatient", "Pharmacy", "Dental"],
			color: "primary",
			chartData: [
				{ name: "Outpatient", value: 420 },
				{ name: "Inpatient", value: 280 },
				{ name: "Pharmacy", value: 190 },
				{ name: "Dental", value: 110 },
			],
		},
		{
			title: "Claim Sources",
			categories: ["Providers", "Members", "Partners", "Other"],
			color: "warning",
			chartData: [
				{ name: "Providers", value: 580 },
				{ name: "Members", value: 240 },
				{ name: "Partners", value: 130 },
				{ name: "Other", value: 50 },
			],
		},
		{
			title: "Adjudication Types",
			categories: ["Auto-Approved", "Manual Review", "Flagged", "Rejected"],
			color: "danger",
			chartData: [
				{ name: "Auto-Approved", value: 650 },
				{ name: "Manual Review", value: 200 },
				{ name: "Flagged", value: 100 },
				{ name: "Rejected", value: 50 },
			],
		},
	];

	return (	
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				{/* Header with Title and Actions */}
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
							size="sm"
							defaultSelectedKeys={["week"]}
							className="w-40"
						>
							<SelectItem key="today">Today</SelectItem>
							<SelectItem key="week">Last 7 Days</SelectItem>
							<SelectItem key="month">Last 30 Days</SelectItem>
							<SelectItem key="quarter">Last Quarter</SelectItem>
						</Select>
						<Button
							color="primary"
							startContent={<DynamicIcon name="refresh-cw" size={16} />}
						>
							Refresh
						</Button>
						<Dropdown>
							<DropdownTrigger>
								<Button
									variant="flat"
									startContent={<DynamicIcon name="download" size={16} />}
								>
									Export
								</Button>
							</DropdownTrigger>
							<DropdownMenu aria-label="Export Options">
								<DropdownItem key="pdf">PDF Report</DropdownItem>
								<DropdownItem key="excel">Excel Spreadsheet</DropdownItem>
								<DropdownItem key="csv">CSV Data</DropdownItem>
							</DropdownMenu>
						</Dropdown>
					</div>
				</div>

				{/* Summary Metrics Cards */}
				<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
					<MetricsCard
						title="Total Claims"
						value="1,525"
						change={8}
						icon="file-text"
						color="primary"
						helpText="Total number of claims received in the selected period"
					/>
					<MetricsCard
						title="Auto-Approved Rate"
						value="65%"
						change={3}
						icon="check-circle"
						color="success"
						helpText="Percentage of claims automatically approved by the AI system"
					/>
					<MetricsCard
						title="Avg. Processing Time"
						value="2.4"
						subtitle="minutes"
						change={-12}
						icon="clock"
						color="secondary"
						helpText="Average time from claim submission to final decision"
					/>
					<MetricsCard
						title="Claims Flagged"
						value="47"
						change={5}
						icon="alert-triangle"
						color="warning"
						helpText="Claims flagged for potential fraud or policy violations"
					/>
				</div>

				{/* Main Charts Section */}
				<div className="w-full">
					{/* Weekly Activity Chart - Spans 2 columns */}
					<Card className="lg:col-span-2 shadow-none   border-zinc-200 border-1">
						<CardHeader className="flex justify-between items-center pb-2">
							<div>
								<h3 className="text-base font-medium">Claims Activity</h3>
								<p className="text-xs text-gray-500">
									Daily claim volume and processing status
								</p>
							</div>
							<Tabs size="sm" aria-label="Time periods">
								<Tab key="weekly" title="Weekly" />
								<Tab key="monthly" title="Monthly" />
								<Tab key="quarterly" title="Quarterly" />
							</Tabs>
						</CardHeader>
						<CardBody>
							<ClaimsChart title="" />
						</CardBody>
					</Card>
				</div>

				{/* Recent Claims Table */}
				<Card className="shadow-none p-2 border-zinc-200 border-1">
					<CardHeader className="flex justify-between items-center pb-2">
						<div>
							<h3 className="text-base font-medium">Recent Claims</h3>
							<p className="text-xs text-gray-500">
								Latest claims requiring attention
							</p>
						</div>
					</CardHeader>
					<CardBody>
						<RecentClaimsTable />
					</CardBody>
				</Card>

			
			</div>
		</main>
	);
}
