import React, { useState } from "react";
import { ClaimsTable } from "@/components/claims/claims-table";
import { ClaimModal } from "@/components/claims/claim-modal";
import { getClaimsStats } from "@/components/claims/claims-data";
import { createFileRoute } from "@tanstack/react-router";
import { 
	Select, 
	SelectItem, 
	Card
} from "@heroui/react";
import { Calendar, TrendingUp, AlertTriangle, Flag } from "lucide-react";

export const Route = createFileRoute("/__app/claims/")({
	component: RouteComponent,
});

type TimeFilter = 'today' | 'yesterday' | 'week' | 'month' | 'custom';

function RouteComponent() {
	const [timeFilter, setTimeFilter] = useState<TimeFilter>('today');
	const [selectedClaim, setSelectedClaim] = useState<any>(null);

	const stats = getClaimsStats();

	const timeFilterOptions = [
		{ key: 'today', label: 'Today' },
		{ key: 'yesterday', label: 'Yesterday' },
		{ key: 'week', label: 'Last 7 days' },
		{ key: 'month', label: 'Last 30 days' },
		{ key: 'custom', label: 'Custom range' },
	];

	const handleClaimClick = (claim: any) => {
		setSelectedClaim(claim);
	};

	const handleCloseModal = () => {
		setSelectedClaim(null);
	};

	const renderStatsCards = () => (
		<div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
			<Card shadow="none" className="p-4 border border-gray-200">
				<div className="flex items-center gap-3">
					<TrendingUp className="w-5 h-5 text-gray-600" />
					<div>
						<p className="text-sm text-gray-500">Total Claims</p>
						<p className="text-xl font-semibold text-gray-900">{stats.total}</p>
					</div>
				</div>
			</Card>
			<Card shadow="none" className="p-4 border border-gray-200">
				<div className="flex items-center gap-3">
					<Calendar className="w-5 h-5 text-gray-600" />
					<div>
						<p className="text-sm text-gray-500">Total Value</p>
						<p className="text-xl font-semibold text-gray-900">${stats.totalAmount.toLocaleString()}</p>
					</div>
				</div>
			</Card>
			<Card shadow="none" className="p-4 border border-gray-200">
				<div className="flex items-center gap-3">
					<AlertTriangle className="w-5 h-5 text-gray-600" />
					<div>
						<p className="text-sm text-gray-500">High Risk</p>
						<p className="text-xl font-semibold text-gray-900">{stats.highRiskCount}</p>
					</div>
				</div>
			</Card>
			<Card shadow="none" className="p-4 border border-gray-200">
				<div className="flex items-center gap-3">
					<Flag className="w-5 h-5 text-gray-600" />
					<div>
						<p className="text-sm text-gray-500">Flagged</p>
						<p className="text-xl font-semibold text-gray-900">{stats.flaggedCount}</p>
					</div>
				</div>
			</Card>
		</div>
	);

	return (
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				{/* Header with Title and Actions */}
				<div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
					<div>
						<h1 className="text-2xl font-semibold text-gray-900">Claims</h1>
						<p className="text-sm text-gray-500">
							Claims processing and adjudication
						</p>
					</div>

					<div className="flex items-center gap-3">
						{/* Time Filter */}
						<Select
							size="sm"
							placeholder="Select time range"
							selectedKeys={[timeFilter]}
							onSelectionChange={(keys) => setTimeFilter(Array.from(keys)[0] as TimeFilter)}
							className="w-40"
							startContent={<Calendar className="w-4 h-4" />}
						>
							{timeFilterOptions.map((option) => (
								<SelectItem key={option.key}>
									{option.label}
								</SelectItem>
							))}
						</Select>
					</div>
				</div>

				{/* Statistics Cards */}
				{renderStatsCards()}

				{/* Main Content */}
				<div className="w-full border border-gray-200 rounded-lg p-6 bg-white">
					<ClaimsTable />
				</div>

				{/* Claim Modal */}
				{selectedClaim && (
					<ClaimModal 
						claim={selectedClaim} 
						onClose={handleCloseModal}
					/>
				)}
			</div>
		</main>
	);
}

