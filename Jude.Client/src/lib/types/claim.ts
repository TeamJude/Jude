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

export enum ClaimsDashboardPeriod {
	Last24Hours,
    Last7Days,
    Last30Days,
    LastQuarter
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
