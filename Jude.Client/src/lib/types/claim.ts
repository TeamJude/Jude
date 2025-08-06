export enum ClaimStatus {
	Pending,
	Processing,
	Failed,
	Review,
	Completed,
}

export enum ClaimSource {
	CIMAS,
}

export enum ClaimDecision {
	Approved,
	Rejected,
}

export enum FraudRiskLevel {
	Low,
	Medium,
	High,
	Critical,
}

export enum ClaimReviewDecision {
	Approve,
	Reject,
	Pend,
	Partial,
}

export enum ClaimReviewStatus {
	Draft,
	Submitted,
}

export enum ClaimsDashboardPeriod {
	Last24Hours,
    Last7Days,
    Last30Days,
    LastQuarter
}

export interface Citation {
	id: string;
	type: string;
	source: string;
	quote: string;
	context: string;
	citedAt: string;
}

export interface ClaimReview {
	id: string;
	claimId: string;
	reviewer: {
		id: string;
		username?: string;
		email: string;
	};
	decision: ClaimReviewDecision;
	notes: string;
	status: ClaimReviewStatus;
	createdAt: string;
	updatedAt: string;
	submittedAt?: string;
	isEdited: boolean;
}

export interface Claim {
	id: string;
	transactionNumber: string;
	claimNumber: string;
	patientName: string;
	membershipNumber: string;
	providerPractice: string;
	claimAmount: number;
	approvedAmount?: number;
	currency: string;
	status: ClaimStatus;
	source: ClaimSource;
	submittedAt?: string;
	processedAt?: string;
	agentRecommendation?: string;
	agentReasoning?: string;
	agentReasoningLog?: string[];
	agentConfidenceScore?: number;
	agentProcessedAt?: string;
	fraudIndicators?: string[];
	fraudRiskLevel: FraudRiskLevel;
	isFlagged: boolean;
	requiresHumanReview: boolean;
	finalDecision?: ClaimDecision;
	reviewerComments?: string;
	rejectionReason?: string;
	reviewedAt?: string;
	reviewedBy?: {
		id: string;
		username?: string;
		email: string;
	};
	ingestedAt: string;
	updatedAt: string;
	cimasPayload?: string;
	citations?: Citation[];
	reviews: ClaimReview[];
}

export type ClaimSummary = Pick<
	Claim,
	| "id"
	| "transactionNumber"
	| "patientName"
	| "membershipNumber"
	| "providerPractice"
	| "claimAmount"
	| "approvedAmount"
	| "currency"
	| "status"
	| "source"
	| "fraudRiskLevel"
	| "isFlagged"
	| "requiresHumanReview"
	| "agentRecommendation"
	| "ingestedAt"
	| "updatedAt"
> & {
	submittedAt: string;
};

// Review-related request/response types
export interface CreateClaimReviewRequest {
	claimId: string;
	decision: ClaimReviewDecision;
	notes: string;
}

export interface UpdateClaimReviewRequest {
	decision: ClaimReviewDecision;
	notes: string;
}

export interface GetClaimReviewsResponse {
	reviews: ClaimReview[];
}
