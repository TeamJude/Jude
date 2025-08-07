import type { Citation, GetClaimDetailResponse } from "@/lib/types/claim";
import {
	Card,
	CardBody,
	Chip,
} from "@heroui/react";
import { BookOpen } from "lucide-react";
import React from "react";

interface ClaimPolicyTabProps {
	claim: GetClaimDetailResponse;
	citations?: Citation[];
}

export const ClaimPolicyTab: React.FC<ClaimPolicyTabProps> = ({ claim, citations = [] }) => {
	return (
		<Card shadow="none">
			<CardBody>
				<h3 className="text-lg font-medium mb-4">Relevant Policies</h3>

				<div className="space-y-4">
					{citations && citations.length > 0 ? (
						citations.map((citation: Citation, index: number) => (
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
	);
};
