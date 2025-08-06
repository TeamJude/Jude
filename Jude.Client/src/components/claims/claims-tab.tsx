import { createReview, getClaim, getUserReviewForClaim, submitReview, updateReview } from "@/lib/services/claims.service";
import { authState } from "@/lib/state/auth.state";
import { ClaimReviewDecision, FraudRiskLevel } from "@/lib/types/claim";
import {
	addToast,
	Button,
	Card,
	CardBody,
	Chip,
	Divider,
	Progress,
	Spinner,
	Tab,
	Tabs,
	Textarea,
} from "@heroui/react";
import { useQuery } from "@tanstack/react-query";
import {
	AlertCircle,
	BookOpen,
	Check,
	CheckSquare,
	ClipboardList,
	Clock,
	Cpu,
	Edit,
	History,
	Inbox,
	User,
	UserCheck,
	X
} from "lucide-react";
import React from "react";

interface ClaimTabsProps {
	claimId: string;
}

export const ClaimTabs: React.FC<ClaimTabsProps> = ({ claimId }) => {
	const { user } = authState.state;
	const [selected, setSelected] = React.useState("summary");
	const [decision, setDecision] = React.useState<ClaimReviewDecision | "">("");
	const [notes, setNotes] = React.useState("");
	const [showPreviousReviews, setShowPreviousReviews] = React.useState(false);
	const [currentReviewId, setCurrentReviewId] = React.useState<string | null>(null);
	const [isSubmitting, setIsSubmitting] = React.useState(false);

	const {
		data: claimResponse,
		isLoading,
		error,
	} = useQuery({
		queryKey: ["claim", claimId],
		queryFn: () => getClaim(claimId),
	});

	const {
		data: userReviewResponse,
		isLoading: isLoadingUserReview,
	} = useQuery({
		queryKey: ["userReview", claimId, user?.id],
		queryFn: () => getUserReviewForClaim(claimId),
		enabled: !!user?.id,
	});

	// Pre-populate form when user review is loaded
	React.useEffect(() => {
		if (userReviewResponse?.success && userReviewResponse.data) {
			const userReview = userReviewResponse.data;
			setDecision(userReview.decision);
			setNotes(userReview.notes);
			setCurrentReviewId(userReview.id);
		}
	}, [userReviewResponse]);

	const handleSelectionChange = (key: React.Key) => {
		setSelected(key.toString());
	};

	const handleSubmitReview = async () => {
		if (!decision || !notes.trim()) return;

		setIsSubmitting(true);
		try {
			if (currentReviewId) {
				// Update existing review
				const updateResult = await updateReview(currentReviewId, {
					decision,
					notes: notes.trim(),
				});
				
				if (updateResult.success) {
					// Submit the review
					await submitReview(currentReviewId);
					addToast({
						title: "Review updated and submitted successfully!",
						color: "success",
					});
				} else {
					addToast({
						title: "Failed to update review",
						color: "danger",
					});
				}
			} else {
				// Create new review
				const createResult = await createReview({
					claimId,
					decision,
					notes: notes.trim(),
				});
				
				if (createResult.success) {
					// Submit the new review
					await submitReview(createResult.data.id);
					setCurrentReviewId(createResult.data.id);
					addToast({
						title: "Review submitted successfully!",
						color: "success",
					});
				} else {
					addToast({
						title: "Failed to create review",
						color: "danger",
					});
				}
			}
			
			// Reset form
			setDecision("");
			setNotes("");
		} catch (error) {
			addToast({
				title: "An error occurred while submitting the review",
				color: "danger",
			});
		} finally {
			setIsSubmitting(false);
		}
	};

	if (isLoading) {
		return (
			<div className="flex items-center justify-center p-8">
				<Spinner label="Loading claim details..." />
			</div>
		);
	}

	if (error || !claimResponse?.success) {
		return (
			<div className="flex flex-col items-center justify-center p-8">
				<AlertCircle className="w-8 h-8 text-danger mb-2" />
				<p className="text-danger">Failed to load claim details</p>
			</div>
		);
	}

	const claim = claimResponse.data;
	const previousReviews = claim.reviews || [];

	return (
		<div className="flex w-full flex-col">
			<Tabs
				aria-label="Claim details tabs"
				selectedKey={selected}
				onSelectionChange={handleSelectionChange}
				color="primary"
				variant="underlined"
				classNames={{
					tabList: "gap-6",
					cursor: "w-full bg-primary",
					tab: "max-w-fit px-0 h-12",
				}}
			>
				<Tab
					key="summary"
					title={
						<div className="flex items-center gap-2">
							<ClipboardList width={18} />
							<span>Claim Summary & Agent Output</span>
						</div>
					}
				>
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
												{" "}
												<div className="flex justify-between">
													<span className="text-sm text-foreground-500">
														Billed Amount
													</span>
													<span className="text-sm">
														{claim.currency}
														{claim.claimAmount.toLocaleString()}
													</span>
												</div>
												<div className="flex justify-between">
													<span className="text-sm text-foreground-500">
														Allowed Amount
													</span>
													<span className="text-sm">
														{claim.approvedAmount
															? `${claim.currency}${claim.approvedAmount.toLocaleString()}`
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
										{claim.agentReasoningLog && claim.agentReasoningLog.length > 0 ? (
											claim.agentReasoningLog.map((reasoning, index) => (
												<div key={index} className="flex gap-2">
													<span className="bg-primary text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold mt-0.5">
														{index + 1}
													</span>
													<div>
														<p className="text-sm">{reasoning}</p>
													</div>
												</div>
											))
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
												{" "}
												<div className="flex items-center gap-2 mb-2">
													<UserCheck className="text-warning" width={20} />
													<h4 className="font-medium">
														{claim.agentRecommendation ||
															"Pending Agent Processing"}
													</h4>
												</div>
												<p className="text-sm">
													{claim.agentReasoning ||
														"This claim is queued for AI agent processing. The agent will analyze policy compliance, validate tariff codes, assess fraud risk, and provide a comprehensive recommendation with supporting citations."}
												</p>
												{!claim.agentRecommendation && (
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
											{" "}
											<div>
												<div className="flex justify-between mb-1">
													<span className="text-sm">Fraud Risk</span>
													<span className="text-sm font-medium">
														{claim.fraudRiskLevel || "Pending"}
													</span>
												</div>
												<Progress
													value={
														claim.fraudRiskLevel
															? {
																	[FraudRiskLevel.Low]: 15,
																	[FraudRiskLevel.Medium]: 40,
																	[FraudRiskLevel.High]: 70,
																	[FraudRiskLevel.Critical]: 90,
																}[claim.fraudRiskLevel]
															: 25
													}
													color={
														claim.fraudRiskLevel
															? {
																	[FraudRiskLevel.Low]: "success" as const,
																	[FraudRiskLevel.Medium]: "warning" as const,
																	[FraudRiskLevel.High]: "danger" as const,
																	[FraudRiskLevel.Critical]: "danger" as const,
																}[claim.fraudRiskLevel]
															: "warning"
													}
													className="h-2"
												/>
												{!claim.fraudRiskLevel && (
													<p className="text-xs text-foreground-500 mt-1">
														Risk assessment pending AI analysis
													</p>
												)}
											</div>
											<div>
												<div className="flex justify-between mb-1">
													<span className="text-sm">Agent Confidence</span>
													<span className="text-sm font-medium">
														{claim.agentConfidenceScore
															? Math.round(claim.agentConfidenceScore * 100) + "%"
															: "Pending"}
													</span>
												</div>
												<Progress
													value={
														claim.agentConfidenceScore
															? claim.agentConfidenceScore * 100
															: 0
													}
													color="primary"
													className="h-2"
												/>
												{!claim.agentConfidenceScore && (
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
				</Tab>

				<Tab
					key="policy"
					title={
						<div className="flex items-center gap-2">
							<BookOpen width={18} />
							<span>Policy Context</span>
						</div>
					}
				>
					<Card shadow="none">
						<CardBody>
							<h3 className="text-lg font-medium mb-4">Relevant Policies</h3>

							<div className="space-y-4">
								{claim.citations && claim.citations.length > 0 ? (
									claim.citations.map((citation, index) => (
										<Card key={citation.id} className="shadow-none border border-divider">
											<CardBody>
												<div className="flex items-center justify-between mb-2">
													<h4 className="font-medium">
														{citation.source}
													</h4>
													<Chip size="sm" variant="flat" color="primary">
														{citation.type}
													</Chip>
												</div>
												<p className="text-sm text-foreground-500 mt-1">
													Cited on {new Date(citation.citedAt).toLocaleDateString()}
												</p>
												<div className="mt-3 p-3 bg-content2 rounded-md">
													<p className="text-sm">
														"{citation.quote}"
													</p>
												</div>
												<div className="mt-3">
													<h5 className="text-sm font-medium">
														Agent's Interpretation:
													</h5>
													<p className="text-sm mt-1">
														{citation.context}
													</p>
												</div>
											</CardBody>
										</Card>
									))
								) : (
									<div className="space-y-4">
										<div className="text-center py-8">
											<BookOpen className="w-12 h-12 text-foreground-400 mx-auto mb-4" />
											<p className="text-foreground-500 font-medium">
												Policy Analysis Pending
											</p>
											<p className="text-sm text-foreground-400 mt-1">
												AI Agent will search relevant policies and tariffs to support the decision.
											</p>
										</div>
										
										<Card className="shadow-none border border-divider bg-content1">
											<CardBody>
												<div className="flex items-center justify-between mb-2">
													<h4 className="font-medium text-foreground-500">
														Expected Policy Analysis
													</h4>
													<Chip size="sm" variant="flat" color="default">
														Pending
													</Chip>
												</div>
												<div className="mt-3 p-3 bg-content2 rounded-md">
													<p className="text-sm text-foreground-500 italic">
														The AI agent will search for relevant coverage policies, billing guidelines, and tariff information to validate this claim.
													</p>
												</div>
												<div className="mt-3">
													<h5 className="text-sm font-medium text-foreground-500">
														Expected Analysis Areas:
													</h5>
													<ul className="text-xs text-foreground-500 mt-1 space-y-1">
														<li>• Coverage limits and eligibility</li>
														<li>• Pre-authorization requirements</li>
														<li>• Network provider status</li>
														<li>• Tariff code validation</li>
														<li>• Fraud detection indicators</li>
													</ul>
												</div>
											</CardBody>
										</Card>
									</div>
								)}
							</div>
						</CardBody>
					</Card>
				</Tab>

				<Tab
					key="adjudication"
					title={
						<div className="flex items-center gap-2">
							<CheckSquare width={18} />
							<span>Human Review & Adjudication</span>
						</div>
					}
				>
					<Card shadow="none">
						<CardBody className="space-y-6">
							{/* Previous Reviews Section */}
							{previousReviews.length > 0 && (
								<div>
									<div className="flex items-center justify-between mb-4">
										<h3 className="text-lg font-medium">Previous Reviews</h3>
										<Button
											variant="light"
											size="sm"
											onPress={() => setShowPreviousReviews(!showPreviousReviews)}
											className="transition-all duration-200 ease-in-out"
										>
											{showPreviousReviews ? "Hide" : "Show"} ({previousReviews.length})
										</Button>
									</div>
									
									<div className={`transition-all duration-300 ease-in-out overflow-hidden ${
										showPreviousReviews ? "max-h-[1000px] opacity-100" : "max-h-0 opacity-0"
									}`}>
										<div className="space-y-4 mb-6">
											{previousReviews.map((review) => (
												<Card key={review.id} className="border border-divider">
													<CardBody className="p-4">
														<div className="flex items-center justify-between mb-2">
															<div className="flex items-center gap-2">
																<div className="flex items-center gap-2">
																	<User width={16} className="text-foreground-500" />
																	<span className="font-medium">{review.reviewer.username || review.reviewer.email}</span>
																</div>
																<Chip size="sm" variant="flat" color="default">
																	Reviewer
																</Chip>
															</div>
															<div className="flex items-center gap-2">
																<Chip 
																	size="sm" 
																	color={
																		review.decision === ClaimReviewDecision.Approve ? "success" :
																		review.decision === ClaimReviewDecision.Reject ? "danger" :
																		review.decision === ClaimReviewDecision.Pend ? "warning" : "primary"
																	}
																	variant="flat"
																>
																	{ClaimReviewDecision[review.decision]}
																</Chip>
																<span className="text-xs text-foreground-500">
																	{new Date(review.submittedAt || review.updatedAt).toLocaleString()}
																</span>
															</div>
														</div>
														<p className="text-sm text-foreground-600">{review.notes}</p>
													</CardBody>
												</Card>
											))}
										</div>
									</div>
									
									<Divider />
								</div>
							)}

							{/* Current Review Form */}
							<div>
								<h3 className="text-lg font-medium mb-4">
									{previousReviews.length > 0 ? "Add Your Review" : "Make Decision"}
								</h3>
								<div className="grid grid-cols-2 md:grid-cols-4 gap-3">
									<Button
										color="success"
										variant={decision === ClaimReviewDecision.Approve ? "solid" : "flat"}
										startContent={<Check width={16} />}
										onPress={() => setDecision(ClaimReviewDecision.Approve)}
										className="h-12"
									>
										Approve
									</Button>
									<Button
										color="warning"
										variant={decision === ClaimReviewDecision.Pend ? "solid" : "flat"}
										startContent={<Clock width={16} />}
										onPress={() => setDecision(ClaimReviewDecision.Pend)}
										className="h-12"
									>
										Pend
									</Button>
									<Button
										color="primary"
										variant={decision === ClaimReviewDecision.Partial ? "solid" : "flat"}
										startContent={<Edit width={16} />}
										onPress={() => setDecision(ClaimReviewDecision.Partial)}
										className="h-12"
									>
										Partial
									</Button>
									<Button
										color="danger"
										variant={decision === ClaimReviewDecision.Reject ? "solid" : "flat"}
										startContent={<X width={16} />}
										onPress={() => setDecision(ClaimReviewDecision.Reject)}
										className="h-12"
									>
										Reject
									</Button>
								</div>
								{decision && (
									<div className="mt-3 p-3 bg-content2 rounded-md">
										<p className="text-sm">
											<span className="font-medium">Selected:</span> {ClaimReviewDecision[decision]}
										</p>
									</div>
								)}
							</div>

							<Divider />

							{/* Step 2: Notes */}
							<div>
								<h3 className="text-lg font-medium mb-4">Add Notes</h3>
								<Textarea
									placeholder={
										previousReviews.length > 0 
											? "Add your review comments and reasoning..." 
											: "Enter your reasoning for this decision..."
									}
									value={notes}
									onValueChange={setNotes}
									minRows={4}
									className="w-full"
								/>
								<p className="text-xs text-foreground-500 mt-2">
									Required: Provide detailed reasoning{previousReviews.length > 0 ? " and reference previous reviews if needed" : ", especially if overriding the agent's recommendation"}.
								</p>
							</div>

							<Divider />

							{/* Step 3: Submit */}
							<div className="flex justify-between items-center">
								<div className="text-sm text-foreground-500">
									{decision && notes ? (
										<span className="text-success">✓ Ready to submit</span>
									) : (
										<span>Please select a decision and add notes</span>
									)}
								</div>
								<div className="flex gap-3">
									<Button
										color="primary"
										isDisabled={!decision || !notes.trim()}
										isLoading={isSubmitting}
										onPress={handleSubmitReview}
									>
										{currentReviewId ? "Update Review" : "Submit Review"}
									</Button>
								</div>
							</div>
						</CardBody>
					</Card>
				</Tab>

				<Tab
					key="audit"
					title={
						<div className="flex items-center gap-2">
							<History width={18} />
							<span>Audit Trail</span>
						</div>
					}
				>
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
										</div>{" "}
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
										</div>{" "}
										<p className="text-sm text-foreground-500 mt-1">
											{claim.agentProcessedAt
												? new Date(claim.agentProcessedAt).toLocaleString()
												: "Pending"}
										</p>
										<p className="text-sm mt-2">
											{claim.agentProcessedAt
												? `AI Agent completed processing with recommendation: ${claim.agentRecommendation || "Review"}`
												: "AI Agent is queued for processing this claim"}
											.
										</p>
										{!claim.agentProcessedAt && (
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
				</Tab>
			</Tabs>
		</div>
	);
};
