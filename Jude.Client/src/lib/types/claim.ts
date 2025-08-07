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

// CIMAS Data Structures
export interface ClaimResponsePersonal {
	surname: string;
	firstName: string;
	initials?: string;
	gender: string;
	dateOfBirth: string;
}

export interface ClaimResponsePatient {
	dependantCode: number;
	personal: ClaimResponsePersonal;
}

export interface ClaimResponseMember {
	medicalSchemeNumber: number;
	medicalSchemeName: string;
	currency: string;
}

export interface ClaimTotalValues {
	claimed: string;
	copayment: string;
	schemeAmount: string;
	savingsAmount: string;
	nettMember: string;
	nettProvider: string;
}

export interface Message {
	type: string;
	code?: string;
	description?: string;
}

export interface ServiceResponse {
	number: string;
	code: string;
	description: string;
	subTotalValues: ClaimTotalValues;
	message: Message;
	totalValues: ClaimTotalValues;
}

export interface ProductResponse {
	number: string;
	code: string;
	description: string;
	subTotalValues: ClaimTotalValues;
	message: Message;
	totalValues: ClaimTotalValues;
}

export interface TransactionResponse {
	type?: string;
	number: string;
	claimNumber: string;
	dateTime: string;
	submittedBy: string;
	reversed: boolean;
	dateReversed?: string;
}

export interface ClaimHeaderResponse {
	responseCode: string;
	totalValues: ClaimTotalValues;
}

export interface ClaimResponse {
	transactionResponse: TransactionResponse;
	member: ClaimResponseMember;
	patient: ClaimResponsePatient;
	claimHeaderResponse: ClaimHeaderResponse;
	serviceResponse: ServiceResponse[];
	productResponse: ProductResponse[];
}

// Backend Contract Types
export interface GetClaimResponse {
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

export interface AgentReviewResponse {
	id: string;
	reviewedAt: string;
	decisionStatus: ClaimDecision;
	recommendation: string;
	reasoning: string;
	confidenceScore: number;
}

export interface HumanReviewResponse {
	id: string;
	reviewedAt: string;
	decisionStatus: ClaimDecision;
	comments: string;
}

export interface ReviewerInfo {
	id: string;
	username?: string;
	email: string;
}

export interface GetClaimDetailResponse {
	id: string;
	ingestedAt: string;
	updatedAt: string;
	status: ClaimStatus;
	data: ClaimResponse;
	transactionNumber: string;
	claimNumber: string;
	patientFirstName: string;
	patientSurname: string;
	medicalSchemeName: string;
	totalClaimAmount: number;
	agentReview?: AgentReviewResponse;
	humanReview?: HumanReviewResponse;
	reviewedBy?: ReviewerInfo;
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
	reviewer: ReviewerInfo;
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

// Legacy interface for backward compatibility (deprecated)
export interface Claim {
	id: string;
	ingestedAt: string;
	updatedAt: string;
	status: ClaimStatus;
	data: ClaimResponse;
	transactionNumber: string;
	claimNumber: string;
	patientFirstName: string;
	patientSurname: string;
	medicalSchemeName: string;
	totalClaimAmount: number;
	agentReview?: AgentReviewResponse;
	humanReview?: HumanReviewResponse;
	reviewedBy?: ReviewerInfo;
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

// Dashboard types
export interface ClaimsActivityResponse {
	day: string;
	newClaims: number;
	processed: number;
	approved: number;
	rejected: number;
}

export interface ClaimsDashboardResponse {
	totalClaims: number;
	autoApprovedRate: number;
	avgProcessingTime: number;
	claimsFlagged: number;
	totalClaimsChangePercent: number;
	autoApprovedRateChangePercent: number;
	avgProcessingTimeChangePercent: number;
	claimsFlaggedChangePercent: number;
	claimsActivity: ClaimsActivityResponse[];
}
