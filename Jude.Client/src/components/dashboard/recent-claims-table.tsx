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
  type Selection,
  type ChipProps,
  type SortDescriptor,
} from "@heroui/react";
import { ChevronDown, EllipsisVertical, Plus, Search } from "lucide-react";

const columns = [
  { name: "CLAIM ID", uid: "id", sortable: true },
  { name: "PROVIDER", uid: "provider", sortable: true },
  { name: "MEMBER", uid: "member", sortable: true },
  { name: "AMOUNT", uid: "amount", sortable: true },
  { name: "STATUS", uid: "status", sortable: true },
  { name: "FLAG", uid: "flag" },
  { name: "ACTIONS", uid: "actions" },
];

const statusOptions = [
  { name: "Auto-Approved", uid: "auto-approved" },
  { name: "Under Review", uid: "under-review" },
  { name: "Pending", uid: "pending" },
  { name: "Flagged", uid: "flagged" },
];

const claims = [
  {
    id: "CL-78291",
    provider: "City Medical Center",
    member: "J. Smith",
    amount: "$3,450.00",
    status: "under-review",
    flag: "High Value",
    avatar: "https://i.pravatar.cc/150?u=a042581f4e29026024d",
  },
  {
    id: "CL-78302",
    provider: "Wellness Pharmacy",
    member: "R. Johnson",
    amount: "$890.25",
    status: "auto-approved",
    flag: null,
    avatar: "https://i.pravatar.cc/150?u=a042581f4e29026704d",
  },
  {
    id: "CL-78315",
    provider: "Metro Dental",
    member: "A. Williams",
    amount: "$1,245.00",
    status: "pending",
    flag: null,
    avatar: "https://i.pravatar.cc/150?u=a04258114e29026702d",
  },
  {
    id: "CL-78326",
    provider: "Central Hospital",
    member: "E. Brown",
    amount: "$6,780.50",
    status: "flagged",
    flag: "Unusual Pattern",
    avatar: "https://i.pravatar.cc/150?u=a048581f4e29026701d",
  },
  {
    id: "CL-78337",
    provider: "Family Practice",
    member: "M. Davis",
    amount: "$425.75",
    status: "auto-approved",
    flag: null,
    avatar: "https://i.pravatar.cc/150?u=a092581d4ef9026700d",
  },
];

const capitalize = (s: string) => {
  if (s.length === 0) return s;
  return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
};

const statusColorMap: Record<string, ChipProps["color"]> = {
  "auto-approved": "success",
  "under-review": "warning",
  pending: "default",
  flagged: "danger",
};

const INITIAL_VISIBLE_COLUMNS = ["id", "provider", "member", "amount", "status", "flag", "actions"];

type Claim = (typeof claims)[0];

export function RecentClaimsTable() {
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
        claim.provider.toLowerCase().includes(filterValue.toLowerCase()) ||
        claim.member.toLowerCase().includes(filterValue.toLowerCase())
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
      const first = a[sortDescriptor.column as keyof Claim] ?? '';
      const second = b[sortDescriptor.column as keyof Claim] ?? '';
      const cmp = first < second ? -1 : first > second ? 1 : 0;
      return sortDescriptor.direction === "descending" ? -cmp : cmp;
    });
  }, [sortDescriptor, items]);

  const renderCell = React.useCallback((claim: Claim, columnKey: React.Key) => {
    const cellValue = claim[columnKey as keyof Claim];

    switch (columnKey) {
      case "provider":
        return (
          <User
            avatarProps={{ radius: "full", size: "sm", src: claim.avatar }}
            name={cellValue}
          >
            {claim.provider}
          </User>
        );
      case "status":
        return (
          <Chip
            className="capitalize border-none gap-1 text-default-600"
            color={statusColorMap[claim.status]}
            size="sm"
            variant="dot"
          >
            {capitalize(claim.status)}
          </Chip>
        );
      case "flag":
        return claim.flag ? (
          <Chip
            className="capitalize"
            color="danger"
            size="sm"
            variant="flat"
          >
            {claim.flag}
          </Chip>
        ) : null;
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
                <DropdownItem key="view">View Details</DropdownItem>
                <DropdownItem key="edit">Edit Claim</DropdownItem>
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
  }, []);

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
            placeholder="Search by claim ID, provider, or member..."
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

  return (
    <Table
      isCompact
      removeWrapper
      aria-label="Recent claims table with custom cells, pagination and sorting"
      bottomContentPlacement="outside"
      checkboxesProps={{
        classNames: {
          wrapper: "after:bg-foreground after:text-background text-background ",
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
      <TableBody emptyContent={"No claims found"} items={sortedItems}>
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