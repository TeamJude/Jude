import type { GetClaimDetailResponse } from "@/lib/types/claim";
import {
	Card,
	CardBody,
	Chip,
} from "@heroui/react";
import {
	Cpu,
	Inbox,
	User,
	UserCheck,
} from "lucide-react";
import React from "react";

interface ClaimAuditTabProps {
	claim: GetClaimDetailResponse;
}

export const ClaimAuditTab: React.FC<ClaimAuditTabProps> = ({ claim }) => {
	return (
		<Card shadow="none">
			<CardBody>
				<h3 className="text-lg font-medium mb-4">Claim Activity Log</h3>

				<div className="space-y-6">
					<div className="flex gap-4">
						<div className="flex flex-col items-center">
							<div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-white">
								<Inbox width={16} />
							</div>
							<div className="flex-grow w-0.5 bg-divider my-2"></div>
						</div>
						<div>
							<div className="flex items-center gap-2">
								<h4 className="font-medium">Claim Ingested</h4>
								<Chip size="sm" variant="flat" color="primary">
									System
								</Chip>
							</div>
							<p className="text-sm text-foreground-500 mt-1">
								{new Date(claim.ingestedAt).toLocaleString()}
							</p>
							<p className="text-sm mt-2">
								Claim #{claim.transactionNumber} was received via Portal
								and entered into the system.
							</p>
						</div>
					</div>

					<div className="flex gap-4">
						<div className="flex flex-col items-center">
							<div className="w-8 h-8 rounded-full bg-secondary flex items-center justify-center text-white">
								<Cpu width={16} />
							</div>
							<div className="flex-grow w-0.5 bg-divider my-2"></div>
						</div>
						<div>
							<div className="flex items-center gap-2">
								<h4 className="font-medium">Agent Processing Started</h4>
								<Chip size="sm" variant="flat" color="secondary">
									AI Agent
								</Chip>
							</div>
							<p className="text-sm text-foreground-500 mt-1">
								May 15, 2023 - 09:35 AM
							</p>
							<p className="text-sm mt-2">
								AI Agent began processing the claim.
							</p>
						</div>
					</div>

					<div className="flex gap-4">
						<div className="flex flex-col items-center">
							<div className="w-8 h-8 rounded-full bg-secondary flex items-center justify-center text-white">
								<Cpu width={16} />
							</div>
							<div className="flex-grow w-0.5 bg-divider my-2"></div>
						</div>
						<div>
							<div className="flex items-center gap-2">
								<h4 className="font-medium">
									Agent Processing Completed
								</h4>
								<Chip size="sm" variant="flat" color="secondary">
									AI Agent
								</Chip>
							</div>
							<p className="text-sm text-foreground-500 mt-1">
								{claim.agentReview?.reviewedAt
									? new Date(claim.agentReview.reviewedAt).toLocaleString()
									: "Pending"}
							</p>
							<p className="text-sm mt-2">
								{claim.agentReview?.reviewedAt
									? `AI Agent completed processing with recommendation: ${claim.agentReview.recommendation || "Review"}`
									: "AI Agent is queued for processing this claim"}
								.
							</p>
							{!claim.agentReview?.reviewedAt && (
								<div className="mt-2 p-2 bg-secondary-100 rounded text-xs">
									<strong>Processing Queue:</strong> Claim is in the AI processing queue
								</div>
							)}
						</div>
					</div>

					<div className="flex gap-4">
						<div className="flex flex-col items-center">
							<div className="w-8 h-8 rounded-full bg-warning flex items-center justify-center text-white">
								<UserCheck width={16} />
							</div>
							<div className="flex-grow w-0.5 bg-divider my-2"></div>
						</div>
						<div>
							<div className="flex items-center gap-2">
								<h4 className="font-medium">
									Status Changed to Pending Human Review
								</h4>
								<Chip size="sm" variant="flat" color="warning">
									System
								</Chip>
							</div>
							<p className="text-sm text-foreground-500 mt-1">
								May 15, 2023 - 09:36 AM
							</p>
							<p className="text-sm mt-2">
								Claim status was updated to "Pending Human Review".
							</p>
						</div>
					</div>

					<div className="flex gap-4">
						<div className="flex flex-col items-center">
							<div className="w-8 h-8 rounded-full bg-default flex items-center justify-center text-white">
								<User width={16} />
							</div>
						</div>
						<div>
							<div className="flex items-center gap-2">
								<h4 className="font-medium">Viewed by John Smith</h4>
								<Chip size="sm" variant="flat" color="default">
									User
								</Chip>
							</div>
							<p className="text-sm text-foreground-500 mt-1">
								May 15, 2023 - 10:15 AM
							</p>
							<p className="text-sm mt-2">
								Claim details were viewed by John Smith (Claims
								Adjudicator).
							</p>
						</div>
					</div>
				</div>
			</CardBody>
		</Card>
	);
};
