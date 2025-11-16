import { getClaims } from "@/lib/services/claims.service";
import { uploadExcelClaims } from "@/lib/services/excel-upload.service";
import { ClaimStatus, type GetClaimResponse } from "@/lib/types/claim";
import {
	Button,
	Chip,
	type ChipProps,
	Dropdown,
	DropdownItem,
	DropdownMenu,
	DropdownTrigger,
	Input,
	Pagination,
	type Selection,
	Spinner,
	Table,
	TableBody,
	TableCell,
	TableColumn,
	TableHeader,
	TableRow,
} from "@heroui/react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import {
	AlertCircle,
	CheckCircle,
	ChevronDown,
	Clock,
	EllipsisVertical,
	Search,
	Upload,
	XCircle,
} from "lucide-react";
import React from "react";

const columns = [
	{ name: "CLAIM ID", uid: "id", sortable: true },
	{ name: "PATIENT", uid: "patient", sortable: true },
	{ name: "AMOUNT", uid: "amount", sortable: true },
	{ name: "STATUS", uid: "status", sortable: true },
	{ name: "ACTIONS", uid: "actions" },
];

const statusOptions = [
	{ name: "Pending", uid: ClaimStatus.Pending },
	{ name: "Under Agent Review", uid: ClaimStatus.UnderAgentReview },
	{ name: "Under Human Review", uid: ClaimStatus.UnderHumanReview },
	{ name: "Approved", uid: ClaimStatus.Approved },
	{ name: "Rejected", uid: ClaimStatus.Rejected },
	{ name: "Completed", uid: ClaimStatus.Completed },
	{ name: "Failed", uid: ClaimStatus.Failed },
];

const statusColorMap: Record<ClaimStatus, ChipProps["color"]> = {
	[ClaimStatus.Pending]: "default",
	[ClaimStatus.UnderAgentReview]: "primary",
	[ClaimStatus.UnderHumanReview]: "warning",
	[ClaimStatus.Approved]: "success",
	[ClaimStatus.Rejected]: "danger",
	[ClaimStatus.Completed]: "success",
	[ClaimStatus.Failed]: "danger",
};

const getStatusIcon = (status: ClaimStatus) => {
	switch (status) {
		case ClaimStatus.Completed:
		case ClaimStatus.Approved:
			return <CheckCircle className="w-4 h-4" />;
		case ClaimStatus.Failed:
		case ClaimStatus.Rejected:
			return <XCircle className="w-4 h-4" />;
		case ClaimStatus.UnderAgentReview:
		case ClaimStatus.UnderHumanReview:
			return <AlertCircle className="w-4 h-4" />;
		case ClaimStatus.Pending:
		default:
			return <Clock className="w-4 h-4" />;
	}
};

const INITIAL_VISIBLE_COLUMNS = [
	"id",
	"patient",
	"amount",
	"status",
	"actions",
];

