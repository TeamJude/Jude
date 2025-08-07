import { ClaimStatus } from "@/lib/types/claim";
import { Button, Card, CardBody } from "@heroui/react";
import { Building2, Calendar, DollarSign, Printer, User } from "lucide-react";
import type React from "react";

interface ClaimDetailHeaderProps {
	claimId: string;
	memberName: string;
	memberId: string;
	providerName: string;
	providerId: string;
	dateReceived: string;
	dateOfService: string;
	amount: number;
	status: ClaimStatus;
}

const statusColorMap: Record<ClaimStatus, string> = {
	[ClaimStatus.Pending]: "ring-yellow-500",
	[ClaimStatus.UnderAgentReview]: "ring-blue-500",
	[ClaimStatus.UnderHumanReview]: "ring-orange-500",
	[ClaimStatus.Approved]: "ring-green-500",
	[ClaimStatus.Rejected]: "ring-red-500",
	[ClaimStatus.Completed]: "ring-green-500",
	[ClaimStatus.Failed]: "ring-red-500",
};

export const ClaimDetailHeader: React.FC<ClaimDetailHeaderProps> = ({
	memberName,
	memberId,
	providerName,
	providerId,
	dateReceived,
	dateOfService,
	amount,
	status,
}) => {
	const statusKey = status;
	const ringColor = statusColorMap[statusKey];

	return (
		<Card shadow="none" className="w-full">
			<CardBody>
				<div className="flex flex-row items-center gap-8 flex-wrap">
					{/* Member Info */}
					<div className="flex flex-col min-w-[160px]">
						<div className="flex items-center gap-2">
							<User className="text-primary w-4 h-4" />
							<div>
								<p className="text-xs text-foreground-500">Member</p>
								<p className="font-medium text-sm">{memberName}</p>
								<p className="text-xs text-foreground-500">{memberId}</p>
							</div>
						</div>
					</div>

					{/* Provider Info */}
					<div className="flex flex-col min-w-[140px]">
						<div className="flex items-center gap-2">
							<Building2 className="text-primary w-4 h-4" />
							<div>
								<p className="text-xs text-foreground-500">Provider</p>
								<p className="font-medium text-sm">{providerName}</p>
								<p className="text-xs text-foreground-500">{providerId}</p>
							</div>
						</div>
					</div>

					{/* Amount */}
					<div className="flex flex-col min-w-[100px]">
						<div className="flex items-center gap-2">
							<DollarSign className="text-primary w-4 h-4" />
							<div>
								<p className="text-xs text-foreground-500">Amount</p>
								<p className="font-medium text-sm">
									${amount.toLocaleString()}
								</p>
							</div>
						</div>
					</div>

					{/* Dates */}
					<div className="flex flex-col min-w-[140px]">
						<div className="flex items-center gap-2">
							<Calendar className="text-primary w-4 h-4" />
							<div>
								<p className="text-xs text-foreground-500">Service Date</p>
								<p className="font-medium text-sm">{dateOfService}</p>
								<p className="text-xs text-foreground-500">
									Received: {dateReceived}
								</p>
							</div>
						</div>
					</div>

					{/* Print Button */}
					<div className="ml-auto">
						<Button
							size="sm"
							variant="flat"
							color="primary"
							startContent={<Printer className="w-4 h-4" />}
						>
							Print
						</Button>
					</div>
				</div>
			</CardBody>
		</Card>
	);
};
