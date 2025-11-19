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
	Upload,
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

export enum ClaimSearchField {
	MemberNumber = 0,
	ClaimNumber = 1,
	ClaimLineNo = 2,
	PatientName = 3,
	ProviderName = 4,
	PracticeNumber = 5,
	All = 6,
}

// CIMAS Data Structures
export interface ClaimResponsePersonal {
	Surname: string;
	FirstName: string;
	Initials?: string;
	Gender: string;
	DateOfBirth: string;
}

export interface ClaimResponsePatient {
	DependantCode: number;
	Personal: ClaimResponsePersonal;
}

export interface ClaimResponseMember {
	MedicalSchemeNumber: number;
	MedicalSchemeName: string;
	Currency: string;
}

export interface ClaimTotalValues {
	Claimed: string;
	Copayment: string;
	SchemeAmount: string;
	SavingsAmount: string;
	NettMember: string;
	NettProvider: string;
}

export interface Message {
	Type: string;
	Code?: string;
	Description?: string;
}

export interface ServiceResponse {
	Number: string;
	Code: string;
	Description: string;
	SubTotalValues: ClaimTotalValues;
	Message: Message;
	TotalValues: ClaimTotalValues;
}

export interface ProductResponse {
	Number: string;
	Code: string;
	Description: string;
	SubTotalValues: ClaimTotalValues;
	Message: Message;
	TotalValues: ClaimTotalValues;
}

export interface TransactionResponse {
	Type?: string;
	Number: string;
	ClaimNumber: string;
	DateTime: string;
	SubmittedBy: string;
	Reversed: boolean;
	DateReversed?: string;
}

export interface ClaimHeaderResponse {
	ResponseCode: string;
	TotalValues: ClaimTotalValues;
}

export interface ClaimResponse {
	TransactionResponse: TransactionResponse;
	Member: ClaimResponseMember;
	Patient: ClaimResponsePatient;
	ClaimHeaderResponse: ClaimHeaderResponse;
	ServiceResponse: ServiceResponse[];
	ProductResponse: ProductResponse[];
}

// Backend Contract Types
export interface GetClaimResponse {
	id: string;
	claimNumber: string;
	claimLineNo: string;
	patientFirstName: string;
	patientSurname: string;
	memberNumber: string;
	providerName: string;
	practiceNumber: string;
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
	claimNumber: string;
	patientFirstName: string;
	patientSurname: string;
	memberNumber: string;
	ino: string;
	dis: string;
	medicalSchemeName: string;
	optionName: string;
	payerName: string;
	providerName: string;
	practiceNumber: string;
	invoiceReference: string;
	asAtNetworks: string;
	referringPractice: string;
	serviceDate: string;
	assessmentDate: string;
	dateReceived: string;
	claimCode: string;
	codeDescription: string;
	units: number;
	scriptCode: string;
	icd10Code: string;
	totalClaimAmount: number;
	paidFromRiskAmount: number;
	paidFromThreshold: number;
	paidFromSavings: number;
	recoveryAmount: number;
	totalAmountPaid: number;
	tariff: number;
	coPayAmount: number;
	payTo: string;
	rej: string;
	rev: string;
	authNo: string;
	dl: string;
	claimLineNo: string;
	duplicateClaim: string;
	duplicateClaimLine: string;
	paperOrEdi: string;
	assessorName: string;
	claimTypeCode: string;
	patientBirthDate: string;
	patientCurrentAge: number;
	agentReview?: AgentReviewResponse;
	humanReview?: HumanReviewResponse;
	reviewedBy?: ReviewerInfo;
	source: ClaimSource;
	claimMarkdown?: string;
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
	claimNumber: string;
	claimLineNo: string;
	patientFirstName: string;
	patientSurname: string;
	memberNumber: string;
	providerName: string;
	practiceNumber: string;
	totalClaimAmount: number;
	status: ClaimStatus;
	ingestedAt: string;
	updatedAt: string;
}

// Excel Upload Response
export interface UploadExcelResponse {
	totalRows: number;
	successfullyQueued: number;
	duplicates: number;
	failed: number;
	errors: string[];
}

export interface SubmitHumanReviewRequest {
	decision: ClaimDecision;
	comments: string;
}

export interface SubmitHumanReviewResponse {
	id: string;
	reviewedAt: string;
	decision: ClaimDecision;
	comments: string;
}

export enum AuditActorType {
	User,
	System,
	AiAgent,
}

export interface AuditLogResponse {
	id: string;
	timestamp: string;
	action: string;
	actorType: AuditActorType;
	actorName?: string;
	description: string;
	metadata?: Record<string, unknown>;
}

export interface GetClaimAuditLogsResponse {
	auditLogs: AuditLogResponse[];
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
