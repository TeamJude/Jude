import { createReview, submitReview, updateReview } from "@/lib/services/claims.service";
import type { ClaimReview, GetClaimDetailResponse } from "@/lib/types/claim";
import { ClaimReviewDecision } from "@/lib/types/claim";
import {
	addToast,
	Button,
	Card,
	CardBody,
	Chip,
	Divider,
	Textarea,
} from "@heroui/react";
import {
	Check,
	Clock,
	Edit,
	User,
	X
} from "lucide-react";
import React from "react";

interface ClaimAdjudicationTabProps {
	claim: GetClaimDetailResponse;
	previousReviews: ClaimReview[];
}

export const ClaimAdjudicationTab: React.FC<ClaimAdjudicationTabProps> = ({ 
	claim, 
	previousReviews = [] 
}) => {
	const [decision, setDecision] = React.useState<ClaimReviewDecision | "">("");
	const [notes, setNotes] = React.useState("");
	const [showPreviousReviews, setShowPreviousReviews] = React.useState(false);
	const [currentReviewId, setCurrentReviewId] = React.useState<string | null>(null);
	const [isSubmitting, setIsSubmitting] = React.useState(false);

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
					claimId: claim.id,
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

	return (
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
								{previousReviews.map((review: ClaimReview) => (
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
	);
};
