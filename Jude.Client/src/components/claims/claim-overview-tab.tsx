import type {
	GetClaimDetailResponse,
	ClaimTotalValues,
	ServiceResponse,
	ProductResponse,
} from "@/lib/types/claim";
import { ClaimSource } from "@/lib/types/claim";
import {
	Card,
	CardBody,
	Divider,
	Table,
	TableHeader,
	TableColumn,
	TableBody,
	TableRow,
	TableCell,
	Chip,
} from "@heroui/react";
import React from "react";
import { Markdown } from "@/components/ai/markdown";

interface ClaimOverviewTabProps {
	claim: GetClaimDetailResponse;
}

const AmountList: React.FC<{
	values?: ClaimTotalValues;
	currency?: string;
}> = ({ values, currency }) => {
	if (!values) return null;
	const c = currency || "";
	const format = (v?: string) => (v ? `${c}${v}` : "-");
	return (
		<div className="grid grid-cols-2 md:grid-cols-3 gap-x-6 gap-y-2 text-sm">
			<div className="flex justify-between">
				<span className="text-foreground-500">Claimed</span>
				<span>{format(values.Claimed)}</span>
			</div>
			<div className="flex justify-between">
				<span className="text-foreground-500">Copayment</span>
				<span>{format(values.Copayment)}</span>
			</div>
			<div className="flex justify-between">
				<span className="text-foreground-500">Scheme Amount</span>
				<span>{format(values.SchemeAmount)}</span>
			</div>
			<div className="flex justify-between">
				<span className="text-foreground-500">Savings Amount</span>
				<span>{format(values.SavingsAmount)}</span>
			</div>
			<div className="flex justify-between">
				<span className="text-foreground-500">Nett Member</span>
				<span>{format(values.NettMember)}</span>
			</div>
			<div className="flex justify-between">
				<span className="text-foreground-500">Nett Provider</span>
				<span>{format(values.NettProvider)}</span>
			</div>
		</div>
	);
};