export function ClaimsTable() {
	const [search, setSearch] = React.useState("");
	const [selectedKeys, setSelectedKeys] = React.useState<Selection>(
		new Set([]),
	);
	const [visibleColumns, setVisibleColumns] = React.useState<Selection>(
		new Set(INITIAL_VISIBLE_COLUMNS),
	);
	const [statusFilter, setStatusFilter] = React.useState<Selection>("all");
	const [rowsPerPage, setRowsPerPage] = React.useState(10);
	const [page, setPage] = React.useState(1);
	const [isUploading, setIsUploading] = React.useState(false);
	const navigate = useNavigate();

	const {
		data: claimsResponse,
		isLoading,
		error,
		refetch,
	} = useQuery({
		queryKey: ["claims", page, rowsPerPage, statusFilter],
		queryFn: () =>
			getClaims({
				page,
				pageSize: rowsPerPage,
				status:
					statusFilter !== "all" && statusFilter.size > 0
						? (Array.from(statusFilter).map((key) =>
								Number(key),
							) as ClaimStatus[])
						: undefined,
			}),
	});

	const claims = claimsResponse?.success
		? claimsResponse.data.claims || []
		: [];
	const totalCount = claimsResponse?.success
		? claimsResponse.data.totalCount || 0
		: 0;
	const pages = Math.ceil(totalCount / rowsPerPage);

	const headerColumns = React.useMemo(() => {
		if (visibleColumns === "all") return columns;
		return columns.filter((column) =>
			Array.from(visibleColumns).includes(column.uid),
		);
	}, [visibleColumns]);

	const onRowsPerPageChange = React.useCallback(
		(e: React.ChangeEvent<HTMLSelectElement>) => {
			setRowsPerPage(Number(e.target.value));
			setPage(1);
		},
		[],
	);

	const handleClaimClick = (claimId: string) => {
		navigate({ to: `/claims/${claimId}` });
	};

	const handleFileUpload = async (file: File) => {
		const allowedExtensions = [".xlsx", ".xls"];
		const fileExtension = file.name
			.substring(file.name.lastIndexOf("."))
			.toLowerCase();

		if (!allowedExtensions.includes(fileExtension)) {
			alert("Please select an Excel file (.xlsx or .xls)");
			return;
		}

		setIsUploading(true);

		try {
			const result = await uploadExcelClaims(file);

			if (result.success) {
				const { totalRows } = result.data;

				alert(
					`Excel file uploaded successfully!\n\n` +
						`Total claims found: ${totalRows}\n\n` +
						`Claims are being processed in the background.\n` +
						`Duplicates will be automatically skipped.\n\n` +
						`Refresh the page to see newly processed claims.`,
				);

				setTimeout(() => {
					refetch();
				}, 2000);
			} else {
				throw new Error(result.errors?.[0] || "Upload failed");
			}
		} catch (error) {
			console.error("Upload error:", error);
			alert(
				`Upload failed: ${error instanceof Error ? error.message : "Unknown error"}`,
			);
		} finally {
			setIsUploading(false);
		}
	};

	const renderCell = React.useCallback(
		(claim: GetClaimResponse, columnKey: React.Key) => {
			const cellValue = claim[columnKey as keyof GetClaimResponse];

			switch (columnKey) {
				case "id":
					return (
						<div className="flex items-center gap-2">
							<span className="text-small font-mono">{claim.claimNumber}</span>
						</div>
					);
				case "patient":
					return (
						<div className="flex flex-col">
							<p className="text-bold text-small capitalize">
								{claim.patientFirstName} {claim.patientSurname}
							</p>
							<p className="text-bold text-tiny capitalize text-default-500">
								{claim.medicalSchemeName}
							</p>
						</div>
					);
				case "amount":
					return (
						<div className="flex flex-col">
							<span className="font-medium">
								${claim.totalClaimAmount.toLocaleString()}
							</span>
						</div>
					);
				case "status":
					return (
						<Chip
							className="capitalize"
							color={statusColorMap[claim.status]}
							size="sm"
							variant="flat"
							startContent={getStatusIcon(claim.status)}
						>
							{ClaimStatus[claim.status]}
						</Chip>
					);
				case "actions":
					return (
						<div className="relative flex justify-end items-center gap-2">
							<Dropdown>
								<DropdownTrigger>
									<Button isIconOnly size="sm" variant="light">
										<EllipsisVertical className="text-default-300" />
									</Button>
								</DropdownTrigger>
								<DropdownMenu>
									<DropdownItem
										key="view"
										onClick={() => handleClaimClick(claim.id)}
									>
										View Details
									</DropdownItem>
								</DropdownMenu>
							</Dropdown>
						</div>
					);
				default:
					return cellValue;
			}
		},
		[navigate],
	);

	const topContent = React.useMemo(() => {
		return (
			<div className="flex flex-col gap-4">
				<div className="flex justify-between gap-3 items-end">
					<Input
						isClearable
						className="w-full sm:max-w-[44%]"
						placeholder="Search by transaction number or patient name..."
						startContent={<Search />}
						value={search}
						onClear={() => setSearch("")}
						onValueChange={setSearch}
					/>
					<div className="flex gap-3">
						<Button
							color="primary"
							startContent={<Upload className="w-4 h-4" />}
							isLoading={isUploading}
							isDisabled={isUploading}
							onPress={() => {
								if (isUploading) return;
								const input = document.createElement("input");
								input.type = "file";
								input.accept =
									".xlsx,.xls,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel";
								input.onchange = (e) => {
									const file = (e.target as HTMLInputElement).files?.[0];
									if (file) {
										handleFileUpload(file);
									}
								};
								input.click();
							}}
						>
							{isUploading ? "Processing..." : "Upload Excel"}
						</Button>
						<Dropdown>
							<DropdownTrigger className="hidden sm:flex">
								<Button
									endContent={<ChevronDown className="text-small" />}
									variant="flat"
								>
									Status
								</Button>
							</DropdownTrigger>
							<DropdownMenu
								disallowEmptySelection
								aria-label="Status Filter"
								closeOnSelect={false}
								selectedKeys={statusFilter}
								selectionMode="multiple"
								onSelectionChange={setStatusFilter}
							>
								{statusOptions.map((status) => (
									<DropdownItem key={status.uid} className="capitalize">
										{status.name}
									</DropdownItem>
								))}
							</DropdownMenu>
						</Dropdown>
						<Dropdown>
							<DropdownTrigger className="hidden sm:flex">
								<Button
									endContent={<ChevronDown className="text-small" />}
									variant="flat"
								>
									Columns
								</Button>
							</DropdownTrigger>
							<DropdownMenu
								disallowEmptySelection
								aria-label="Table Columns"
								closeOnSelect={false}
								selectedKeys={visibleColumns}
								selectionMode="multiple"
								onSelectionChange={setVisibleColumns}
							>
								{columns.map((column) => (
									<DropdownItem key={column.uid} className="capitalize">
										{column.name}
									</DropdownItem>
								))}
							</DropdownMenu>
						</Dropdown>
					</div>
				</div>
				<div className="flex justify-between items-center">
					<span className="text-default-400 text-small">
						Total {totalCount} claims
					</span>
					<label className="flex items-center text-default-400 text-small">
						Rows per page:
						<select
							className="bg-transparent outline-none text-default-400 text-small"
							onChange={onRowsPerPageChange}
							value={rowsPerPage}
						>
							<option value="5">5</option>
							<option value="10">10</option>
							<option value="15">15</option>
							<option value="20">20</option>
							<option value="25">25</option>
						</select>
					</label>
				</div>
			</div>
		);
	}, [
		search,
		statusFilter,
		visibleColumns,
		totalCount,
		rowsPerPage,
		onRowsPerPageChange,
	]);

	const bottomContent = React.useMemo(() => {
		return (
			<div className="py-2 px-2 flex justify-between items-center">
				<span className="w-[30%] text-small text-default-400">
					{selectedKeys === "all"
						? "All items selected"
						: `${selectedKeys.size} of ${claims.length} selected`}
				</span>
				<Pagination
					isCompact
					showControls
					showShadow
					color="primary"
					page={page}
					total={pages}
					onChange={setPage}
				/>
				<div className="hidden sm:flex w-[30%] justify-end gap-2">
					<Button
						isDisabled={page === 1}
						size="sm"
						variant="flat"
						onPress={() => setPage(Math.max(1, page - 1))}
					>
						Previous
					</Button>
					<Button
						isDisabled={page === pages}
						size="sm"
						variant="flat"
						onPress={() => setPage(Math.min(pages, page + 1))}
					>
						Next
					</Button>
				</div>
			</div>
		);
	}, [selectedKeys, page, pages, claims.length]);

	if (isLoading) {
		return (
			<div className="flex items-center justify-center h-40">
				<Spinner label="Loading claims..." />
			</div>
		);
	}

	if (error) {
		return (
			<div className="flex flex-col items-center justify-center h-40">
				<AlertCircle className="w-8 h-8 text-danger mb-2" />
				<p className="text-danger">Failed to load claims</p>
				<Button size="sm" variant="flat" onPress={() => refetch()}>
					Retry
				</Button>
			</div>
		);
	}

	return (
		<Table
			aria-label="Claims table"
			isHeaderSticky
			bottomContent={bottomContent}
			bottomContentPlacement="outside"
			classNames={{
				wrapper: "min-h-[520px] max-h-[calc(100vh-500px)]",
			}}
			selectedKeys={selectedKeys}
			selectionMode="multiple"
			sortDescriptor={undefined}
			topContent={topContent}
			topContentPlacement="outside"
			onSelectionChange={setSelectedKeys}
		>
			<TableHeader columns={headerColumns}>
				{(column) => (
					<TableColumn
						key={column.uid}
						align={column.uid === "actions" ? "center" : "start"}
						allowsSorting={column.sortable}
					>
						{column.name}
					</TableColumn>
				)}
			</TableHeader>
			<TableBody emptyContent={"No claims found"} items={claims}>
				{(item) => (
					<TableRow
						key={item.id}
						className="cursor-pointer hover:bg-default-100"
						onClick={() => handleClaimClick(item.id)}
					>
						{(columnKey) => (
							<TableCell>{renderCell(item, columnKey)}</TableCell>
						)}
					</TableRow>
				)}
			</TableBody>
		</Table>
	);
}
