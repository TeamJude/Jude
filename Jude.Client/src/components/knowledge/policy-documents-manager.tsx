import {
	addPolicyDocument,
	getPolicies,
	getPolicyDocumentUrl,
} from "@/lib/services/policies.service";
import { PolicyStatus, type Policy } from "@/lib/types/policy";
import {
	addToast,
	Button,
	Card,
	CardBody,
	Chip,
	Dropdown,
	DropdownItem,
	DropdownMenu,
	DropdownTrigger,
	Input,
	Modal,
	ModalBody,
	ModalContent,
	ModalFooter,
	ModalHeader,
	Table,
	TableBody,
	TableCell,
	TableColumn,
	TableHeader,
	TableRow,
	useDisclosure,
} from "@heroui/react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Archive, Eye, MoreVertical, Upload, UploadCloud } from "lucide-react";
import React from "react";
import { DocumentViewer } from "./document-viewer";

export const PolicyDocumentsManager: React.FC = () => {
	const { isOpen, onOpen, onOpenChange } = useDisclosure();
	const {
		isOpen: isViewerOpen,
		onOpen: onViewerOpen,
		onOpenChange: onViewerOpenChange,
	} = useDisclosure();
	const queryClient = useQueryClient();

	const { isPending, error, data } = useQuery({
		queryKey: ["policies"],
		queryFn: () => getPolicies({ page: 1, pageSize: 100 }),
	});

	const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
	const [documentName, setDocumentName] = React.useState("");
	const [isUploading, setIsUploading] = React.useState(false);
	const [isLoadingDocument, setIsLoadingDocument] = React.useState(false);
	const [documentError, setDocumentError] = React.useState<string | null>(null);
	const [selectedPolicy, setSelectedPolicy] = React.useState<Policy | null>(
		null,
	);

	const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		if (e.target.files && e.target.files[0]) {
			setSelectedFile(e.target.files[0]);
		}
	};

	const handleUpload = async () => {
		if (!selectedFile || !documentName) return;
		setIsUploading(true);

		const formData = new FormData();
		formData.append("file", selectedFile);
		formData.append("name", documentName);

		const result = await addPolicyDocument(formData);

		if (result.success) {
			addToast({
				title: "Policy Document Upload Complete",
				description:
					"Your document has been uploaded and is now being indexed.",
				color: "success",
			});

			setSelectedFile(null);
			setDocumentName("");

			queryClient.invalidateQueries({ queryKey: ["policies"] });
		} else {
			addToast({
				title: "Upload Failed",
				description: result.errors?.[0] || "Failed to upload policy document.",
				color: "danger",
			});
		}

		setIsUploading(false);
		onOpenChange();
	};

	const toggleStatus = (id: number) => {
		// TODO: Implement status toggle functionality
		console.log("Toggle status for policy:", id);
	};

	const handleViewDocument = async (policy: Policy) => {
		console.log(policy);
		setSelectedPolicy(policy);
		setDocumentError(null);
		setIsLoadingDocument(true);
		onViewerOpen();
	};

	const handleCloseViewer = () => {
		setDocumentError(null);
		setSelectedPolicy(null);
		onViewerOpenChange();
	};

	return (
		<>
			<Card className="shadow-sm border-zinc-200 border-1">
				<CardBody>
					<div className="flex justify-between items-center mb-4">
						<h2 className="text-xl font-semibold">Policy Documents</h2>
						<Button
							color="primary"
							onPress={onOpen}
							startContent={<Upload width={16} />}
							isDisabled={isUploading}
						>
							Index New Document
						</Button>
					</div>
					<Table
						aria-label="Policy documents table"
						removeWrapper
						classNames={{
							wrapper: "max-h-0 flex-1 overflow-auto",
							base: "flex flex-col h-full",
							table: "h-full",
						}}
					>
						<TableHeader>
							<TableColumn key="id">ID</TableColumn>
							<TableColumn key="name">NAME</TableColumn>
							<TableColumn key="createdAt">DATE CREATED</TableColumn>
							<TableColumn key="status">STATUS</TableColumn>
							<TableColumn key="actions" className="text-right">
								ACTIONS
							</TableColumn>
						</TableHeader>
						<TableBody
							emptyContent={
								isPending
									? "Loading..."
									: error
										? "Error loading policy documents"
										: "No policy documents found"
							}
						>
							{data?.success
								? data.data.policies.map((policy) => (
										<TableRow key={policy.id}>
											<TableCell>{policy.id}</TableCell>
											<TableCell>{policy.name}</TableCell>
											<TableCell>
												{new Date(policy.createdAt).toLocaleDateString()}
											</TableCell>
											<TableCell>
												<Chip
													color={
														(
															{
																[PolicyStatus.Pending]: "warning",
																[PolicyStatus.Active]: "success",
																[PolicyStatus.Failed]: "danger",
																[PolicyStatus.Archived]: "default",
															} as const
														)[policy.status] || "default"
													}
													variant="flat"
													size="sm"
												>
													{PolicyStatus[policy.status]}
												</Chip>
											</TableCell>
											<TableCell className="text-right">
												<div className="flex justify-end gap-2">
													<Button
														isIconOnly
														disabled={
															policy.status === PolicyStatus.Pending ||
															policy.status == PolicyStatus.Failed
														}
														size="sm"
														variant="light"
														color="primary"
														onPress={() => handleViewDocument(policy)}
													>
														<Eye width={16} />
													</Button>
													<Button
														isIconOnly
														size="sm"
														variant="light"
														color={
															policy.status === PolicyStatus.Active
																? "default"
																: "success"
														}
														onPress={() => toggleStatus(policy.id)}
													>
														<Archive width={16} />
													</Button>
													<Dropdown>
														<DropdownTrigger>
															<Button isIconOnly size="sm" variant="light">
																<MoreVertical width={16} />
															</Button>
														</DropdownTrigger>
														<DropdownMenu aria-label="Document Actions">
															<DropdownItem key="view">
																View Content
															</DropdownItem>
															<DropdownItem key="download">
																Download
															</DropdownItem>
															<DropdownItem key="history">
																Version History
															</DropdownItem>
														</DropdownMenu>
													</Dropdown>
												</div>
											</TableCell>
										</TableRow>
									))
								: []}
						</TableBody>
					</Table>
				</CardBody>
			</Card>

			<Modal isOpen={isOpen} onOpenChange={onOpenChange}>
				<ModalContent>
					{(onClose) => (
						<>
							<ModalHeader className="flex flex-col gap-1">
								New Policy Document
							</ModalHeader>
							<ModalBody>
								<Input
									label="Document Name"
									placeholder="Enter document name"
									value={documentName}
									onValueChange={setDocumentName}
								/>
								<div className="flex flex-col gap-2">
									<label className="text-sm">Document File</label>
									<div className="flex items-center justify-center w-full">
										<label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed rounded-lg cursor-pointer bg-content2 hover:bg-content3 transition-colors">
											{selectedFile ? (
												<div className="flex flex-col items-center justify-center pt-5 pb-6">
													<UploadCloud className="w-8 h-8 mb-3 text-foreground-500" />
													<p className="mb-2 text-sm text-foreground-600 font-medium">
														{selectedFile.name}
													</p>
													<p className="text-xs text-foreground-500">
														Click to change file
													</p>
												</div>
											) : (
												<div className="flex flex-col items-center justify-center pt-5 pb-6">
													<UploadCloud className="w-8 h-8 mb-3 text-foreground-500" />
													<p className="mb-2 text-sm text-foreground-600">
														<span className="font-medium">Click to upload</span>{" "}
														or drag and drop
													</p>
													<p className="text-xs text-foreground-500">
														PDF, DOCX (Max 10MB)
													</p>
												</div>
											)}
											<input
												type="file"
												className="hidden"
												accept=".pdf,.docx"
												onChange={handleFileChange}
											/>
										</label>
									</div>
								</div>
							</ModalBody>
							<ModalFooter>
								<Button variant="flat" onPress={onClose}>
									Cancel
								</Button>
								<Button
									color="primary"
									onPress={handleUpload}
									isDisabled={!selectedFile || !documentName || isUploading}
									isLoading={isUploading}
								>
									{isUploading ? "Uploading..." : "Upload & Index"}
								</Button>
							</ModalFooter>
						</>
					)}
				</ModalContent>
			</Modal>

			<Modal
				isOpen={isViewerOpen}
				onOpenChange={onViewerOpenChange}
				size="5xl"
				classNames={{
					base: "max-w-6xl",
					body: "p-0",
				}}
			>
				<ModalContent>
					{() => (
						<ModalBody>
							<DocumentViewer
								url={selectedPolicy?.documentUrl!}
								isLoading={isLoadingDocument}
								documentName={selectedPolicy?.name}
								onClose={handleCloseViewer}
								error={documentError}
							/>
						</ModalBody>
					)}
				</ModalContent>
			</Modal>
		</>
	);
};
