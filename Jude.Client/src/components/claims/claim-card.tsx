import { Card, Chip, Button } from "@heroui/react";
import {
	AlertTriangle,
	DollarSign,
	Eye
} from "lucide-react";
import React, { useState } from "react";
import { ClaimModal } from "./claim-modal";

interface ClaimCardProps {
	claim: {
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
	};
	onClick?: () => void;
	onReview?: (claimId: string) => void;
}

export const ClaimCard: React.FC<ClaimCardProps> = ({ claim, onClick, onReview }) => {
	const [showModal, setShowModal] = useState(false);

	const getRiskColor = () => {
		switch (claim.fraudRiskLevel) {
			case "CRITICAL":
				return "danger";
			case "HIGH":
				return "danger";
			case "MEDIUM":
				return "warning";
			case "LOW":
				return "success";
			default:
				return "default";
		}
	};

	const handleViewClick = (e: React.MouseEvent) => {
		e.stopPropagation();
		setShowModal(true);
	};

	const handleCloseModal = () => {
		setShowModal(false);
	};

	const handleCardClick = () => {
		if (onClick) {
			onClick();
		}
	};

	return (
		<>
			<Card
				className="relative p-4 shadow-sm border-zinc-200 border-1 cursor-pointer transition-all duration-200 hover:shadow-md bg-white  hover:border-gray-300"
				onClick={handleCardClick}
			>
				{/* Eye Icon Button */}
				<Button
					isIconOnly
					size="sm"
					variant="light"
					className="absolute top-2 right-2 text-gray-400 hover:text-gray-600 z-10"
					onPress={()=>{setShowModal(true)}}
				>
					<Eye className="w-4 h-4" />
				</Button>

				<div className="flex justify-between items-start mb-3 pr-8">
					<div className="flex-1">
						<h3 className="font-medium text-gray-900 mb-1">
							{claim.patientName}
						</h3>
						<p className="text-sm text-gray-500">{claim.providerPractice}</p>
					</div>
					{claim.isFlagged && (
						<AlertTriangle className="w-4 h-4 text-red-500 flex-shrink-0" />
					)}
				</div>

				<div className="flex items-center justify-between mb-3">
					<div className="flex items-center text-gray-700">
						<DollarSign className="w-4 h-4 mr-1" />
						<span className="font-medium text-sm">
							{claim.currency}
							{claim.claimAmount.toLocaleString()}
						</span>
					</div>
					<span className="text-xs text-gray-500 font-mono">
						{claim.transactionNumber}
					</span>
				</div>

				<div className="flex justify-between items-center">
					<Chip color={getRiskColor()} variant="flat" size="sm">
						{claim.fraudRiskLevel}
					</Chip>
					<span className="text-xs text-gray-500">{claim.timeAgo}</span>
				</div>
			</Card>

			{/* Modal */}
			{showModal && (
				<ClaimModal 
					claim={claim} 
					onClose={handleCloseModal}
					onReview={onReview}
				/>
			)}
		</>
	);
};
