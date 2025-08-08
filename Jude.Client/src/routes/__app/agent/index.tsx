import { testAgent, type AgentReviewResult } from "@/lib/services/agent.service";
import { useMutation } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { 
	Button, 
	Card, 
	CardBody, 
	CardHeader, 
	Textarea, 
	Spinner, 
	Badge,
	Divider,
	Progress
} from "@heroui/react";
import { DynamicIcon } from "lucide-react/dynamic";
import { AlertCircle } from "lucide-react";
import { Markdown } from "@/components/ai/markdown";

export const Route = createFileRoute("/__app/agent/")({
	component: AgentTest,
});

function AgentTest() {
	const [claimData, setClaimData] = useState<string>("");
	const [context, setContext] = useState<string>("");

	const {
		mutate: processClaim,
		data: result,
		isPending,
		error,
		reset
	} = useMutation({
		mutationFn: testAgent,
		onSuccess: () => {
			// Success handled by displaying result
		},
		onError: () => {
			// Error handled by displaying error state
		}
	});

	const handleProcessClaim = () => {
		if (!claimData.trim()) return;
		processClaim(claimData);
	};

	const handleClear = () => {
		setClaimData("");
		setContext("");
		reset();
	};

	const getDecisionColor = (decision: number) => {
		return decision === 1 ? "success" : "danger";
	};

	const getDecisionText = (decision: number) => {
		return decision === 1 ? "Approve" : "Reject";
	};

	const getConfidenceColor = (score: number) => {
		if (score >= 0.8) return "success";
		if (score >= 0.6) return "warning";
		return "danger";
	};

	return (
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				<div className="flex justify-between items-center">
					<div>
						<h1 className="text-2xl font-semibold text-gray-800">
							Agent Playground
						</h1>
						<p className="text-sm text-gray-500">
							Test the AI agent with claim data and view processing results
						</p>
					</div>
					<div className="flex items-center gap-3">
						<Button
							color="default"
							variant="flat"
							startContent={<DynamicIcon name="refresh-cw" size={16} />}
							onPress={handleClear}
						>
							Clear
						</Button>
					</div>
				</div>

				<div className="grid grid-cols-1 lg:grid-cols-2 gap-6 h-[calc(100vh-140px)]">
					{/* Left Panel - Input */}
					<Card className="shadow-none border-zinc-200 border-1">
						<CardHeader className="pb-2">
							<div>
								<h3 className="text-base font-medium">Claim Data Input</h3>
								<p className="text-xs text-gray-500">
									Paste your claim data in JSON format for processing
								</p>
							</div>
						</CardHeader>
						<CardBody className="space-y-4">
							<div>

								<Textarea
									placeholder="Paste your claim data here in JSON format..."
									value={claimData}
									onValueChange={setClaimData}
									minRows={18}
									maxRows={25}
									className="w-full"
									classNames={{
										input: "resize-none",
									}}
								/>
							</div>
							

							<Button
								color="primary"
								size="lg"
								onPress={handleProcessClaim}
								isDisabled={!claimData.trim() || isPending}
								startContent={
									isPending ? (
										<Spinner size="sm" />
									) : (
										<DynamicIcon name="play" size={16} />
									)
								}
								className="w-full"
							>
								{isPending ? "Processing..." : "Process Claim"}
							</Button>
						</CardBody>
					</Card>

					{/* Right Panel - Results */}
					<Card className="shadow-none border-zinc-200 border-1">
						<CardHeader className="pb-2">
							<div>
								<h3 className="text-base font-medium">Processing Results</h3>
								<p className="text-xs text-gray-500">
									AI agent analysis and decision output
								</p>
							</div>
						</CardHeader>
						<CardBody>
							{isPending && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<Spinner size="lg" label="Processing claim..." />
									<p className="text-sm text-gray-500">
										The AI agent is analyzing your claim data...
									</p>
								</div>
							)}

							{error && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<AlertCircle className="w-12 h-12 text-danger" />
									<p className="text-danger font-medium">Processing Failed</p>
									<p className="text-sm text-gray-500 text-center">
										{error.message || "An error occurred while processing the claim"}
									</p>
									<Button
										color="primary"
										variant="flat"
										onPress={() => reset()}
									>
										Try Again
									</Button>
								</div>
							)}

							{!isPending && !error && (!result?.success || !result?.data) && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<DynamicIcon name="bot" className="w-12 h-12 text-gray-400" />
									<p className="text-gray-500 font-medium">No Results</p>
									<p className="text-sm text-gray-400 text-center">
										Process a claim to see the AI agent's analysis
									</p>
								</div>
							)}

							{!isPending && !error && result?.success && result.data && (
								<div className="space-y-6">
									{/* Decision Summary */}
									<div className="space-y-3">
										<div className="flex items-center justify-between">
											<h4 className="text-sm font-medium text-gray-700">Decision</h4>
											<Badge
												color={getDecisionColor(result.data.decision)}
												variant="flat"
												size="sm"
											>
												{getDecisionText(result.data.decision)}
											</Badge>
										</div>
										
										<div className="space-y-2">
											<div className="flex items-center justify-between">
												<span className="text-sm text-gray-600">Confidence Score</span>
												<span className="text-sm font-medium">
													{(result.data.confidenceScore * 100).toFixed(1)}%
												</span>
											</div>
											<Progress
												value={result.data.confidenceScore * 100}
												color={getConfidenceColor(result.data.confidenceScore)}
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
												<Markdown>{result.data.recommendation}</Markdown>
											</div>
										</div>
									</div>

									<Divider />

									{/* Reasoning */}
									<div className="space-y-2">
										<h4 className="text-sm font-medium text-gray-700">Detailed Reasoning</h4>
										<div className="bg-gray-50 rounded-lg p-3">
											<div className="text-sm text-gray-700">
												<Markdown>{result.data.reasoning}</Markdown>
											</div>
										</div>
									</div>

									<Divider />

									{/* Metadata */}
									<div className="space-y-2">
										<h4 className="text-sm font-medium text-gray-700">Processing Details</h4>
										<div className="grid grid-cols-2 gap-4 text-sm">
											<div>
												<span className="text-gray-500">Review ID:</span>
												<p className="font-mono text-xs">{result.data.id}</p>
											</div>
											<div>
												<span className="text-gray-500">Reviewed At:</span>
												<p className="text-xs">
													{new Date(result.data.reviewedAt).toLocaleString()}
												</p>
											</div>
										</div>
										
										<div className="mt-3 p-2 bg-blue-50 rounded border border-blue-200">
											<div className="flex items-center gap-2">
												<DynamicIcon name="file-text" size={14} className="text-blue-600" />
												<span className="text-xs font-medium text-blue-800">Policy Document Analyzed</span>
											</div>
											<p className="text-xs text-blue-700 mt-1">
												The AI agent analyzed the uploaded policy document to inform this decision.
											</p>
										</div>
									</div>
								</div>
							)}
						</CardBody>
					</Card>
				</div>
			</div>
		</main>
	);
}