export const ClaimOverviewTab: React.FC<ClaimOverviewTabProps> = ({
	claim,
}) => {
	const isUploadedClaim = claim.source === ClaimSource.Upload;

	// For uploaded claims (from Excel), show the simplified structured data
	if (isUploadedClaim) {
		return (
			<Card shadow="none">
				<CardBody className="space-y-6">
					<div>
						<div className="flex items-center gap-2 mb-4">
							<h2 className="text-xl font-semibold">Claim Details</h2>
							<Chip color="primary" variant="flat" size="sm">
								Excel Upload
							</Chip>
						</div>
					</div>

					<div className="grid grid-cols-1 md:grid-cols-2 gap-6">
						<div>
							<h3 className="text-lg font-medium mb-3">Patient Information</h3>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">First Name</span>
									<span>{claim.patientFirstName || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Surname</span>
									<span>{claim.patientSurname || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Date of Birth</span>
									<span>
										{claim.patientBirthDate
											? new Date(claim.patientBirthDate).toLocaleDateString()
											: "-"}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Current Age</span>
									<span>{claim.patientCurrentAge || "-"}</span>
								</div>
							</div>
						</div>

						<div>
							<h3 className="text-lg font-medium mb-3">Member Information</h3>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Member Number</span>
									<span>{claim.memberNumber || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">INO</span>
									<span>{claim.ino || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">DIS</span>
									<span>{claim.dis || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Medical Scheme</span>
									<span>{claim.medicalSchemeName || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Option Name</span>
									<span>{claim.optionName || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Payer Name</span>
									<span>{claim.payerName || "-"}</span>
								</div>
							</div>
						</div>
					</div>

					<Divider />

					<div className="grid grid-cols-1 md:grid-cols-2 gap-6">
						<div>
							<h3 className="text-lg font-medium mb-3">Provider Information</h3>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Provider Name</span>
									<span>{claim.providerName || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Practice Number</span>
									<span>{claim.practiceNumber || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">
										Referring Practice
									</span>
									<span>{claim.referringPractice || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">As At Networks</span>
									<span>{claim.asAtNetworks || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Invoice Reference</span>
									<span>{claim.invoiceReference || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Assessor Name</span>
									<span>{claim.assessorName || "-"}</span>
								</div>
							</div>
						</div>

						<div>
							<h3 className="text-lg font-medium mb-3">Claim Information</h3>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Claim Number</span>
									<span>{claim.claimNumber || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Claim Line No</span>
									<span className="font-mono">{claim.claimLineNo || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Claim Code</span>
									<span>{claim.claimCode || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Code Description</span>
									<span className="text-right">
										{claim.codeDescription || "-"}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Script Code</span>
									<span>{claim.scriptCode || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">ICD-10 Code</span>
									<span className="font-mono">{claim.icd10Code || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Units</span>
									<span>{claim.units || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Claim Type Code</span>
									<span>{claim.claimTypeCode || "-"}</span>
								</div>
							</div>
						</div>
					</div>

					<Divider />

					<div>
						<h3 className="text-lg font-medium mb-3">Dates</h3>
						<div className="grid grid-cols-1 md:grid-cols-3 gap-4">
							<div className="space-y-2 bg-content2 rounded-md p-3 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Service Date</span>
									<span>
										{claim.serviceDate
											? new Date(claim.serviceDate).toLocaleDateString()
											: "-"}
									</span>
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-3 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Assessment Date</span>
									<span>
										{claim.assessmentDate
											? new Date(claim.assessmentDate).toLocaleDateString()
											: "-"}
									</span>
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-3 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Date Received</span>
									<span>
										{claim.dateReceived
											? new Date(claim.dateReceived).toLocaleDateString()
											: "-"}
									</span>
								</div>
							</div>
						</div>
					</div>

					<Divider />

					<div>
						<h3 className="text-lg font-medium mb-3">Financial Summary</h3>
						<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
							<div className="space-y-2 bg-content2 rounded-md p-4">
								<div className="text-xs text-foreground-500 uppercase">
									Amount Claimed
								</div>
								<div className="text-2xl font-semibold">
									${claim.totalClaimAmount?.toFixed(2) || "0.00"}
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-4">
								<div className="text-xs text-foreground-500 uppercase">
									Total Amount Paid
								</div>
								<div className="text-2xl font-semibold text-primary">
									${claim.totalAmountPaid?.toFixed(2) || "0.00"}
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-4">
								<div className="text-xs text-foreground-500 uppercase">
									Co-Payment
								</div>
								<div className="text-2xl font-semibold text-warning">
									${claim.coPayAmount?.toFixed(2) || "0.00"}
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-4">
								<div className="text-xs text-foreground-500 uppercase">
									Tariff
								</div>
								<div className="text-2xl font-semibold">
									${claim.tariff?.toFixed(2) || "0.00"}
								</div>
							</div>
						</div>
					</div>

					<Divider />

					<div>
						<h3 className="text-lg font-medium mb-3">Payment Breakdown</h3>
						<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="text-xs text-foreground-500 uppercase mb-2">
									Paid From Risk Amount
								</div>
								<div className="text-lg font-semibold">
									${claim.paidFromRiskAmount?.toFixed(2) || "0.00"}
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="text-xs text-foreground-500 uppercase mb-2">
									Paid From Threshold
								</div>
								<div className="text-lg font-semibold">
									${claim.paidFromThreshold?.toFixed(2) || "0.00"}
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="text-xs text-foreground-500 uppercase mb-2">
									Paid From Savings
								</div>
								<div className="text-lg font-semibold">
									${claim.paidFromSavings?.toFixed(2) || "0.00"}
								</div>
							</div>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="text-xs text-foreground-500 uppercase mb-2">
									Recovery Amount
								</div>
								<div className="text-lg font-semibold">
									${claim.recoveryAmount?.toFixed(2) || "0.00"}
								</div>
							</div>
						</div>
					</div>

					<Divider />

					<div className="grid grid-cols-1 md:grid-cols-2 gap-6">
						<div>
							<h3 className="text-lg font-medium mb-3">
								Authorization & Processing
							</h3>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Authorization No</span>
									<span className="font-mono">{claim.authNo || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Pay To</span>
									<span>{claim.payTo || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">REJ</span>
									<span>{claim.rej || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">REV</span>
									<span>{claim.rev || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">DL</span>
									<span>{claim.dl || "-"}</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">Paper/EDI</span>
									<span>{claim.paperOrEdi || "-"}</span>
								</div>
							</div>
						</div>

						<div>
							<h3 className="text-lg font-medium mb-3">Duplicate Detection</h3>
							<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
								<div className="flex justify-between">
									<span className="text-foreground-500">Duplicate Claim</span>
									<span>
										{claim.duplicateClaim ? (
											<Chip color="warning" size="sm" variant="flat">
												{claim.duplicateClaim}
											</Chip>
										) : (
											"-"
										)}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-foreground-500">
										Duplicate Claim Line
									</span>
									<span>
										{claim.duplicateClaimLine ? (
											<Chip color="warning" size="sm" variant="flat">
												{claim.duplicateClaimLine}
											</Chip>
										) : (
											"-"
										)}
									</span>
								</div>
							</div>
						</div>
					</div>

					{claim.claimMarkdown && (
						<>
							<Divider />
							<div>
								<h3 className="text-lg font-medium mb-3">
									Additional Information
								</h3>
								<div className="bg-content2 p-4 rounded-md">
									<Markdown>{claim.claimMarkdown}</Markdown>
								</div>
							</div>
						</>
					)}
				</CardBody>
			</Card>
		);
	}

	// Legacy CIMAS data structure (if any old claims exist)
	const data = (claim as any).data;
	const currency = data?.Member?.Currency || "";

	return (
		<Card shadow="none">
			<CardBody className="space-y-6">
				<div>
					<div className="flex items-center gap-2 mb-4">
						<h2 className="text-xl font-semibold">CIMAS API Data (Legacy)</h2>
						<Chip color="secondary" variant="flat" size="sm">
							Structured Data
						</Chip>
					</div>
				</div>

				<div>
					<h3 className="text-lg font-medium mb-3">Transaction</h3>
					<div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
						<div className="space-y-2 bg-content2 rounded-md p-3">
							<div className="flex justify-between">
								<span className="text-foreground-500">Type</span>
								<span>{data?.TransactionResponse?.Type || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Number</span>
								<span>{data?.TransactionResponse?.Number || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Claim #</span>
								<span>{data?.TransactionResponse?.ClaimNumber || "-"}</span>
							</div>
						</div>
						<div className="space-y-2 bg-content2 rounded-md p-3">
							<div className="flex justify-between">
								<span className="text-foreground-500">Submitted By</span>
								<span>{data?.TransactionResponse?.SubmittedBy || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Date/Time</span>
								<span>
									{data?.TransactionResponse?.DateTime
										? new Date(
												data.TransactionResponse.DateTime,
											).toLocaleString()
										: "-"}
								</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Reversed</span>
								<span>
									{data?.TransactionResponse?.Reversed ? "Yes" : "No"}
								</span>
							</div>
						</div>
						<div className="space-y-2 bg-content2 rounded-md p-3">
							<div className="flex justify-between">
								<span className="text-foreground-500">Date Reversed</span>
								<span>
									{data?.TransactionResponse?.DateReversed
										? new Date(
												data.TransactionResponse.DateReversed,
											).toLocaleString()
										: "-"}
								</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Response Code</span>
								<span>{data?.ClaimHeaderResponse?.ResponseCode || "-"}</span>
							</div>
						</div>
					</div>
				</div>

				<Divider />

				<div className="grid grid-cols-1 md:grid-cols-2 gap-6">
					<div>
						<h3 className="text-lg font-medium mb-3">Member</h3>
						<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
							<div className="flex justify-between">
								<span className="text-foreground-500">Scheme #</span>
								<span>{data?.Member?.MedicalSchemeNumber ?? "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Scheme Name</span>
								<span>{data?.Member?.MedicalSchemeName || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Currency</span>
								<span>{currency || "-"}</span>
							</div>
						</div>
					</div>

					<div>
						<h3 className="text-lg font-medium mb-3">Patient</h3>
						<div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
							<div className="flex justify-between">
								<span className="text-foreground-500">Dependant Code</span>
								<span>{data?.Patient?.DependantCode ?? "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">First Name</span>
								<span>{data?.Patient?.Personal?.FirstName || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Surname</span>
								<span>{data?.Patient?.Personal?.Surname || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Initials</span>
								<span>{data?.Patient?.Personal?.Initials || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">Gender</span>
								<span>{data?.Patient?.Personal?.Gender || "-"}</span>
							</div>
							<div className="flex justify-between">
								<span className="text-foreground-500">DOB</span>
								<span>
									{data?.Patient?.Personal?.DateOfBirth
										? new Date(
												data.Patient.Personal.DateOfBirth,
											).toLocaleDateString()
										: "-"}
								</span>
							</div>
						</div>
					</div>
				</div>

				<Divider />

				<div>
					<div className="flex items-center gap-2 mb-3">
						<h3 className="text-lg font-medium">Totals</h3>
						<Chip size="sm" variant="flat" color="default">
							Overall
						</Chip>
					</div>
					<AmountList
						values={data?.ClaimHeaderResponse?.TotalValues}
						currency={currency}
					/>
				</div>

				<Divider />

				<div>
					<h3 className="text-lg font-medium mb-3">Services</h3>
					{data?.ServiceResponse && data.ServiceResponse.length > 0 ? (
						<Table aria-label="Services">
							<TableHeader>
								<TableColumn>NUMBER</TableColumn>
								<TableColumn>CODE</TableColumn>
								<TableColumn>DESCRIPTION</TableColumn>
								<TableColumn>SUBTOTAL (Claimed)</TableColumn>
								<TableColumn>TOTAL (Claimed)</TableColumn>
								<TableColumn>MESSAGE</TableColumn>
							</TableHeader>
							<TableBody>
								{data.ServiceResponse.map((s: ServiceResponse, idx: number) => (
									<TableRow key={`${s.Number}-${idx}`}>
										<TableCell>{s.Number}</TableCell>
										<TableCell>{s.Code}</TableCell>
										<TableCell>{s.Description}</TableCell>
										<TableCell>
											{currency}
											{s.SubTotalValues?.Claimed || "-"}
										</TableCell>
										<TableCell>
											{currency}
											{s.TotalValues?.Claimed || "-"}
										</TableCell>
										<TableCell>
											{s.Message?.Type ? (
												<div className="text-xs">
													<div className="font-medium">
														{s.Message.Type}
														{s.Message.Code ? ` (${s.Message.Code})` : ""}
													</div>
													<div className="text-foreground-500">
														{s.Message.Description || ""}
													</div>
												</div>
											) : (
												"-"
											)}
										</TableCell>
									</TableRow>
								))}
							</TableBody>
						</Table>
					) : (
						<div className="text-sm text-foreground-500">No services</div>
					)}
				</div>

				<Divider />

				<div>
					<h3 className="text-lg font-medium mb-3">Products</h3>
					{data?.ProductResponse && data.ProductResponse.length > 0 ? (
						<Table aria-label="Products">
							<TableHeader>
								<TableColumn>NUMBER</TableColumn>
								<TableColumn>CODE</TableColumn>
								<TableColumn>DESCRIPTION</TableColumn>
								<TableColumn>SUBTOTAL (Claimed)</TableColumn>
								<TableColumn>TOTAL (Claimed)</TableColumn>
								<TableColumn>MESSAGE</TableColumn>
							</TableHeader>
							<TableBody>
								{data.ProductResponse.map((p: ProductResponse, idx: number) => (
									<TableRow key={`${p.Number}-${idx}`}>
										<TableCell>{p.Number}</TableCell>
										<TableCell>{p.Code}</TableCell>
										<TableCell>{p.Description}</TableCell>
										<TableCell>
											{currency}
											{p.SubTotalValues?.Claimed || "-"}
										</TableCell>
										<TableCell>
											{currency}
											{p.TotalValues?.Claimed || "-"}
										</TableCell>
										<TableCell>
											{p.Message?.Type ? (
												<div className="text-xs">
													<div className="font-medium">
														{p.Message.Type}
														{p.Message.Code ? ` (${p.Message.Code})` : ""}
													</div>
													<div className="text-foreground-500">
														{p.Message.Description || ""}
													</div>
												</div>
											) : (
												"-"
											)}
										</TableCell>
									</TableRow>
								))}
							</TableBody>
						</Table>
					) : (
						<div className="text-sm text-foreground-500">No products</div>
					)}
				</div>
			</CardBody>
		</Card>
	);
};

export default ClaimOverviewTab;
