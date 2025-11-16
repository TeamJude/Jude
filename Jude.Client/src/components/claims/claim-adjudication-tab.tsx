import { submitHumanReview } from "@/lib/services/claims.service";
import { ClaimDecision, type GetClaimDetailResponse } from "@/lib/types/claim";
import {
	addToast,
	Button,
	Card,
	CardBody,
	Chip,
	Divider,
	Textarea,
} from "@heroui/react";
import { useQueryClient } from "@tanstack/react-query";
import { CheckCircle, User, XCircle } from "lucide-react";
import React from "react";

interface ClaimAdjudicationTabProps {
	claim: GetClaimDetailResponse;
}

export const ClaimAdjudicationTab: React.FC<ClaimAdjudicationTabProps> = ({
	claim,
}) => {
	const [decision, setDecision] = React.useState<ClaimDecision>(
		ClaimDecision.None,
	);
	const [comments, setComments] = React.useState("");
	const [isSubmitting, setIsSubmitting] = React.useState(false);
	const queryClient = useQueryClient();

	const existingReview = claim.humanReview;

	const handleSubmitReview = async () => {
		if (decision === ClaimDecision.None || !comments.trim()) {
			addToast({
				title: "Please select a decision and add comments",
				color: "warning",
			});
			return;
		}

		setIsSubmitting(true);
		try {
			const result = await submitHumanReview(claim.id, {
				decision,
				comments: comments.trim(),
			});

			if (result.success) {
				addToast({
					title: "Human review submitted successfully!",
					color: "success",
				});

				queryClient.invalidateQueries({ queryKey: ["claim", claim.id] });

				setDecision(ClaimDecision.None);
				setComments("");
			} else {
				addToast({
					title: result.errors?.[0] || "Failed to submit review",
					color: "danger",
				});
			}
		} catch (error) {
			addToast({
				title: "An error occurred while submitting the review",
				color: "danger",
			});
		} finally {
			setIsSubmitting(false);
		}
	};

	const getDecisionColor = (decision: ClaimDecision) => {
		if (decision === ClaimDecision.Approve) return "success";
		if (decision === ClaimDecision.Reject) return "danger";
		return "default";
	};

	const getDecisionIcon = (decision: ClaimDecision) => {
		if (decision === ClaimDecision.Approve)
			return <CheckCircle className="w-5 h-5" />;
		if (decision === ClaimDecision.Reject)
			return <XCircle className="w-5 h-5" />;
		return null;
	};

	return (
		<Card shadow="none">
			<CardBody className="space-y-6">
				{/* Existing Human Review */}
				{existingReview && (
					<>
						<div>
							<div className="flex items-center justify-between mb-4">
								<h3 className="text-lg font-medium">Current Human Review</h3>
								<Chip
									color={getDecisionColor(existingReview.decisionStatus)}
									variant="flat"
									startContent={getDecisionIcon(existingReview.decisionStatus)}
								>
									{ClaimDecision[existingReview.decisionStatus]}
								</Chip>
							</div>
							<Card shadow="sm" className="">
								<CardBody className="p-4 space-y-3">
									<div className="flex items-center justify-between">
										<div className="flex items-center gap-2">
											<User className="w-4 h-4 text-foreground-500" />
											<div className="flex flex-col">
												<span className="text-xs text-foreground-500">
													Reviewed By
												</span>
												<span className="text-sm font-medium">
													{claim.reviewedBy
														? claim.reviewedBy.username ||
															claim.reviewedBy.email
														: "Unknown"}
												</span>
											</div>
										</div>
										<div className="flex flex-col text-right">
											<span className="text-xs text-foreground-500">
												Reviewed At
											</span>
											<span className="text-sm">
												{new Date(existingReview.reviewedAt).toLocaleString()}
											</span>
										</div>
									</div>
									<Divider />
									<div>
										<div className="text-sm text-foreground-500 mb-2">
											Comments:
										</div>
										<p className="text-sm text-foreground-700 whitespace-pre-wrap">
											{existingReview.comments}
										</p>
									</div>
								</CardBody>
							</Card>
						</div>
						<Divider />
					</>
				)}

				{/* New Review Form */}
				<div>
					<h3 className="text-lg font-medium mb-4">
						{existingReview ? "Update Review" : "Submit Human Review"}
					</h3>

					{/* Decision Selection */}
					<div>
						<h4 className="text-sm font-medium text-foreground-700 mb-3">
							Make Decision
						</h4>
						<div className="grid grid-cols-2 gap-3">
							<Button
								color="success"
								variant={decision === ClaimDecision.Approve ? "solid" : "flat"}
								startContent={<CheckCircle className="w-5 h-5" />}
								onPress={() => setDecision(ClaimDecision.Approve)}
								className="h-14"
								size="lg"
							>
								Approve
							</Button>
							<Button
								color="danger"
								variant={decision === ClaimDecision.Reject ? "solid" : "flat"}
								startContent={<XCircle className="w-5 h-5" />}
								onPress={() => setDecision(ClaimDecision.Reject)}
								className="h-14"
								size="lg"
							>
								Reject
							</Button>
						</div>
						{decision !== ClaimDecision.None && (
							<div className="mt-3 p-3 bg-content2 rounded-md flex items-center gap-2">
								{getDecisionIcon(decision)}
								<span className="text-sm font-medium">
									Selected: {ClaimDecision[decision]}
								</span>
							</div>
						)}
					</div>
				</div>

				<Divider />

				{/* Comments */}
				<div>
					<h4 className="text-sm font-medium text-foreground-700 mb-3">
						Add Comments
					</h4>
					<Textarea
						placeholder="Enter detailed comments explaining your decision..."
						value={comments}
						onValueChange={setComments}
						minRows={5}
						className="w-full"
						description="Required: Provide clear reasoning for your decision"
					/>
				</div>

				<Divider />

				{/* Submit */}
				<div className="flex justify-between items-center">
					<div className="text-sm text-foreground-500">
						{decision !== ClaimDecision.None && comments.trim() ? (
							<span className="text-success flex items-center gap-1">
								<CheckCircle className="w-4 h-4" />
								Ready to submit
							</span>
						) : (
							<span>Please select a decision and add comments</span>
						)}
					</div>
					<Button
						color="primary"
						isDisabled={decision === ClaimDecision.None || !comments.trim()}
						isLoading={isSubmitting}
						onPress={handleSubmitReview}
						size="lg"
					>
						Submit Review
					</Button>
				</div>
			</CardBody>
		</Card>
	);
};
