"use client";

import React from "react";
import {
  Table,
  TableHeader,
  TableColumn,
  TableBody,
  TableRow,
  TableCell,
  Input,
  Button,
  DropdownTrigger,
  Dropdown,
  DropdownMenu,
  DropdownItem,
  Chip,
  User,
  Pagination,
  type Selection,
  type ChipProps,
  type SortDescriptor,
  Tooltip,
} from "@heroui/react";
import { ChevronDown, EllipsisVertical, Plus, Search, AlertCircle, Clock, CheckCircle, XCircle } from "lucide-react";
import { useNavigate } from "@tanstack/react-router";

const columns = [
  { name: "CLAIM ID", uid: "id", sortable: true },
  { name: "MEMBER", uid: "member", sortable: true },
  { name: "PROVIDER", uid: "provider", sortable: true },
  { name: "AMOUNT", uid: "amount", sortable: true },
  { name: "DATES", uid: "dates", sortable: true },
  { name: "STATUS", uid: "status", sortable: true },
  { name: "RISK", uid: "risk", sortable: true },
  { name: "SOURCE", uid: "source", sortable: true },
  { name: "ACTIONS", uid: "actions" },
];

const statusOptions = [
  { name: "New", uid: "new" },
  { name: "Pending Human Review", uid: "pending-human-review" },
  { name: "Agent Processing", uid: "agent-processing" },
  { name: "Approved", uid: "approved" },
  { name: "Rejected", uid: "rejected" },
  { name: "Flagged for Investigation", uid: "flagged" },
  { name: "Pending External Info", uid: "pending-external" },
];

const claims = [
  {
    id: 'CL-001',
    memberId: 'MEM-12345',
    memberName: 'John Smith',
    providerId: 'PRV-28765',
    providerName: 'City Medical Center',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-10',
    amount: 1250,
    status: 'pending-human-review',
    source: 'Portal',
    riskScore: 15,
    assignedReviewer: 'Sarah Johnson',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a042581f4e29026024d',
  },
  {
    id: 'CL-002',
    memberId: 'MEM-23456',
    memberName: 'Emily Davis',
    providerId: 'PRV-34521',
    providerName: 'Wellness Family Practice',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-08',
    amount: 450,
    status: 'approved',
    source: 'Email',
    riskScore: 5,
    assignedReviewer: '',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a042581f4e29026704d',
  },
  {
    id: 'CL-003',
    memberId: 'MEM-34567',
    memberName: 'Michael Brown',
    providerId: 'PRV-12398',
    providerName: 'Northside Hospital',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-12',
    amount: 3200,
    status: 'pending-human-review',
    source: 'Portal',
    riskScore: 65,
    assignedReviewer: '',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a04258114e29026702d',
  },
  {
    id: 'CL-004',
    memberId: 'MEM-45678',
    memberName: 'Jessica Wilson',
    providerId: 'PRV-56789',
    providerName: 'Downtown Medical Associates',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-09',
    amount: 850,
    status: 'agent-processing',
    source: 'API',
    riskScore: 25,
    assignedReviewer: '',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a048581f4e29026701d',
  },
  {
    id: 'CL-005',
    memberId: 'MEM-56789',
    memberName: 'Robert Taylor',
    providerId: 'PRV-34567',
    providerName: 'Eastside Specialists',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-11',
    amount: 1800,
    status: 'rejected',
    source: 'Email',
    riskScore: 85,
    assignedReviewer: 'David Miller',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a092581d4ef9026700d',
  },
  {
    id: 'CL-006',
    memberId: 'MEM-67890',
    memberName: 'Sarah Anderson',
    providerId: 'PRV-45678',
    providerName: 'Community Health Partners',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-07',
    amount: 550,
    status: 'new',
    source: 'Manual',
    riskScore: 10,
    assignedReviewer: '',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a042581f4e29026024d',
  },
  {
    id: 'CL-007',
    memberId: 'MEM-78901',
    memberName: 'Thomas Johnson',
    providerId: 'PRV-67890',
    providerName: 'Valley Medical Group',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-10',
    amount: 2100,
    status: 'flagged',
    source: 'Portal',
    riskScore: 92,
    assignedReviewer: 'Lisa Wong',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a042581f4e29026704d',
  },
  {
    id: 'CL-008',
    memberId: 'MEM-89012',
    memberName: 'Jennifer Martinez',
    providerId: 'PRV-78901',
    providerName: 'Riverside Clinic',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-08',
    amount: 375,
    status: 'approved',
    source: 'API',
    riskScore: 8,
    assignedReviewer: '',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a04258114e29026702d',
  },
  {
    id: 'CL-009',
    memberId: 'MEM-90123',
    memberName: 'Daniel Garcia',
    providerId: 'PRV-89012',
    providerName: 'Metro Health Services',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-12',
    amount: 920,
    status: 'pending-external',
    source: 'Email',
    riskScore: 35,
    assignedReviewer: 'Mark Thompson',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a048581f4e29026701d',
  },
  {
    id: 'CL-010',
    memberId: 'MEM-01234',
    memberName: 'Amanda Lee',
    providerId: 'PRV-90123',
    providerName: 'Sunshine Pediatrics',
    dateReceived: '2023-05-15',
    dateOfService: '2023-05-09',
    amount: 680,
    status: 'approved',
    source: 'Portal',
    riskScore: 12,
    assignedReviewer: '',
    lastAction: '2023-05-15',
    avatar: 'https://i.pravatar.cc/150?u=a092581d4ef9026700d',
  },
];

