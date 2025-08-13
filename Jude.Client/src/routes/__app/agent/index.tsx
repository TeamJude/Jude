import { testAgent, extractClaim } from "@/lib/services/agent.service";
import { useMutation } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  Spinner,
  Badge,
  Divider,
  Progress,
} from "@heroui/react";
import { DynamicIcon } from "lucide-react/dynamic";
import { Markdown } from "@/components/ai/markdown";

export const Route = createFileRoute("/__app/agent/")({
	component: AgentTest,
});

function AgentTest() {
	const [selectedFile, setSelectedFile] = useState<File | null>(null);
	const [filePreviewUrl, setFilePreviewUrl] = useState<string | null>(null);
	const [extractedMarkdown, setExtractedMarkdown] = useState<string>("");

	const {
		mutate: startExtraction,
		isPending: isExtracting,
		error: extractError,
		reset: resetExtract,
	} = useMutation({
		mutationFn: async (file: File) => {
			const resp = await extractClaim(file);
			if (resp.success) return resp.data.content;
			throw new Error(resp.errors?.[0] || "Extraction failed");
		},
		onSuccess: (markdown) => {
			setExtractedMarkdown(markdown);
		},
	});

	const {
		mutate: reviewExtracted,
		data: reviewResult,
		isPending: isReviewing,
		error: reviewError,
		reset: resetReview,
	} = useMutation({
		mutationFn: async (markdown: string) => {
			const resp = await testAgent(markdown);
			if (resp.success) return resp.data;
			throw new Error(resp.errors?.[0] || "Review failed");
		},
	});

	const handleClear = () => {
		if (filePreviewUrl) URL.revokeObjectURL(filePreviewUrl);
		setSelectedFile(null);
		setFilePreviewUrl(null);
		setExtractedMarkdown("");
		resetExtract();
		resetReview();
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

	const onFileDrop = (fileList: FileList | null) => {
		const file = fileList?.[0];
		if (!file) return;
		if (file.type !== "application/pdf") {
			alert("Please upload a PDF file");
			return;
		}
		if (filePreviewUrl) URL.revokeObjectURL(filePreviewUrl);
		setSelectedFile(file);
		setFilePreviewUrl(URL.createObjectURL(file));
		setExtractedMarkdown("");
		resetExtract();
		resetReview();
	};

	const canExtract = !!selectedFile && !isExtracting;
	const canReview = !!extractedMarkdown && !isReviewing;

	return (
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				<div className="flex justify-between items-center">
					<div>
						<h1 className="text-2xl font-semibold text-gray-800">Agent Playground</h1>
						<p className="text-sm text-gray-500">Upload a PDF, extract claim data, and run AI review</p>
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

				<div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-[calc(100vh-140px)]">
					{/* Panel 1 - PDF Upload & Preview */}
					<Card className="shadow-none border-zinc-200 border-1">
						<CardHeader className="pb-2">
							<div>
								<h3 className="text-base font-medium">Upload PDF</h3>
								<p className="text-xs text-gray-500">Drop a claim PDF here or click to select a file</p>
							</div>
						</CardHeader>
						<CardBody className="space-y-4">
							<div
								onDragOver={(e) => e.preventDefault()}
								onDrop={(e) => {
									e.preventDefault();
									onFileDrop(e.dataTransfer.files);
								}}
								className="border-2 border-dashed border-zinc-300 rounded-lg p-4 flex flex-col items-center justify-center gap-3 cursor-pointer hover:bg-zinc-50"
								onClick={() => {
									const input = document.getElementById("file-input-hidden") as HTMLInputElement | null;
									input?.click();
								}}
							>
								<input
									id="file-input-hidden"
									type="file"
									accept="application/pdf"
									className="hidden"
									onChange={(e) => onFileDrop(e.target.files)}
								/>
								<DynamicIcon name="upload" size={20} className="text-zinc-500" />
								<div className="text-sm text-zinc-600">
									{selectedFile ? selectedFile.name : "Click to select or drop a PDF here"}
								</div>
							</div>

							<div className="h-[480px] border rounded-lg overflow-hidden bg-zinc-50">
								{!filePreviewUrl ? (
									<div className="h-full flex items-center justify-center text-sm text-zinc-400">No PDF selected</div>
								) : (
									<iframe src={filePreviewUrl} className="w-full h-full" title="PDF Preview" />
								)}
							</div>

							<Button
								color="primary"
								size="lg"
								onPress={() => selectedFile && startExtraction(selectedFile)}
								isDisabled={!canExtract}
								startContent={isExtracting ? <Spinner size="sm" /> : <DynamicIcon name="sparkles" size={16} />}
								className="w-full"
							>
								{isExtracting ? "Extracting..." : "Process (Extract Data)"}
							</Button>
							{extractError && <div className="text-danger text-xs">{(extractError as Error).message}</div>}
						</CardBody>
					</Card>

					{/* Panel 2 - Extracted Data */}
					<Card className="shadow-none border-zinc-200 border-1">
						<CardHeader className="pb-2">
							<div>
								<h3 className="text-base font-medium">Extracted Data</h3>
								<p className="text-xs text-gray-500">AI extraction result from the uploaded PDF</p>
							</div>
						</CardHeader>
						<CardBody>
							{isExtracting && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<Spinner size="lg" label="Extracting data..." />
									<p className="text-sm text-gray-500">AI is extracting data from your PDF...</p>
								</div>
							)}

							{!isExtracting && !extractedMarkdown && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<DynamicIcon name="file-text" className="w-12 h-12 text-gray-400" />
									<p className="text-gray-500 font-medium">No Extracted Data</p>
									<p className="text-sm text-gray-400 text-center">Upload and process a PDF to view extracted claim data</p>
								</div>
							)}

							{!!extractedMarkdown && (
								<div className="space-y-4">
									<div className="bg-gray-50 rounded-lg p-3">
										<div className="text-sm text-gray-700">
											<Markdown>{extractedMarkdown}</Markdown>
										</div>
									</div>
									<Button
										color="secondary"
										onPress={() => reviewExtracted(extractedMarkdown)}
										isDisabled={!canReview}
										startContent={isReviewing ? <Spinner size="sm" /> : <DynamicIcon name="bot" size={16} />}
									>
										{isReviewing ? "Reviewing..." : "Review with AI"}
									</Button>
									{reviewError && <div className="text-danger text-xs">{(reviewError as Error).message}</div>}
								</div>
							)}
						</CardBody>
					</Card>

					{/* Panel 3 - Review Results */}
					<Card className="shadow-none border-zinc-200 border-1">
						<CardHeader className="pb-2">
							<div>
								<h3 className="text-base font-medium">Review Result</h3>
								<p className="text-xs text-gray-500">AI agent decision and rationale</p>
							</div>
						</CardHeader>
						<CardBody>
							{isReviewing && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<Spinner size="lg" label="Running review..." />
									<p className="text-sm text-gray-500">The AI agent is analyzing the extracted data...</p>
								</div>
							)}

							{!isReviewing && !reviewResult && (
								<div className="flex flex-col items-center justify-center h-64 space-y-4">
									<DynamicIcon name="bot" className="w-12 h-12 text-gray-400" />
									<p className="text-gray-500 font-medium">No Review Yet</p>
									<p className="text-sm text-gray-400 text-center">Run a review after extraction to see the AI decision</p>
								</div>
							)}

							{!!reviewResult && (
								<div className="space-y-6">
									<div className="space-y-3">
										<div className="flex items-center justify-between">
											<h4 className="text-sm font-medium text-gray-700">Decision</h4>
											<Badge color={getDecisionColor(reviewResult.decision)} variant="flat" size="sm">
												{getDecisionText(reviewResult.decision)}
											</Badge>
										</div>
										<div className="space-y-2">
											<div className="flex items-center justify-between">
												<span className="text-sm text-gray-600">Confidence Score</span>
												<span className="text-sm font-medium">{(reviewResult.confidenceScore * 100).toFixed(1)}%</span>
											</div>
											<Progress value={reviewResult.confidenceScore * 100} color={getConfidenceColor(reviewResult.confidenceScore)} className="w-full" />
										</div>
									</div>

									<Divider />

									<div className="space-y-2">
										<h4 className="text-sm font-medium text-gray-700">Recommendation</h4>
										<div className="bg-gray-50 rounded-lg p-3">
											<div className="text-sm text-gray-700">
												<Markdown>{reviewResult.recommendation}</Markdown>
											</div>
										</div>
									</div>

									<Divider />

									<div className="space-y-2">
										<h4 className="text-sm font-medium text-gray-700">Detailed Reasoning</h4>
										<div className="bg-gray-50 rounded-lg p-3">
											<div className="text-sm text-gray-700">
												<Markdown>{reviewResult.reasoning}</Markdown>
											</div>
										</div>
									</div>

									<Divider />

									<div className="space-y-2">
										<h4 className="text-sm font-medium text-gray-700">Processing Details</h4>
										<div className="grid grid-cols-2 gap-4 text-sm">
											<div>
												<span className="text-gray-500">Review ID:</span>
												<p className="font-mono text-xs">{reviewResult.id}</p>
											</div>
											<div>
												<span className="text-gray-500">Reviewed At:</span>
												<p className="text-xs">{new Date(reviewResult.reviewedAt).toLocaleString()}</p>
											</div>
										</div>
										<div className="mt-3 p-2 bg-blue-50 rounded border border-blue-200">
											<div className="flex items-center gap-2">
												<DynamicIcon name="file-text" size={14} className="text-blue-600" />
												<span className="text-xs font-medium text-blue-800">Policy Document Analyzed</span>
											</div>
											<p className="text-xs text-blue-700 mt-1">The AI agent analyzed the uploaded policy document to inform this decision.</p>
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


