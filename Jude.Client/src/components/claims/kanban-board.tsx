import React from "react";
import { Card, Chip, Badge, Button } from "@heroui/react";
import { ClaimCard } from "./claim-card";
import { Clock, RefreshCw, AlertTriangle, Plus } from "lucide-react";

interface KanbanBoardProps {
	claims: Array<{
		id: string;
		transactionNumber: string;
		patientName: string;
		membershipNumber: string;
		claimAmount: number;
		currency: string;
		approvedAmount?: number;
		providerPractice: string;
		timeAgo: string;
		itemSummary: string;
		status: "inQueue" | "inProgress" | "awaitingReview";
		fraudRiskLevel: "LOW" | "MEDIUM" | "HIGH" | "CRITICAL";
		isFlagged: boolean;
		source: string;
		ingestedAt: string;
		processingStartedAt?: string;
		lastUpdatedAt: string;
		patientDetails?: {
			dateOfBirth: string;
			gender: string;
			idNumber: string;
		};
		providerDetails?: {
			name: string;
			contactInfo: string;
		};
		items?: Array<{
			type: 'product' | 'service';
			code: string;
			description: string;
			amount: number;
			status: string;
		}>;
		agentProgress?: number;
		agentReasoning?: string[];
	}>;
	onClaimClick: (claim: any) => void;
	onReview?: (claimId: string) => void;
}

export const KanbanBoard: React.FC<KanbanBoardProps> = ({
	claims,
	onClaimClick,
	onReview,
}) => {
	const lanes = [
		{
			title: "In Queue",
			status: "inQueue" as const,
			color: "default" as const,
			description: "Claims waiting to be processed",
			icon: Clock,
		},
		{
			title: "In Progress",
			status: "inProgress" as const,
			color: "secondary" as const,
			description: "Claims currently being processed by AI",
			icon: RefreshCw,
		},
		{
			title: "Awaiting Review",
			status: "awaitingReview" as const,
			color: "warning" as const,
			description: "Claims pending human review",
			icon: AlertTriangle,
		},
	];

	const getClaimsForLane = (status: string) => {
		return claims.filter((claim) => claim.status === status);
	};

	return (
		<div className="space-y-6">
			{/* Kanban Board */}
			<div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
				{lanes.map((lane) => {
					const laneClaims = getClaimsForLane(lane.status);
					const LaneIcon = lane.icon;

					return (
						<div key={lane.status} className="flex flex-col">
							{/* Lane Header */}
							<div className="flex items-center justify-between mb-4 bg-white rounded-lg p-4 border border-gray-200">
								<div className="flex items-center gap-3">
									<div className="flex items-center gap-2">
										<LaneIcon className="w-5 h-5 text-gray-600" />
										<h3 className="font-semibold text-gray-900">{lane.title}</h3>
									</div>
									<Chip
										variant="flat"
										color={lane.color}
										size="sm"
										className="min-w-6 h-6"
									>
										{laneClaims.length}
									</Chip>
								</div>
								<Button
									isIconOnly
									size="sm"
									variant="ghost"
									className="text-gray-400 hover:text-gray-600"
								>
									<Plus className="w-4 h-4" />
								</Button>
							</div>

							{/* Claims Container */}
							<div className="flex-1 min-h-[500px]">
								{laneClaims.length === 0 ? (
									<div className="text-center py-12">
										<LaneIcon className="w-12 h-12 text-gray-300 mx-auto mb-3" />
										<p className="text-gray-500 font-medium mb-1">No claims yet</p>
										<p className="text-sm text-gray-400">
											Claims will appear here as they move through the process
										</p>
									</div>
								) : (
									<div className="space-y-3">
										{laneClaims.map((claim) => (
											<ClaimCard
												key={claim.transactionNumber}
												claim={claim}
												onClick={() => onClaimClick(claim)}
												onReview={onReview}
											/>
										))}
									</div>
								)}
							</div>
						</div>
					);
				})}
			</div>
		</div>
	);
};
