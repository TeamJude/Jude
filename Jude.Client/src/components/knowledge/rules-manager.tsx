import { createRule, getRules } from "@/lib/services/rules.service";
import { RuleStatus, type Rule } from "@/lib/types/rule";
import {
	addToast,
	Button,
	Card,
	CardBody,
	Input,
	Modal,
	ModalBody,
	ModalContent,
	ModalFooter,
	ModalHeader,
	Pagination,
	Select,
	SelectItem,
	Spinner,
	Switch,
	Table,
	TableBody,
	TableCell,
	TableColumn,
	TableHeader,
	TableRow,
	Textarea,
	useDisclosure,
} from "@heroui/react";
import { useForm } from "@tanstack/react-form";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import React, { useState } from "react";

export const RulesManager: React.FC = () => {
	const queryClient = useQueryClient();
	const { isOpen, onOpen, onOpenChange } = useDisclosure();
	const [editingRule, setEditingRule] = useState<Rule | null>(null);
	const [errors, setErrors] = useState<string[]>([]);
	const [page, setPage] = useState(1);
	const [rowsPerPage, setRowsPerPage] = useState(10);

	const { isPending, error, data } = useQuery({
		queryKey: ["rules", page, rowsPerPage],
		queryFn: () => getRules({ page, pageSize: rowsPerPage }),
	});
	const createRuleMutation = useMutation({
		mutationFn: (data: {
			name: string;
			description: string;
			status: RuleStatus;
			priority: number;
		}) => createRule(data),
		onSuccess: (response) => {
			if (response.success) {
				queryClient.invalidateQueries({ queryKey: ["rules"] });
				addToast({
					title: "Rule created successfully",
				});
				onOpenChange();
				form.reset();
			} else {
				setErrors(response.errors);
			}
		},
		onError: (error: Error) => {
			setErrors([error.message || "An unexpected error occurred."]);
		},
	});

	const form = useForm({
		defaultValues: {
			name: "",
			description: "",
			status: RuleStatus.Active,
			priority: 1,
		},
		onSubmit: async ({ value }) => {
			setErrors([]);
			createRuleMutation.mutate(value);
		},
	});

	const handleOpenModal = (rule?: Rule) => {
		setErrors([]);
		if (rule) {
			setEditingRule(rule);
			form.setFieldValue("name", rule.name);
			form.setFieldValue("description", rule.description);
			form.setFieldValue("status", rule.status);
		} else {
			setEditingRule(null);
			form.reset();
		}
		onOpen();
	};

	const handleStatusToggle = (rule: Rule) => {
		// TODO: Implement status update functionality
		console.log("Toggle status for:", rule.id);
	};

	const rules = data?.success ? data.data.rules || [] : [];
	const totalCount = data?.success ? data.data.totalCount || 0 : 0;
	const pages = Math.ceil(totalCount / rowsPerPage);

	const onRowsPerPageChange = React.useCallback(
		(e: React.ChangeEvent<HTMLSelectElement>) => {
			setRowsPerPage(Number(e.target.value));
			setPage(1);
		},
		[],
	);

	return (
		<>
			<Card className="shadow-sm border-zinc-200 border-1">
				<CardBody>
					<div className="flex justify-between items-center mb-4">
						<h2 className="text-xl font-semibold">Claims Processing Rules</h2>
						<Button
							color="primary"
							onPress={() => handleOpenModal()}
							startContent={<Plus width={16} />}
						>
							Create New Rule
						</Button>
					</div>

					{isPending ? (
						<div className="flex items-center justify-center h-40">
							<Spinner label="Loading rules..." />
						</div>
					) : (
						<>
							<div className="flex justify-between items-center mb-4">
								<span className="text-default-400 text-small">
									Total {totalCount} rules
								</span>
								<label className="flex items-center text-default-400 text-small">
									Rows per page:
									<select
										className="bg-transparent outline-none text-default-400 text-small ml-2"
										value={rowsPerPage}
										onChange={onRowsPerPageChange}
									>
										<option value="5">5</option>
										<option value="10">10</option>
										<option value="15">15</option>
									</select>
								</label>
							</div>

							<Table 
								aria-label="Claims processing rules table" 
								removeWrapper
								bottomContent={
									pages > 1 && (
										<div className="py-2 px-2 flex justify-center items-center">
											<Pagination
												isCompact
												showControls
												showShadow
												color="primary"
												page={page}
												total={pages}
												onChange={setPage}
											/>
										</div>
									)
								}
								bottomContentPlacement="outside"
							>
								<TableHeader>
									<TableColumn key="name">RULE NAME</TableColumn>
									<TableColumn key="description">DESCRIPTION</TableColumn>
									<TableColumn key="status">STATUS</TableColumn>
									<TableColumn key="createdAt">CREATED</TableColumn>
									<TableColumn key="actions" className="text-right">
										ACTIONS
									</TableColumn>
								</TableHeader>
								<TableBody
									emptyContent={
										error
											? "Error loading rules"
											: "No rules found"
									}
								>
									{rules.map((rule) => (
										<TableRow key={rule.id}>
											<TableCell className="font-medium">{rule.name}</TableCell>

											<TableCell className="max-w-md">
												<div className="truncate" title={rule.description}>
													{rule.description}
												</div>
											</TableCell>

											<TableCell>
												<Switch
													isSelected={rule.status === RuleStatus.Active}
													onValueChange={() => handleStatusToggle(rule)}
													size="sm"
													color="success"
												/>
											</TableCell>

											<TableCell>
												{new Date(rule.createdAt).toLocaleDateString()}
											</TableCell>

											<TableCell className="text-right">
												<div className="flex justify-end gap-2">
													<Button
														size="sm"
														variant="flat"
														color="primary"
														onPress={() => handleOpenModal(rule)}
													>
														Edit
													</Button>
													<Button size="sm" variant="light" color="primary">
														View
													</Button>
												</div>
											</TableCell>
										</TableRow>
									))}
								</TableBody>
							</Table>
						</>
					)}
				</CardBody>
			</Card>
			<Modal isOpen={isOpen} onOpenChange={onOpenChange} size="lg">
				<ModalContent>
					{(onClose) => (
						<>
							<ModalHeader className="flex flex-col gap-1">
								{editingRule ? "Edit Rule" : "Create New Rule"}
							</ModalHeader>
							<form
								onSubmit={(e) => {
									e.preventDefault();
									e.stopPropagation();
									form.handleSubmit();
								}}
							>
								<ModalBody className="space-y-4">
									{errors.length > 0 && (
										<div className="bg-danger-50 border border-danger-200 rounded-lg p-3">
											<ul className="list-disc ml-5 text-sm text-danger-600">
												{errors.map((error, index) => (
													<li key={index}>{error}</li>
												))}
											</ul>
										</div>
									)}

									<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
										<form.Field name="name">
											{(field) => (
												<Input
													label="Rule Name"
													placeholder="Enter rule name"
													value={field.state.value}
													onValueChange={(v) => field.handleChange(v)}
													isRequired
													isDisabled={createRuleMutation.isPending}
												/>
											)}
										</form.Field>

										<div className="flex gap-4">
											<form.Field name="priority">
												{(field) => (
													<Select
														label="Priority"
														placeholder="Select priority"
														selectedKeys={[field.state.value.toString()]}
														onChange={(e) =>
															field.handleChange(Number(e.target.value))
														}
														className="flex-1"
														isDisabled={createRuleMutation.isPending}
													>
														{[1, 2, 3, 4, 5].map((priority) => (
															<SelectItem
																key={priority.toString()}
																textValue={priority.toString()}
															>
																{priority}
															</SelectItem>
														))}
													</Select>
												)}
											</form.Field>

											<div className="flex flex-col flex-1">
												<span className="text-sm mb-2 text-foreground-600">
													Status
												</span>
												<form.Field name="status">
													{(field) => (
														<div className="flex items-center h-[40px]">
															<Switch
																isSelected={
																	field.state.value === RuleStatus.Active
																}
																onValueChange={(selected) =>
																	field.handleChange(
																		selected
																			? RuleStatus.Active
																			: RuleStatus.Inactive,
																	)
																}
																size="sm"
																color="success"
																isDisabled={createRuleMutation.isPending}
															/>
															<span className="ml-2 text-sm">
																{field.state.value}
															</span>
														</div>
													)}
												</form.Field>
											</div>
										</div>
									</div>

									<form.Field name="description">
										{(field) => (
											<Textarea
												label="Description"
												placeholder="Enter rule description"
												value={field.state.value}
												onValueChange={(v) => field.handleChange(v)}
												rows={4}
												isRequired
												isDisabled={createRuleMutation.isPending}
											/>
										)}
									</form.Field>
								</ModalBody>
								<ModalFooter>
									<Button
										variant="flat"
										onPress={onClose}
										isDisabled={createRuleMutation.isPending}
									>
										Cancel
									</Button>
									<Button
										color="primary"
										type="submit"
										isLoading={createRuleMutation.isPending}
									>
										{editingRule ? "Update Rule" : "Create Rule"}
									</Button>
								</ModalFooter>
							</form>
						</>
					)}
				</ModalContent>
			</Modal>
		</>
	);
};
