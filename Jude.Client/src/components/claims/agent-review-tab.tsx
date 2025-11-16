import type { GetClaimDetailResponse } from "@/lib/types/claim";
import { Badge, Card, CardBody, Divider, Progress } from "@heroui/react";
import { Markdown } from "@/components/ai/markdown";
import { CheckCircle, XCircle } from "lucide-react";

interface AgentReviewTabProps {
	claim: GetClaimDetailResponse;
}

export const AgentReviewTab: React.FC<AgentReviewTabProps> = ({ claim }) => {
	// Use placeholder data when no agent review exists
	const placeholderReview = {
		id: "demo-review-id",
		reviewedAt: new Date().toISOString(),
		decisionStatus: 2, // REJECT
		recommendation:
			"Immediate rejection with optional manual review for fraud investigation",
		reasoning: `### **Claim Adjudication Breakdown**

#### **1. Invalid Patient Information**  
- **Rule Violated**:  
  - **Claim Validation Failure** (*ID: c3d4e5f6-a7b8-9012-3456-7890abcdef12*)  
    > _"Claims with obviously invalid patient information should be auto-rejected."_  
- **Issue**:  
  - Patient name **"VIDEOTAPE POMEGRANATE EAST"** is nonsensical and indicative of potential fraud.  
  - No initials provided despite being a required field for CIMAS claims.  

#### **2. Dental Procedure Requirements Missing**  
- **Rules Violated**:  
  - **ICD-10 Code Mandate** (*ID: b8c9d0e1-f2a3-4567-8901-cdef12345678*)  
    > _"ICD-10 codes are mandatory for dental disciplines."_  
  - **Tooth Number Check** (*ID: e9f0a1b2-c3d4-5678-9012-345678901234*)  
    > _"Dental procedures require documented tooth numbers for validation."_  
- **Issue**:  
  - Tariff **98101** (dental examination) submitted without:  
    - ICD-10 diagnosis code (e.g., K02.9 for dental caries).  
    - Tooth number or quadrant specification.  

#### **3. Invalid Currency (ZWG)**  
- **Rule Violated**:  
  - **Foreign Treatment Payment Rules** (*ID: b6c7d8e9-f0a1-b2c3-d4e5-f6a7b8c9d0e1*)  
    > _"Claims must be submitted in USD. Non-USD claims require manual adjudication."_  
- **Issue**:  
  - Currency **ZWG** (Zimbabwean Gold) is not accepted for CIMAS claims.  

#### **4. Service-Specific Validation Failure**  
- **Rule Violated**:  
  - **Tariff/Nappi/CPT Code Validation** (*ID: a7b8c9d0-e1f2-3456-7890-bcdef1234567*)  
    > _"Tariff codes must match the provider's discipline and claim type."_  
- **Issue**:  
  - No provider discipline documented to validate if tariff **98101** is applicable.  

### **Conclusion**  
This claim exhibits **multiple critical violations** of CIMAS adjudication rules, including fraudulent patient data, incomplete dental documentation, and non-compliant currency. **Immediate rejection** is recommended, with an optional fraud investigation flag.`,
		confidenceScore: 0.99,
	};

	const review = claim.agentReview || placeholderReview;

	const getDecisionText = (decision: number | undefined) => {
		if (decision === 1) return "Approve";
		if (decision === 2) return "Reject";
		return "Pending";
	};

	const getDecisionColor = (decision: number | undefined) => {
		if (decision === 1) return "success" as const;
		if (decision === 2) return "danger" as const;
		return "default" as const;
	};

	const getDecisionIcon = (decision: number | undefined) => {
		if (decision === 1) return <CheckCircle className="w-6 h-6 text-success" />;
		if (decision === 2) return <XCircle className="w-6 h-6 text-danger" />;
		return null;
	};

	const confidence = review?.confidenceScore ?? 0;

	return (
		<Card shadow="none">
			<CardBody className="space-y-6">
				{/* Decision Summary */}
				<div className="space-y-3">
					<div className="flex items-center justify-between">
						<div className="flex items-center gap-2">
							{getDecisionIcon(review?.decisionStatus as unknown as number)}
							<h4 className="text-sm font-medium text-gray-700">Decision</h4>
						</div>
						<Badge
							color={getDecisionColor(
								review?.decisionStatus as unknown as number,
							)}
							variant="flat"
							size="sm"
						>
							{getDecisionText(review?.decisionStatus as unknown as number)}
						</Badge>
					</div>

					<div className="space-y-2">
						<div className="flex items-center justify-between">
							<span className="text-sm text-gray-600">Confidence Score</span>
							<span className="text-sm font-medium">
								{Math.round(confidence * 100)}%
							</span>
						</div>
						<Progress
							value={confidence * 100}
							color="primary"
							className="w-full"
						/>
					</div>
				</div>

				<Divider />

				{/* Recommendation */}
				<div className="space-y-2">
					<h4 className="text-sm font-medium text-gray-700">Recommendation</h4>
					<div className="bg-gray-50 rounded-lg p-3">
						<div className="text-sm text-gray-700">
							<Markdown>
								{review?.recommendation || "Pending Agent Processing"}
							</Markdown>
						</div>
					</div>
				</div>

				<Divider />

				{/* Reasoning */}
				<div className="space-y-2">
					<h4 className="text-sm font-medium text-gray-700">
						Detailed Reasoning
					</h4>
					<div className="bg-gray-50 rounded-lg p-3">
						<div className="text-sm text-gray-700">
							<Markdown>
								{review?.reasoning ||
									"The AI agent is analyzing policy compliance, coverage rules, and fraud indicators to generate a decision."}
							</Markdown>
						</div>
					</div>
				</div>

				<Divider />

				{/* Metadata */}
				<div className="space-y-2">
					<h4 className="text-sm font-medium text-gray-700">
						Processing Details
					</h4>
					<div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
						<div>
							<span className="text-gray-500">Review ID:</span>
							<p className="font-mono text-xs">{review?.id || "—"}</p>
						</div>
						<div>
							<span className="text-gray-500">Reviewed At:</span>
							<p className="text-xs">
								{review?.reviewedAt
									? new Date(review.reviewedAt).toLocaleString()
									: "—"}
							</p>
						</div>
					</div>
				</div>
			</CardBody>
		</Card>
	);
};
