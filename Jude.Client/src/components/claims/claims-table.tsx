import { getClaims } from "@/lib/services/claims.service";
import { ClaimStatus, type ClaimSummary, FraudRiskLevel } from "@/lib/types/claim";
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
    TableRow
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
    XCircle
} from "lucide-react";
import React from "react";

const columns = [
    { name: "CLAIM ID", uid: "id", sortable: true },
    { name: "PATIENT", uid: "patient", sortable: true },
    { name: "AMOUNT", uid: "amount", sortable: true },
    { name: "RISK", uid: "risk", sortable: true },
    { name: "ACTIONS", uid: "actions" },
];

const statusOptions = [
    { name: "Pending", uid: ClaimStatus.Pending },
    { name: "Processing", uid: ClaimStatus.Processing },
    { name: "Failed", uid: ClaimStatus.Failed },
    { name: "Review", uid: ClaimStatus.Review },
    { name: "Completed", uid: ClaimStatus.Completed },
];

const riskOptions = [
    { name: "Low", uid: FraudRiskLevel.Low },
    { name: "Medium", uid: FraudRiskLevel.Medium },
    { name: "High", uid: FraudRiskLevel.High },
    { name: "Critical", uid: FraudRiskLevel.Critical },
];

const capitalize = (s: string) => {
    if (s.length === 0) return s;
    return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
};

const statusColorMap: Record<ClaimStatus, ChipProps["color"]> = {
    [ClaimStatus.Pending]: "default",
    [ClaimStatus.Processing]: "primary",
    [ClaimStatus.Failed]: "danger",
    [ClaimStatus.Review]: "warning",
    [ClaimStatus.Completed]: "success",
};

const riskColorMap: Record<FraudRiskLevel, ChipProps["color"]> = {
    [FraudRiskLevel.Low]: "success",
    [FraudRiskLevel.Medium]: "warning",
    [FraudRiskLevel.High]: "danger",
    [FraudRiskLevel.Critical]: "danger",
};

