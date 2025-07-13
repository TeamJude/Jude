import { Card, Chip } from "@heroui/react";
import {
	AlertTriangle,
	ClipboardList,
	Clock,
	DollarSign,
	Hospital,
} from "lucide-react";
import React from "react";

interface ClaimCardProps {
	claim: {
		id: string;
		transactionNumber: string;
		patientName: string;
		membershipNumber: string;
		claimAmount: number;
		currency: string;
		providerPractice: string;
		timeAgo: string;
		itemSummary: string;
		status: "inQueue" | "inProgress" | "awaitingReview";
		fraudRiskLevel: "LOW" | "MEDIUM" | "HIGH" | "CRITICAL";
		isFlagged: boolean;
		source: string;
	};
	onClick: () => void;
}

export const ClaimCard: React.FC<ClaimCardProps> = ({ claim, onClick }) => {
	const getBorderClass = () => {
		switch (claim.status) {
			case "inQueue":
				return "border-blue-500 hover:border-blue-600";
			case "inProgress":
				return "border-purple-500 hover:border-purple-600";
			case "awaitingReview":
				return "border-orange-500 shadow-lg shadow-orange-200 hover:border-orange-600";
			default:
				return "border-gray-300";
		}
	};

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

	const getProcessingIndicator = () => {
		if (claim.status === "inProgress") {
			return (
				<div className="absolute top-2 right-2">
					<div className="w-2 h-2 bg-purple-500 rounded-full"></div>
				</div>
			);
		}
		return null;
	};

	return (
		<Card
			className={`relative p-4 cursor-pointer transition-all duration-300 hover:shadow-md ${getBorderClass()} ${
				claim.isFlagged ? "border-l-4 border-l-danger-500" : ""
			}`}
			onClick={onClick}
		>
			{getProcessingIndicator()}

			<div className="flex justify-between items-start mb-3">
				<Chip color={getRiskColor()} variant="flat" size="sm">
					{claim.fraudRiskLevel} RISK
				</Chip>
				<div className="text-right">
					<span className="text-xs text-gray-500 font-mono">
						{claim.transactionNumber}
					</span>
					{claim.isFlagged && (
						<div className="flex items-center mt-1">
							<AlertTriangle className="w-3 h-3 text-danger-500 mr-1" />
							<span className="text-xs text-danger-500">Flagged</span>
						</div>
					)}
				</div>
			</div>

			<div className="mb-3">
				<h3 className="font-semibold text-gray-800 mb-1">
					{claim.patientName}
				</h3>
				<p className="text-sm text-gray-600">
					Member: {claim.membershipNumber}
				</p>
			</div>

			<div className="flex justify-between items-center mb-3">
				<div className="flex items-center text-gray-700">
					<DollarSign className="w-4 h-4 mr-1" />
					<span className="font-medium">
						{claim.currency}
						{claim.claimAmount.toLocaleString()} claimed
					</span>
				</div>
			</div>

			<div className="flex items-center text-sm text-gray-500 mb-3">
				<Hospital className="w-4 h-4 mr-1" />
				<span>Practice: {claim.providerPractice}</span>
			</div>

			<div className="flex justify-between items-center text-sm text-gray-500">
				<div className="flex items-center">
					<Clock className="w-4 h-4 mr-1" />
					<span>{claim.timeAgo}</span>
				</div>
				<div className="flex items-center">
					<ClipboardList className="w-4 h-4 mr-1" />
					<span>{claim.itemSummary}</span>
				</div>
			</div>
		</Card>
	);
};
