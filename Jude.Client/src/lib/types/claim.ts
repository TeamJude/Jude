export enum ClaimStatus {
	Pending,
	UnderAgentReview,
	UnderHumanReview,
	Approved,
	Rejected,
	Completed,
	Failed,
}

export enum ClaimSource {
	CIMAS,
}

export enum ClaimDecision {
	None,
	Approve,
	Reject,
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

export interface ClaimSummary {
	id: string;
	transactionNumber: string;
	claimNumber: string;
	patientFirstName: string;
	patientSurname: string;
	medicalSchemeName: string;
	totalClaimAmount: number;
	status: ClaimStatus;
	ingestedAt: string;
	updatedAt: string;
}

export interface Claim {
	id: string;
	ingestedAt: string;
	updatedAt: string;
	status: ClaimStatus;
	data: any; 
	transactionNumber: string;
	claimNumber: string;
	patientFirstName: string;
	patientSurname: string;
	medicalSchemeName: string;
	totalClaimAmount: number;
	agentReview?: {
		id: string;
		reviewedAt: string;
		decisionStatus: ClaimDecision;
		recommendation: string;
		reasoning: string;
		confidenceScore: number;
	};
	humanReview?: {
		id: string;
		reviewedAt: string;
		decisionStatus: ClaimDecision;
		comments: string;
	};
	reviewedBy?: {
		id: string;
		username?: string;
		email: string;
	};
	// Additional properties that seem to be used in the component
	reviews?: ClaimReview[];
	citations?: Citation[];
	agentReasoningLog?: string[];
	agentRecommendation?: string;
	agentReasoning?: string;
	fraudRiskLevel?: FraudRiskLevel;
	agentConfidenceScore?: number;
	agentProcessedAt?: string;
	// Financial properties
	currency?: string;
	approvedAmount?: number;
	app?: boolean; // This seems to be checking if approved
}

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