const getStatusIcon = (status: ClaimStatus) => {
    switch (status) {
        case ClaimStatus.Completed:
            return <CheckCircle className="w-4 h-4" />;
        case ClaimStatus.Failed:
            return <XCircle className="w-4 h-4" />;
        case ClaimStatus.Processing:
            return <Clock className="w-4 h-4" />;
        case ClaimStatus.Review:
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
    "risk",
    "actions",
];

export function ClaimsTable() {
    const [search, setSearch] = React.useState("");
    const [selectedKeys, setSelectedKeys] = React.useState<Selection>(new Set([]));
    const [visibleColumns, setVisibleColumns] = React.useState<Selection>(new Set(INITIAL_VISIBLE_COLUMNS));
    const [statusFilter, setStatusFilter] = React.useState<Selection>("all");
    const [riskFilter, setRiskFilter] = React.useState<Selection>("all");
    const [rowsPerPage, setRowsPerPage] = React.useState(10);
    const [page, setPage] = React.useState(1);
    const navigate = useNavigate();    
    
    const {
        data: claimsResponse,
        isLoading,
        error,
        refetch,
    } = useQuery({
        queryKey: ["claims", search, rowsPerPage, page],
        queryFn: () => getClaims({
        page,
        pageSize: rowsPerPage,
        search: search || undefined,
        status: statusFilter !== "all" && statusFilter.size > 0 ? Array.from(statusFilter).map(key => Number(key)) as ClaimStatus[] : undefined,
        riskLevel: riskFilter !== "all" && riskFilter.size > 0 ? Array.from(riskFilter).map(key => Number(key)) as FraudRiskLevel[] : undefined,
    }),
    });    

    const claims = claimsResponse?.success ? claimsResponse.data.claims : [];
    const totalCount = claimsResponse?.success ? claimsResponse.data.totalCount : 0;
    const pages = Math.ceil(totalCount / rowsPerPage);

    const headerColumns = React.useMemo(() => {
        if (visibleColumns === "all") return columns;
        return columns.filter((column) =>
            Array.from(visibleColumns).includes(column.uid),
        );
    }, [visibleColumns]);

    const onSearchChange = React.useCallback((value?: string) => {
        if (value) {
            setSearch(value);
            setPage(1);
        } else {
            setSearch("");
        }
    }, []);

    const onClear = React.useCallback(() => {
        setSearch("");
        setPage(1);
    }, []);

    const onRowsPerPageChange = React.useCallback(
        (e: React.ChangeEvent<HTMLSelectElement>) => {
            setRowsPerPage(Number(e.target.value));
            setPage(1);
        },
        [],
    );

    const handleClaimClick = (claimId: string) => {
        navigate({ to: `/claims/${claimId}` });
    };    const renderCell = React.useCallback((claim: ClaimSummary, columnKey: React.Key) => {
        const cellValue = claim[columnKey as keyof ClaimSummary];

        switch (columnKey) {
            case "id":
                return (
                    <div className="flex items-center gap-2">
                        <span className="text-small font-mono">{claim.transactionNumber}</span>
                    </div>
                );
            case "patient":
                return (
                    <div className="flex flex-col">
                        <p className="text-bold text-small capitalize">{claim.patientName}</p>
                        <p className="text-bold text-tiny capitalize text-default-500">
                            {claim.membershipNumber}
                        </p>
                    </div>
                );
            case "amount":
                return (
                    <div className="flex flex-col">
                        <span className="font-medium">{claim.currency}{claim.claimAmount.toLocaleString()}</span>
                        {claim.approvedAmount ? (
                            <span className="text-tiny text-success">
                                Approved: {claim.currency}{claim.approvedAmount.toLocaleString()}
                            </span>
                        ) : null}
                    </div>
                );
            case "status":
                return (
                    <Chip
                        className="capitalize border-none gap-1 text-default-600"
                        color={statusColorMap[claim.status]}
                        size="sm"
                        variant="dot"
                        startContent={getStatusIcon(claim.status)}
                    >
                        {ClaimStatus[claim.status]}
                    </Chip>
                );            
                case "risk":
                return (
                    <Chip
                        className="capitalize border-none gap-1 text-default-600"
                        color={riskColorMap[claim.fraudRiskLevel]}
                        size="sm"
                        variant="flat"
                    >
                        {FraudRiskLevel[claim.fraudRiskLevel]}
                        {claim.isFlagged && " 🚩"}
                    </Chip>
                );
            case "actions":
                return (
                    <div className="relative flex justify-end items-center gap-2">
                        <Dropdown>
                            <DropdownTrigger>
                                <Button isIconOnly size="sm" variant="light">
                                    <EllipsisVertical className="w-4 h-4" />
                                </Button>
                            </DropdownTrigger>
                            <DropdownMenu>
                                <DropdownItem key="view" onPress={() => handleClaimClick(claim.id)}>
                                    View Details
                                </DropdownItem>
                                <DropdownItem key="edit">Edit</DropdownItem>
                                <DropdownItem key="delete" className="text-danger" color="danger">
                                    Delete
                                </DropdownItem>
                            </DropdownMenu>
                        </Dropdown>
                    </div>
                );
            default:
                return cellValue as React.ReactNode;
        }
    }, [navigate]);

    const topContent = React.useMemo(() => {
        return (
            <div className="flex flex-col gap-4 pb-4">
                <div className="flex justify-between gap-3 items-end">
                    <Input
                        isClearable
                        className="w-full sm:max-w-[44%]"
                        placeholder="Search by claim ID, patient, or provider..."
                        startContent={<Search className="text-default-300" />}
                        value={search}
                        variant="bordered"
                        onClear={onClear}
                        onValueChange={onSearchChange}
                    />
                    <div className="flex gap-3">
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
                                    Risk Level
                                </Button>
                            </DropdownTrigger>
                            <DropdownMenu
                                disallowEmptySelection
                                aria-label="Risk Filter"
                                closeOnSelect={false}
                                selectedKeys={riskFilter}
                                selectionMode="multiple"
                                onSelectionChange={setRiskFilter}
                            >
                                {riskOptions.map((risk) => (
                                    <DropdownItem key={risk.uid} className="capitalize">
                                        {risk.name}
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
                                        {capitalize(column.name)}
                                    </DropdownItem>
                                ))}
                            </DropdownMenu>
                        </Dropdown>
                
                    </div>
                </div>
            </div>
        );
    }, [
        search,
        statusFilter,
        riskFilter,
        visibleColumns,
        onSearchChange,
        onClear,
        onRowsPerPageChange,
        totalCount,
        rowsPerPage,
    ]);

    const bottomContent = React.useMemo(() => {
        return (
            <div className="py-2 px-2 flex justify-between items-center">
                <span className="w-[30%] text-small text-default-400">
                    {selectedKeys === "all"
                        ? "All items selected"
                        : `${selectedKeys.size} of ${totalCount} selected `}
                        ({totalCount} total claims)
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
                   <div className="flex justify-between items-center">
                    <label className="flex items-center text-default-400 text-small gap-2">
                        <span>
                        Rows per page: 
                        </span>
                        <select
                            className="bg-transparent outline-none text-default-400 text-small"
                            onChange={onRowsPerPageChange}
                            value={rowsPerPage}
                        >
                            <option value="5">5</option>
                            <option value="10">10</option>
                            <option value="15">15</option>
                            <option value="20">20</option>
                        </select>
                    </label>
                </div>
                </div>
            </div>
        );
    }, [selectedKeys, totalCount, page, pages]);

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center p-8">
                <AlertCircle className="w-8 h-8 text-danger mb-2" />
                <p className="text-danger">Failed to load claims</p>
                <Button size="sm" color="primary" variant="flat" onPress={() => refetch()} className="mt-2">
                    Retry
                </Button>
            </div>
        );
    }    return (
        <div className="flex flex-col h-full w-full">
            <Table
            aria-label="Claims table"
            bottomContent={bottomContent}
            className="h-full"
            bottomContentPlacement="outside"            
            classNames={{
                wrapper: "max-h-[calc(100vh-300px)] h-full overflow-y-scroll",
                base: "flex flex-col h-full",
                table: "min-h-0",
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
            <TableBody 
                emptyContent="No claims found" 
                items={claims}
                isLoading={isLoading}
                loadingContent={<Spinner label="Loading claims..." />}
            >
                {(item) => (
                    <TableRow key={item.id}>
                        {(columnKey) => <TableCell>{renderCell(item, columnKey)}</TableCell>}
                    </TableRow>
                )}           
                 </TableBody>
        </Table>
        </div>
    );
}
