import { ClaimDecision, type GetClaimDetailResponse } from "@/lib/types/claim";
import {
	Card,
	CardBody,
	Chip,
	Divider,
	Progress,
} from "@heroui/react";
import { UserCheck } from "lucide-react";
import React from "react";

interface ClaimSummaryTabProps {
	claim: GetClaimDetailResponse;
}

export const ClaimSummaryTab: React.FC<ClaimSummaryTabProps> = ({ claim }) => {
	return (
		<Card shadow="none">
			<CardBody className="gap-6">
				<div>
					<h3 className="text-lg font-medium mb-3">Extracted Data</h3>
					<div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
						<div>
							<h4 className="text-sm font-medium">Claim Details</h4>
							<div className="mt-2 space-y-2">
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Service Type
									</span>
									<span className="text-sm">Consultation</span>
								</div>
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Service Code
									</span>
									<span className="text-sm">CON-2023</span>
								</div>
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Diagnosis Code
									</span>
									<span className="text-sm">J45.909</span>
								</div>
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Place of Service
									</span>
									<span className="text-sm">Office</span>
								</div>
							</div>
						</div>

						<div>
							<h4 className="text-sm font-medium">Financial Details</h4>
							<div className="mt-2 space-y-2">
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Billed Amount
									</span>
									<span className="text-sm">
										${claim.totalClaimAmount.toLocaleString()}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Allowed Amount
									</span>
									<span className="text-sm">
										{claim.agentReview?.decisionStatus === ClaimDecision.Approve && (claim.data as any).member.currency
											? `${(claim.data as any)?.member.currency}${claim.totalClaimAmount.toLocaleString()}`
											: "Pending"}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Member Responsibility
									</span>
									<span className="text-sm">$150.00</span>
								</div>
								<div className="flex justify-between">
									<span className="text-sm text-foreground-500">
										Plan Payment
									</span>
									<span className="text-sm">$800.00</span>
								</div>
							</div>
						</div>
					</div>
				</div>

				<Divider />

				<div>
					<div className="flex items-center gap-2 mb-3">
						<h3 className="text-lg font-medium">Agent's Reasoning Log</h3>
						<Chip color="secondary" variant="flat" size="sm">
							AI Generated
						</Chip>
					</div>

					<div className="bg-content2 p-4 rounded-md space-y-4">
						{claim.agentReview?.reasoning ? (
							<div className="space-y-4">
								{claim.agentReview.reasoning.split('\n').map((reasoning: string, index: number) => (
									reasoning.trim() && (
										<div key={index} className="flex gap-2">
											<span className="bg-primary text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold mt-0.5">
												{index + 1}
											</span>
											<div>
												<p className="text-sm">{reasoning.trim()}</p>
											</div>
										</div>
									)
								))}
							</div>
						) : (
							<div className="space-y-4">
								<div className="flex gap-2">
									<span className="bg-primary text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold mt-0.5">1</span>
									<div>
										<p className="text-sm">
											<span className="font-medium">Claim Analysis Pending</span>
										</p>
										<p className="text-xs text-foreground-600 mt-1">
											AI Agent will analyze claim details, validate against policies, and check tariff compliance.
										</p>
									</div>
								</div>
								<div className="flex gap-2">
									<span className="bg-primary text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold mt-0.5">2</span>
									<div>
										<p className="text-sm">
											<span className="font-medium">Policy Validation</span>
										</p>
										<p className="text-xs text-foreground-600 mt-1">
											Checking coverage limits, pre-authorization requirements, and network status.
										</p>
									</div>
								</div>
								<div className="flex gap-2">
									<span className="bg-primary text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold mt-0.5">3</span>
									<div>
										<p className="text-sm">
											<span className="font-medium">Fraud Risk Assessment</span>
										</p>
										<p className="text-xs text-foreground-600 mt-1">
											Evaluating billing patterns, provider history, and unusual claim characteristics.
										</p>
									</div>
								</div>
								<div className="flex gap-2">
									<span className="bg-primary text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold mt-0.5">4</span>
									<div>
										<p className="text-sm">
											<span className="font-medium">Decision Generation</span>
										</p>
										<p className="text-xs text-foreground-600 mt-1">
											Finalizing recommendation based on comprehensive analysis and policy compliance.
										</p>
									</div>
								</div>
							</div>
						)}
					</div>
				</div>

				<Divider />

				<div className="grid grid-cols-1 md:grid-cols-2 gap-6">
					<div>
						<h3 className="text-lg font-medium mb-3">
							Agent's Recommendation
						</h3>
						<Card className="bg-warning-50 border-warning">
							<CardBody>
								<div className="flex items-center gap-2 mb-2">
									<UserCheck className="text-warning" width={20} />
									<h4 className="font-medium">
										{claim.agentReview?.recommendation ||
											"Pending Agent Processing"}
									</h4>
								</div>
								<p className="text-sm">
									{claim.agentReview?.reasoning ||
										"This claim is queued for AI agent processing. The agent will analyze policy compliance, validate tariff codes, assess fraud risk, and provide a comprehensive recommendation with supporting citations."}
								</p>
								{!claim.agentReview?.recommendation && (
									<div className="mt-3 p-2 bg-warning-100 rounded text-xs">
										<strong>Processing Status:</strong> Claim submitted and awaiting AI analysis
									</div>
								)}
							</CardBody>
						</Card>
					</div>

					<div>
						<h3 className="text-lg font-medium mb-3">Risk Assessment</h3>
						<div className="space-y-4">
							<div>
								<div className="flex justify-between mb-1">
									<span className="text-sm">Agent Confidence</span>
									<span className="text-sm font-medium">
										{claim.agentReview?.confidenceScore !== undefined
											? Math.round(claim.agentReview.confidenceScore * 100) + "%"
											: "Pending"}
									</span>
								</div>
								<Progress
									value={
										claim.agentReview?.confidenceScore !== undefined
											? claim.agentReview.confidenceScore * 100
											: 0
									}
									color="primary"
									className="h-2"
								/>
								{claim.agentReview?.confidenceScore === undefined && (
									<p className="text-xs text-foreground-500 mt-1">
										Confidence score will be calculated after processing
									</p>
								)}
							</div>
						</div>
					</div>
				</div>
			</CardBody>
		</Card>
	);
};