const capitalize = (s: string) => {
  if (s.length === 0) return s;
  return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
};

const statusColorMap: Record<string, ChipProps["color"]> = {
  "new": "default",
  "pending-human-review": "warning",
  "agent-processing": "primary",
  "approved": "success",
  "rejected": "danger",
  "flagged": "danger",
  "pending-external": "warning",
};

const getStatusIcon = (status: string) => {
  switch (status) {
    case "approved":
      return <CheckCircle className="w-4 h-4" />;
    case "rejected":
      return <XCircle className="w-4 h-4" />;
    case "flagged":
      return <AlertCircle className="w-4 h-4" />;
    default:
      return <Clock className="w-4 h-4" />;
  }
};

const getRiskColor = (score: number): ChipProps["color"] => {
  if (score >= 80) return "danger";
  if (score >= 50) return "warning";
  if (score >= 20) return "primary";
  return "success";
};

const INITIAL_VISIBLE_COLUMNS = ["id", "member", "provider", "amount", "dates", "status", "risk", "source", "actions"];

type Claim = (typeof claims)[0];

export function   ClaimsTable() {
  const [filterValue, setFilterValue] = React.useState("");
  const [selectedKeys, setSelectedKeys] = React.useState<Selection>(new Set([]));
  const [visibleColumns, setVisibleColumns] = React.useState<Selection>(new Set(INITIAL_VISIBLE_COLUMNS));
  const [statusFilter, setStatusFilter] = React.useState<Selection>("all");
  const [rowsPerPage, setRowsPerPage] = React.useState(5);
  const [sortDescriptor, setSortDescriptor] = React.useState<SortDescriptor>({
    column: "id",
    direction: "ascending",
  });
  const [page, setPage] = React.useState(1);
  const navigate = useNavigate();

  const pages = Math.ceil(claims.length / rowsPerPage);

  const hasSearchFilter = Boolean(filterValue);

  const headerColumns = React.useMemo(() => {
    if (visibleColumns === "all") return columns;
    return columns.filter((column) => Array.from(visibleColumns).includes(column.uid));
  }, [visibleColumns]);

  const filteredItems = React.useMemo(() => {
    let filteredClaims = [...claims];

    if (hasSearchFilter) {
      filteredClaims = filteredClaims.filter((claim) =>
        claim.id.toLowerCase().includes(filterValue.toLowerCase()) ||
        claim.memberName.toLowerCase().includes(filterValue.toLowerCase()) ||
        claim.providerName.toLowerCase().includes(filterValue.toLowerCase())
      );
    }
    if (statusFilter !== "all" && Array.from(statusFilter).length !== statusOptions.length) {
      filteredClaims = filteredClaims.filter((claim) =>
        Array.from(statusFilter).includes(claim.status)
      );
    }

    return filteredClaims;
  }, [hasSearchFilter, statusFilter, filterValue]);

  const items = React.useMemo(() => {
    const start = (page - 1) * rowsPerPage;
    const end = start + rowsPerPage;
    return filteredItems.slice(start, end);
  }, [page, filteredItems, rowsPerPage]);

  const sortedItems = React.useMemo(() => {
    return [...items].sort((a: Claim, b: Claim) => {
      const first = a[sortDescriptor.column as keyof Claim];
      const second = b[sortDescriptor.column as keyof Claim];
      const cmp = first < second ? -1 : first > second ? 1 : 0;
      return sortDescriptor.direction === "descending" ? -cmp : cmp;
    });
  }, [sortDescriptor, items]);

  const renderCell = React.useCallback((claim: Claim, columnKey: React.Key) => {
    const cellValue = claim[columnKey as keyof Claim];

    switch (columnKey) {
      case "member":
        return (
          <User
            avatarProps={{ radius: "full", size: "sm", src: claim.avatar }}
            name={claim.memberName}
            description={claim.memberId}
          >
            {claim.memberName}
          </User>
        );
      case "provider":
        return (
          <div className="flex flex-col">
            <p className="text-bold text-small capitalize">{claim.providerName}</p>
            <p className="text-bold text-tiny capitalize text-default-500">
              {claim.providerId}
            </p>
          </div>
        );
      case "amount":
        return (
          <div className="font-medium">
            ${claim.amount.toLocaleString()}
          </div>
        );
      case "dates":
        return (
          <div className="flex flex-col">
            <p className="text-small">Service: {claim.dateOfService}</p>
            <p className="text-tiny text-default-500">Received: {claim.dateReceived}</p>
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
            {capitalize(claim.status.replace(/-/g, ' '))}
          </Chip>
        );
      case "risk":
        return (
          <Tooltip content={`Risk Score: ${claim.riskScore}`}>
            <Chip
              className="capitalize"
              color={getRiskColor(claim.riskScore)}
              size="sm"
              variant="flat"
            >
              {claim.riskScore}
            </Chip>
          </Tooltip>
        );
      case "source":
        return (
          <Chip
            className="capitalize"
            color="default"
            size="sm"
            variant="flat"
          >
            {claim.source}
          </Chip>
        );
      case "actions":
        return (
          <div className="relative flex justify-end items-center gap-2">
            <Dropdown>
              <DropdownTrigger>
                <Button isIconOnly radius="full" size="sm" variant="light">
                  <EllipsisVertical />
                </Button>
              </DropdownTrigger>
              <DropdownMenu>
                <DropdownItem 
                  key="view" 
                  onPress={() => navigate({ to: '/claims/$id', params: { id: claim.id } })}
                >
                  View Details
                </DropdownItem>
                <DropdownItem key="edit">Edit Claim</DropdownItem>
                <DropdownItem key="assign">Assign Reviewer</DropdownItem>
                <DropdownItem key="delete" className="text-danger" color="danger">
                  Delete Claim
                </DropdownItem>
              </DropdownMenu>
            </Dropdown>
          </div>
        );
      default:
        return cellValue;
    }
  }, [navigate]);

  const onRowsPerPageChange = React.useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setRowsPerPage(Number(e.target.value));
    setPage(1);
  }, []);

  const onSearchChange = React.useCallback((value?: string) => {
    if (value) {
      setFilterValue(value);
      setPage(1);
    } else {
      setFilterValue("");
    }
  }, []);

  const topContent = React.useMemo(() => {
    return (
      <div className="flex flex-col gap-4">
        <div className="flex justify-between gap-3 items-end">
          <Input
            isClearable
            className="w-full sm:max-w-[44%]"
            placeholder="Search by claim ID, member, or provider..."
            size="sm"
            startContent={<Search className="text-default-300" />}
            value={filterValue}
            variant="bordered"
            onClear={() => setFilterValue("")}
            onValueChange={onSearchChange}
          />
          <div className="flex gap-3">
            <Dropdown>
              <DropdownTrigger className="hidden sm:flex">
                <Button
                  endContent={<ChevronDown className="text-small" />}
                  size="sm"
                  variant="flat"
                >
                  Status
                </Button>
              </DropdownTrigger>
              <DropdownMenu
                disallowEmptySelection
                aria-label="Table Columns"
                closeOnSelect={false}
                selectedKeys={statusFilter}
                selectionMode="multiple"
                onSelectionChange={setStatusFilter}
              >
                {statusOptions.map((status) => (
                  <DropdownItem key={status.uid} className="capitalize">
                    {capitalize(status.name)}
                  </DropdownItem>
                ))}
              </DropdownMenu>
            </Dropdown>
            <Dropdown>
              <DropdownTrigger className="hidden sm:flex">
                <Button
                  endContent={<ChevronDown className="text-small" />}
                  size="sm"
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
            <Button
              className="bg-foreground text-background"
              endContent={<Plus />}
              size="sm"
            >
              New Claim
            </Button>
          </div>
        </div>
        <div className="flex justify-between items-center">
          <span className="text-default-400 text-small">
            Total {claims.length} claims
          </span>
          <label className="flex items-center text-default-400 text-small">
            Rows per page:
            <select
              className="bg-transparent outline-none text-default-400 text-small"
              onChange={onRowsPerPageChange}
            >
              <option value="5">5</option>
              <option value="10">10</option>
              <option value="15">15</option>
            </select>
          </label>
        </div>
      </div>
    );
  }, [filterValue, statusFilter, visibleColumns, onSearchChange, onRowsPerPageChange]);

  const bottomContent = React.useMemo(() => {
    return (
      <div className="py-2 px-2 flex justify-between items-center">
        <Pagination
          showControls
          classNames={{
            cursor: "bg-foreground text-background",
          }}
          color="default"
          isDisabled={hasSearchFilter}
          page={page}
          total={pages}
          variant="light"
          onChange={setPage}
        />
        <span className="text-small text-default-400">
          {selectedKeys === "all"
            ? "All items selected"
            : `${selectedKeys.size} of ${items.length} selected`}
        </span>
      </div>
    );
  }, [selectedKeys, items.length, page, pages, hasSearchFilter]);

  return (
    <Table
      isCompact
      removeWrapper
      aria-label="Claims table with custom cells, pagination and sorting"
      bottomContent={bottomContent}
      bottomContentPlacement="outside"
      checkboxesProps={{

        classNames: {
          wrapper: "after:bg-foreground after:text-background text-background  ",
        },
      }}
      selectedKeys={selectedKeys}
      selectionMode="multiple"
      sortDescriptor={sortDescriptor}
      topContent={topContent}
      topContentPlacement="outside"
      onSelectionChange={setSelectedKeys}
      onSortChange={setSortDescriptor}
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
      <TableBody items={sortedItems}>
        {(item) => (
          <TableRow key={item.id}>
            {(columnKey) => (
              <TableCell>{renderCell(item, columnKey)}</TableCell>
            )}
          </TableRow>
        )}
      </TableBody>
    </Table>
  );
} 