import React, { useState, useEffect } from "react";
import { ClaimsTable } from "@/components/claims/claims-table";
import { KanbanBoard } from "@/components/claims/kanban-board";
import { kanbanClaims, getClaimsStats } from "@/components/claims/claims-data";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { claimsService, type Claim } from "@/lib/services/claims.service";
import {
	Button,
	ButtonGroup,
	Select,
	SelectItem,
	Card,
	Chip,
	Spinner,
} from "@heroui/react";
import {
	Table,
	Kanban,
	Calendar,
	TrendingUp,
	AlertTriangle,
	Flag,
	Play,
	RefreshCw,
} from "lucide-react";

export const Route = createFileRoute("/__app/claims/")({
	component: RouteComponent,
});

type ViewMode = "table" | "kanban";
type TimeFilter = "today" | "yesterday" | "week" | "month" | "custom";

// Transform backend claim to frontend format
const transformClaim = (backendClaim: Claim) => {
	const mapStatus = (status: string) => {
		switch (status) {
			case "Pending":
				return "inQueue" as const;
			case "Processing":
				return "inProgress" as const;
			case "PendingReview":
				return "awaitingReview" as const;
			default:
				return "inQueue" as const;
		}
	};

	const mapRiskLevel = (level: string) => {
		switch (level.toLowerCase()) {
			case "low":
				return "LOW" as const;
			case "medium":
				return "MEDIUM" as const;
			case "high":
				return "HIGH" as const;
			case "critical":
				return "CRITICAL" as const;
			default:
				return "LOW" as const;
		}
	};

	const timeAgo = () => {
		const now = new Date();
		const ingested = new Date(backendClaim.ingestedAt);
		const diffMs = now.getTime() - ingested.getTime();
		const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
		const diffMinutes = Math.floor(diffMs / (1000 * 60));

		if (diffHours > 0) {
			return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
		} else if (diffMinutes > 0) {
			return `${diffMinutes} minute${diffMinutes > 1 ? 's' : ''} ago`;
		} else {
			return 'Just now';
		}
	};

	return {
		id: backendClaim.id,
		transactionNumber: backendClaim.transactionNumber,
		patientName: backendClaim.patientName,
		membershipNumber: backendClaim.membershipNumber,
		claimAmount: backendClaim.claimAmount,
		currency: backendClaim.currency,
		approvedAmount: backendClaim.approvedAmount,
		providerPractice: backendClaim.providerPractice,
		timeAgo: timeAgo(),
		itemSummary: "Processing...", // We don't have detailed items from backend yet
		status: mapStatus(backendClaim.status),
		fraudRiskLevel: mapRiskLevel(backendClaim.fraudRiskLevel),
		isFlagged: backendClaim.isFlagged,
		source: backendClaim.source,
		ingestedAt: backendClaim.ingestedAt,
		processingStartedAt: backendClaim.processedAt,
		lastUpdatedAt: backendClaim.updatedAt,
		agentProgress: backendClaim.agentConfidenceScore ? Math.round(backendClaim.agentConfidenceScore * 100) : undefined,
		agentReasoning: backendClaim.agentReasoning ? backendClaim.agentReasoning.split('\n').filter(line => line.trim()) : undefined,
		agentRecommendation: backendClaim.agentRecommendation,
		patientDetails: {
			dateOfBirth: "1990-01-01", // Mock data - would come from member lookup
			gender: "M",
			idNumber: "ID123456789"
		},
		providerDetails: {
			name: "Medical Provider",
			contactInfo: "provider@example.com"
		},
		items: [] // Would be populated from claim details
	};
};

