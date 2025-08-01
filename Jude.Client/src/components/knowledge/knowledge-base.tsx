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
import { Archive, Eye, MoreVertical, Upload, UploadCloud } from "lucide-react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getPolicies } from "@/lib/services/policies.service";
import type { Policy } from "@/lib/types/policy";
import { PolicyStatus } from "@/lib/types/policy";

export const PolicyDocumentManager: React.FC = () => {
	const { isOpen, onOpen, onOpenChange } = useDisclosure();
	const queryClient = useQueryClient();
	
	const { isPending, error, data } = useQuery({
		queryKey: ["policies"],
		queryFn: () => getPolicies({ page: 1, pageSize: 100 }),
	});

	const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
	const [documentName, setDocumentName] = React.useState("");
	const [documentVersion, setDocumentVersion] = React.useState("");
	const [isUploading, setIsUploading] = React.useState(false);
	const uploadIntervalRef = React.useRef<number | null>(null);

	// Cleanup interval on unmount
	React.useEffect(() => {
		return () => {
			if (uploadIntervalRef.current) {
				clearInterval(uploadIntervalRef.current);
			}
		};
	}, []);

	const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		if (e.target.files && e.target.files[0]) {
			setSelectedFile(e.target.files[0]);
		}
	};

	const handleUpload = () => {
		setIsUploading(true);
		onOpenChange(); // Close modal first
		
		// Show initial upload toast
		addToast({
			title: `Document Upload Started: ${documentName}`,
		});
		
		// Start the upload simulation
		let progress = 0;
		const totalDuration = 120000; // 2 minutes in milliseconds
		const updateInterval = 5000; // Update every 5 seconds to avoid spam
		const progressIncrement = 100 / (totalDuration / updateInterval);
		
		const updateProgress = () => {
			progress += progressIncrement;
			
			if (progress >= 100) {
				progress = 100;
				// Upload completed
				addToast({
					title: "Document Upload Complete",
				});
				
				setIsUploading(false);
				setSelectedFile(null);
				setDocumentName("");
				setDocumentVersion("");
				queryClient.invalidateQueries({ queryKey: ["policies"] });
				
				if (uploadIntervalRef.current) {
					clearInterval(uploadIntervalRef.current);
					uploadIntervalRef.current = null;
				}
			} else {
				// Show progress update
				addToast({
					title: `Upload Progress: ${Math.round(progress)}% - ${documentName}`,
				});
			}
		};

		uploadIntervalRef.current = setInterval(updateProgress, updateInterval);
	};

	const toggleStatus = (id: number) => {
		// TODO: Implement status toggle functionality
		console.log("Toggle status for policy:", id);
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
										? "Error loading policies"
										: "No policies found"
							}
						>
							{data?.success
								? data.data.policies.map((policy) => (
										<TableRow key={policy.id}>
											<TableCell>POL-{policy.id}</TableCell>
											<TableCell>{policy.name}</TableCell>
											<TableCell>
												{new Date(policy.createdAt).toLocaleDateString()}
											</TableCell>
											<TableCell>
												<Chip
													color={
														policy.status === PolicyStatus.Active
															? "success"
															: "default"
													}
													variant="flat"
													size="sm"
												>
													{policy.status === PolicyStatus.Active
														? "Active"
														: "Archived"}
												</Chip>
											</TableCell>
											<TableCell className="text-right">
												<div className="flex justify-end gap-2">
													<Button
														isIconOnly
														size="sm"
														variant="light"
														color="primary"
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
															<DropdownItem key="view">View Content</DropdownItem>
															<DropdownItem key="download">Download</DropdownItem>
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
								<Input
									label="Version"
									placeholder="e.g., v1.0"
									value={documentVersion}
									onValueChange={setDocumentVersion}
								/>
								<div className="flex flex-col gap-2">
									<label className="text-sm">Document File</label>
									<div className="flex items-center justify-center w-full">
										<label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed rounded-lg cursor-pointer bg-content2 hover:bg-content3 transition-colors">
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
											<input
												type="file"
												className="hidden"
												accept=".pdf,.docx"
												onChange={handleFileChange}
											/>
										</label>
									</div>
									{selectedFile && (
										<p className="text-sm text-foreground-600">
											Selected: {selectedFile.name}
										</p>
									)}
								</div>
							</ModalBody>
							<ModalFooter>
								<Button variant="flat" onPress={onClose}>
									Cancel
								</Button>
								<Button
									color="primary"
									onPress={handleUpload}
									isDisabled={
										!selectedFile || !documentName || !documentVersion || isUploading
									}
									isLoading={isUploading}
								>
									{isUploading ? "Uploading..." : "Upload & Index"}
								</Button>
							</ModalFooter>
						</>
					)}
				</ModalContent>
			</Modal>
		</>
	);
};
