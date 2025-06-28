import React, { useState } from "react";
import { ClaimsTable } from "@/components/claims/claims-table";
import { KanbanBoard } from "@/components/claims/kanban-board";
import { ClaimModal } from "@/components/claims/claim-modal";
import { kanbanClaims, getClaimsStats } from "@/components/claims/claims-data";
import { createFileRoute } from "@tanstack/react-router";
import { 
	Button, 
	ButtonGroup, 
	Select, 
	SelectItem, 
	Card,
	Chip
} from "@heroui/react";
import { Table, Kanban, Calendar, TrendingUp, AlertTriangle, Flag } from "lucide-react";

export const Route = createFileRoute("/__app/claims/")({
	component: RouteComponent,
});

type ViewMode = 'table' | 'kanban';
type TimeFilter = 'today' | 'yesterday' | 'week' | 'month' | 'custom';

function RouteComponent() {
	const [viewMode, setViewMode] = useState<ViewMode>('table');
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

	const renderViewToggle = () => (
		<ButtonGroup>
			<Button
				variant={viewMode === 'table' ? 'solid' : 'bordered'}
				color={viewMode === 'table' ? 'primary' : 'default'}
				startContent={<Table className="w-4 h-4" />}
				onPress={() => setViewMode('table')}
			>
				Table
			</Button>
			<Button
				variant={viewMode === 'kanban' ? 'solid' : 'bordered'}
				color={viewMode === 'kanban' ? 'primary' : 'default'}
				startContent={<Kanban className="w-4 h-4" />}
				onPress={() => setViewMode('kanban')}
			>
				Kanban
			</Button>
		</ButtonGroup>
	);

	const renderStatsCards = () => (
		<div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
			<Card className="p-4">
				<div className="flex items-center gap-2">
					<TrendingUp className="w-5 h-5 text-blue-500" />
					<div>
						<p className="text-sm text-gray-500">Total Claims</p>
						<p className="text-xl font-semibold">{stats.total}</p>
					</div>
				</div>
			</Card>
			<Card className="p-4">
				<div className="flex items-center gap-2">
					<Calendar className="w-5 h-5 text-green-500" />
					<div>
						<p className="text-sm text-gray-500">Total Value</p>
						<p className="text-xl font-semibold">${stats.totalAmount.toLocaleString()}</p>
					</div>
				</div>
			</Card>
			<Card className="p-4">
				<div className="flex items-center gap-2">
					<AlertTriangle className="w-5 h-5 text-orange-500" />
					<div>
						<p className="text-sm text-gray-500">High Risk</p>
						<p className="text-xl font-semibold">{stats.highRiskCount}</p>
					</div>
				</div>
			</Card>
			<Card className="p-4">
				<div className="flex items-center gap-2">
					<Flag className="w-5 h-5 text-red-500" />
					<div>
						<p className="text-sm text-gray-500">Flagged</p>
						<p className="text-xl font-semibold">{stats.flaggedCount}</p>
					</div>
				</div>
			</Card>
		</div>
	);

	const renderKanbanStats = () => (
		<div className="flex gap-4 mb-6">
			<Chip variant="flat" color="primary">
				In Queue: {stats.inQueue}
			</Chip>
			<Chip variant="flat" color="secondary">
				In Progress: {stats.inProgress}
			</Chip>
			<Chip variant="flat" color="warning">
				Awaiting Review: {stats.awaitingReview}
			</Chip>
		</div>
	);

	return (
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				{/* Header with Title and Actions */}
				<div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
					<div>
						<h1 className="text-2xl font-semibold text-gray-800">Claims</h1>
						<p className="text-sm text-gray-500">
							{viewMode === 'kanban' 
								? 'Real-time kanban view of claims processing pipeline' 
								: 'Real-time overview of claims processing'
							}
						</p>
					</div>

					<div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
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
								<SelectItem key={option.key} value={option.key}>
									{option.label}
								</SelectItem>
							))}
						</Select>

						{/* View Toggle */}
						{renderViewToggle()}
					</div>
				</div>

				{/* Statistics Cards */}
				{renderStatsCards()}

				{/* Kanban Lane Statistics - Only show for kanban view */}
				{viewMode === 'kanban' && renderKanbanStats()}

				{/* Main Content */}
				<div className="w-full border-zinc-200 border-1 rounded-lg p-6">
					{viewMode === 'table' ? (
						<ClaimsTable />
					) : (
						<KanbanBoard 
							claims={kanbanClaims} 
							onClaimClick={handleClaimClick}
						/>
					)}
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