function RouteComponent() {
	const navigate = useNavigate();
	const [viewMode, setViewMode] = useState<ViewMode>("kanban"); // Default to kanban for demo
	const [timeFilter, setTimeFilter] = useState<TimeFilter>("today");
	const [claims, setClaims] = useState<any[]>([]);
	const [isLoading, setIsLoading] = useState(false);
	const [isProcessing, setIsProcessing] = useState(false);
	const [processingStage, setProcessingStage] = useState<string>("");
	const [useRealData, setUseRealData] = useState(false);

	const stats = useRealData ? getClaimsStats() : getClaimsStats();

	const timeFilterOptions = [
		{ key: "today", label: "Today" },
		{ key: "yesterday", label: "Yesterday" },
		{ key: "week", label: "Last 7 days" },
		{ key: "month", label: "Last 30 days" },
		{ key: "custom", label: "Custom range" },
	];

	// Load real claims data
	const loadRealClaims = async () => {
		setIsLoading(true);
		try {
			const result = await claimsService.getInternalClaims(10);
			if (result.success) {
				const transformedClaims = result.data.map(transformClaim);
				setClaims(transformedClaims);
				setUseRealData(true);
			} else {
				console.error('Failed to load claims:', result.errors);
				// Fallback to dummy data
				setClaims(kanbanClaims);
				setUseRealData(false);
			}
		} catch (error) {
			console.error('Error loading claims:', error);
			setClaims(kanbanClaims);
			setUseRealData(false);
		} finally {
			setIsLoading(false);
		}
	};

	// Process claims with simulation for demo
	const processRecentClaims = async () => {
		setIsProcessing(true);
		try {
			// Step 1: Load recent claims
			setProcessingStage("Loading recent claims...");
			await loadRealClaims();
			await new Promise(resolve => setTimeout(resolve, 1000));

			// Step 2: Get 3 most recent pending claims
			setProcessingStage("Selecting claims for processing...");
			const pendingClaims = claims.filter(c => c.status === 'inQueue').slice(0, 3);
			
			// Trigger actual CIMAS ingestion and processing
			setProcessingStage("Fetching claims from CIMAS...");
			const result = await claimsService.ingestAndProcessClaims("12345", 3);
			
			if (result.success) {
				console.log('Ingestion and processing result:', result.data);
				
				// Update UI with ingested claims first
				setProcessingStage("Claims ingested, starting processing...");
				const newClaims = result.data.ingestedClaims.map(claim => transformClaimFromBackend(claim));
				setClaims(prev => [...newClaims, ...prev]);
				setUseRealData(true);
				
				// Simulate the processing flow for the new claims
				await simulateProcessingFlowForClaims(newClaims);
			} else {
				console.error('Failed to ingest and process claims:', result.errors);
				// Fallback to simulation with dummy data
				await simulateProcessingFlow();
			}

		} catch (error) {
			console.error('Error processing claims:', error);
		} finally {
			setIsProcessing(false);
			setProcessingStage("");
		}
	};

	// Simulate processing flow for demo
	const simulateProcessingFlow = async () => {
		// Take first 3 claims and move them through the pipeline
		const claimsToProcess = [...(useRealData ? claims : kanbanClaims)].slice(0, 3);
		
		for (let i = 0; i < claimsToProcess.length; i++) {
			const claim = claimsToProcess[i];
			
			// Move to in-progress
			setProcessingStage(`Processing ${claim.patientName}...`);
			setClaims(prev => prev.map(c => 
				c.id === claim.id 
					? { ...c, status: 'inProgress', agentProgress: 0, processingStartedAt: new Date().toISOString() }
					: c
			));
			
			// Simulate progress updates
			for (let progress = 25; progress <= 100; progress += 25) {
				await new Promise(resolve => setTimeout(resolve, 800));
				setClaims(prev => prev.map(c => 
					c.id === claim.id 
						? { 
							...c, 
							agentProgress: progress,
							agentReasoning: [
								'Validating medical codes...',
								'Cross-referencing eligibility...',
								'Analyzing provider patterns...',
								'Calculating fraud risk score...',
								'Generating recommendation...'
							].slice(0, Math.floor(progress / 25))
						}
						: c
				));
			}
			
			// Move to awaiting review
			await new Promise(resolve => setTimeout(resolve, 500));
			setClaims(prev => prev.map(c => 
				c.id === claim.id 
					? { 
						...c, 
						status: 'awaitingReview',
						agentRecommendation: Math.random() > 0.5 ? 'Approve' : 'Review Required',
						agentProgress: 100
					}
					: c
			));
		}
		
		setProcessingStage("Processing complete!");
		await new Promise(resolve => setTimeout(resolve, 1000));
	};

	// Transform backend claim format to frontend format (simpler version)
	const transformClaimFromBackend = (backendClaim: any) => {
		return {
			id: backendClaim.claimId,
			transactionNumber: backendClaim.transactionNumber,
			patientName: backendClaim.patientName,
			membershipNumber: "N/A", // Not in response
			claimAmount: backendClaim.claimAmount,
			currency: "$",
			approvedAmount: undefined,
			providerPractice: "Demo Practice",
			timeAgo: "Just now",
			itemSummary: "Processing...",
			status: "inQueue" as const,
			fraudRiskLevel: "MEDIUM" as const,
			isFlagged: false,
			source: "CIMAS",
			ingestedAt: new Date().toISOString(),
			processingStartedAt: undefined,
			lastUpdatedAt: new Date().toISOString(),
			patientDetails: {
				dateOfBirth: "1990-01-01",
				gender: "M",
				idNumber: "ID123456789"
			},
			providerDetails: {
				name: "Demo Practice",
				contactInfo: "demo@practice.com"
			},
			items: []
		};
	};

	// Simulate processing flow for specific claims
	const simulateProcessingFlowForClaims = async (claimsToProcess: any[]) => {
		for (let i = 0; i < claimsToProcess.length; i++) {
			const claim = claimsToProcess[i];
			
			// Move to in-progress
			setProcessingStage(`Processing ${claim.patientName}...`);
			setClaims(prev => prev.map(c => 
				c.id === claim.id 
					? { ...c, status: 'inProgress', agentProgress: 0, processingStartedAt: new Date().toISOString() }
					: c
			));
			
			// Simulate progress updates
			for (let progress = 25; progress <= 100; progress += 25) {
				await new Promise(resolve => setTimeout(resolve, 800));
				setClaims(prev => prev.map(c => 
					c.id === claim.id 
						? { 
							...c, 
							agentProgress: progress,
							agentReasoning: [
								'Validating medical codes...',
								'Cross-referencing eligibility...',
								'Analyzing provider patterns...',
								'Calculating fraud risk score...',
								'Generating recommendation...'
							].slice(0, Math.floor(progress / 25))
						}
						: c
				));
			}
			
			// Move to awaiting review
			await new Promise(resolve => setTimeout(resolve, 500));
			setClaims(prev => prev.map(c => 
				c.id === claim.id 
					? { 
						...c, 
						status: 'awaitingReview',
						agentRecommendation: Math.random() > 0.5 ? 'Approve' : 'Review Required',
						agentProgress: 100
					}
					: c
			));
		}
		
		setProcessingStage("Processing complete!");
		await new Promise(resolve => setTimeout(resolve, 1000));
	};

	// Load claims on component mount
	useEffect(() => {
		setClaims(kanbanClaims); // Start with dummy data
	}, []);

	const handleClaimClick = (claim: any) => {
		console.log('Claim clicked:', claim);
	};

	const handleReview = (claimId: string) => {
		navigate({ to: `/claims/${claimId}` });
	};

	const renderViewToggle = () => (
		<ButtonGroup>
			<Button
				variant={viewMode === "table" ? "solid" : "bordered"}
				color={viewMode === "table" ? "primary" : "default"}
				startContent={<Table className="w-4 h-4" />}
				onPress={() => setViewMode("table")}
			>
				Table
			</Button>
			<Button
				variant={viewMode === "kanban" ? "solid" : "bordered"}
				color={viewMode === "kanban" ? "primary" : "default"}
				startContent={<Kanban className="w-4 h-4" />}
				onPress={() => setViewMode("kanban")}
			>
				Kanban
			</Button>
		</ButtonGroup>
	);

	const renderStatsCards = () => (
		<div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
			<Card className="p-4 shadow-sm border-zinc-200 border-1">
				<div className="flex items-center gap-2">
					<TrendingUp className="w-5 h-5 text-blue-500" />
					<div>
						<p className="text-sm text-gray-500">Total Claims</p>
						<p className="text-xl font-semibold">{stats.total}</p>
					</div>
				</div>
			</Card>
			<Card className="p-4 shadow-sm border-zinc-200 border-1">
				<div className="flex items-center gap-2">
					<Calendar className="w-5 h-5 text-green-500" />
					<div>
						<p className="text-sm text-gray-500">Total Value</p>
						<p className="text-xl font-semibold">
							${stats.totalAmount.toLocaleString()}
						</p>
					</div>
				</div>
			</Card>
			<Card className="p-4 shadow-sm border-zinc-200 border-1">
				<div className="flex items-center gap-2">
					<AlertTriangle className="w-5 h-5 text-orange-500" />
					<div>
						<p className="text-sm text-gray-500">High Risk</p>
						<p className="text-xl font-semibold">{stats.highRiskCount}</p>
					</div>
				</div>
			</Card>
			<Card className="p-4 shadow-sm border-zinc-200 border-1">
				<div className="flex items-center gap-2">
					<Flag className="w-5 h-5 text-red-500" />
					<div>
						<p className="text-sm text-gray-500">Flagged</p>
						<p className="text-xl font-semibold">{stats.flaggedCount}</p>
					</div>
				</div>
			</Card>
		</div>
	);
	return (
		<main className="h-full py-6 px-4">
			<div className="max-w-7xl mx-auto px-4 space-y-6">
				{/* Header with Title and Actions */}
				<div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
					<div>
						<h1 className="text-2xl font-semibold text-gray-800">Claims</h1>
						<p className="text-sm text-gray-500">
							{viewMode === "kanban"
								? "Real-time kanban view of claims processing pipeline"
								: "Real-time overview of claims processing"}
						</p>
					</div>

					<div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
						{/* Ingest & Process Claims Button */}
						<Button
							color="primary"
							variant="solid"
							startContent={isProcessing ? <Spinner size="sm" color="white" /> : <Play className="w-4 h-4" />}
							isDisabled={isProcessing}
							onPress={processRecentClaims}
							className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700"
						>
							{isProcessing ? "Processing..." : "Ingest & Process Claims"}
						</Button>

						{/* Load Real Data Button */}
						<Button
							color="secondary"
							variant="bordered"
							startContent={isLoading ? <Spinner size="sm" color="secondary" /> : <RefreshCw className="w-4 h-4" />}
							isDisabled={isLoading}
							onPress={loadRealClaims}
						>
							{isLoading ? "Loading..." : "Load Real Data"}
						</Button>

						{/* Time Filter */}
						<Select
							size="sm"
							placeholder="Select time range"
							selectedKeys={[timeFilter]}
							onSelectionChange={(keys) =>
								setTimeFilter(Array.from(keys)[0] as TimeFilter)
							}
							className="w-40"
							startContent={<Calendar className="w-4 h-4" />}
						>
							{timeFilterOptions.map((option) => (
								<SelectItem key={option.key} textValue={option.key}>
									{option.label}
								</SelectItem>
							))}
						</Select>

						{/* View Toggle */}
						{renderViewToggle()}
					</div>
				</div>

				{/* Statistics Cards */}
				{/* {renderStatsCards()} */}

				{/* Processing Status */}
				{processingStage && (
					<Card className="p-4 bg-blue-50 border-blue-200 border-1">
						<div className="flex items-center gap-3">
							<Spinner size="sm" color="primary" />
							<div>
								<p className="text-sm font-medium text-blue-900">Processing Claims</p>
								<p className="text-xs text-blue-700">{processingStage}</p>
							</div>
						</div>
					</Card>
				)}

				{/* Data Source Indicator */}
				<div className="flex items-center justify-between">
					<Chip 
						variant="flat" 
						color={useRealData ? "success" : "default"}
						size="sm"
					>
						{useRealData ? "Real Data" : "Demo Data"}
					</Chip>
					<p className="text-xs text-gray-500">
						Showing {claims.length} claims
					</p>
				</div>

				{/* Main Content */}
				<div className="w-full border-zinc-200 border-1 rounded-lg p-6">
					{viewMode === "table" ? (
						<ClaimsTable />
					) : (
						<KanbanBoard
							claims={claims}
							onClaimClick={handleClaimClick}
							onReview={handleReview}
						/>
					)}
				</div>


			</div>
		</main>
	);
}
