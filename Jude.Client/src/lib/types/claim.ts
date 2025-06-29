export enum ClaimStatus {
  Pending = "Pending",
  Processing = "Processing",
  PendingReview = "PendingReview",
  Approved = "Approved",
  Rejected = "Rejected",
  RequestMoreInfo = "RequestMoreInfo",
  Resubmitted = "Resubmitted",
  Completed = "Completed",
}

export enum ClaimSource {
  CIMAS = "CIMAS",
  Manual = "Manual",
}

export enum ClaimDecision {
  Approve = "Approve",
  Reject = "Reject",
  RequestMoreInfo = "RequestMoreInfo",
  Escalate = "Escalate",
}

export enum FraudRiskLevel {
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical",
}

export interface Claim {
  id: string;
  ingestedAt: string;
  updatedAt: string;
  status: ClaimStatus;
  source: ClaimSource;
  submittedAt?: string;
  processedAt?: string;
  agentRecommendation?: string;
  agentReasoning?: string;
  agentConfidenceScore?: number;
  agentProcessedAt?: string;
  isFlagged: boolean;
  fraudIndicators?: string[];
  fraudRiskLevel: FraudRiskLevel;
  requiresHumanReview: boolean;
  finalDecision?: ClaimDecision;
  reviewerComments?: string;
  rejectionReason?: string;
  reviewedAt?: string;
  reviewedById?: string;
  user?: {
    id: string;
    name: string;
  };
}
