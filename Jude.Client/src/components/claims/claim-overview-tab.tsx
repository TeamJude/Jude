import type { GetClaimDetailResponse, ClaimTotalValues, ServiceResponse, ProductResponse } from "@/lib/types/claim";
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

interface ClaimOverviewTabProps {
  claim: GetClaimDetailResponse;
}

const AmountList: React.FC<{ values?: ClaimTotalValues; currency?: string }> = ({ values, currency }) => {
  if (!values) return null;
  const c = currency || "";
  const format = (v?: string) => (v ? `${c}${v}` : "-");
  return (
    <div className="grid grid-cols-2 md:grid-cols-3 gap-x-6 gap-y-2 text-sm">
      <div className="flex justify-between"><span className="text-foreground-500">Claimed</span><span>{format(values.claimed)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Copayment</span><span>{format(values.copayment)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Scheme Amount</span><span>{format(values.schemeAmount)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Savings Amount</span><span>{format(values.savingsAmount)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Nett Member</span><span>{format(values.nettMember)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Nett Provider</span><span>{format(values.nettProvider)}</span></div>
    </div>
  );
};

export const ClaimOverviewTab: React.FC<ClaimOverviewTabProps> = ({ claim }) => {
  const data = claim.data;
  console.log(data);
  const currency = data?.member?.currency || "";
console.log(data.transactionResponse);

  return (
    <Card shadow="none">
      <CardBody className="space-y-6">
        <div>
          <h3 className="text-lg font-medium mb-3">Transaction</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
            <div className="space-y-2 bg-content2 rounded-md p-3">
              <div className="flex justify-between"><span className="text-foreground-500">Type</span><span>{data?.transactionResponse?.type || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Number</span><span>{data?.transactionResponse?.number || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Claim #</span><span>{data?.transactionResponse?.claimNumber || "-"}</span></div>
            </div>
            <div className="space-y-2 bg-content2 rounded-md p-3">
              <div className="flex justify-between"><span className="text-foreground-500">Submitted By</span><span>{data?.transactionResponse?.submittedBy || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Date/Time</span><span>{data?.transactionResponse?.dateTime ? new Date(data.transactionResponse.dateTime).toLocaleString() : "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Reversed</span><span>{data?.transactionResponse?.reversed ? "Yes" : "No"}</span></div>
            </div>
            <div className="space-y-2 bg-content2 rounded-md p-3">
              <div className="flex justify-between"><span className="text-foreground-500">Date Reversed</span><span>{data?.transactionResponse?.dateReversed ? new Date(data.transactionResponse.dateReversed).toLocaleString() : "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Response Code</span><span>{data?.claimHeaderResponse?.responseCode || "-"}</span></div>
            </div>
          </div>
        </div>

        <Divider />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <h3 className="text-lg font-medium mb-3">Member</h3>
            <div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
              <div className="flex justify-between"><span className="text-foreground-500">Scheme #</span><span>{data?.member?.medicalSchemeNumber ?? "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Scheme Name</span><span>{data?.member?.medicalSchemeName || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Currency</span><span>{currency || "-"}</span></div>
            </div>
          </div>

          <div>
            <h3 className="text-lg font-medium mb-3">Patient</h3>
            <div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
              <div className="flex justify-between"><span className="text-foreground-500">Dependant Code</span><span>{data?.patient?.dependantCode ?? "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">First Name</span><span>{data?.patient?.personal?.firstName || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Surname</span><span>{data?.patient?.personal?.surname || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Initials</span><span>{data?.patient?.personal?.initials || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Gender</span><span>{data?.patient?.personal?.gender || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">DOB</span><span>{data?.patient?.personal?.dateOfBirth ? new Date(data.patient.personal.dateOfBirth).toLocaleDateString() : "-"}</span></div>
            </div>
          </div>
        </div>

        <Divider />

        <div>
          <div className="flex items-center gap-2 mb-3">
            <h3 className="text-lg font-medium">Totals</h3>
            <Chip size="sm" variant="flat" color="default">Overall</Chip>
          </div>
          <AmountList values={data?.claimHeaderResponse?.totalValues} currency={currency} />
        </div>

        <Divider />

        <div>
          <h3 className="text-lg font-medium mb-3">Services</h3>
          {data?.serviceResponse && data.serviceResponse.length > 0 ? (
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
                {data.serviceResponse.map((s: ServiceResponse, idx: number) => (
                  <TableRow key={`${s.number}-${idx}`}>
                    <TableCell>{s.number}</TableCell>
                    <TableCell>{s.code}</TableCell>
                    <TableCell>{s.description}</TableCell>
                    <TableCell>{currency}{s.subTotalValues?.claimed || "-"}</TableCell>
                    <TableCell>{currency}{s.totalValues?.claimed || "-"}</TableCell>
                    <TableCell>
                      {s.message?.type ? (
                        <div className="text-xs">
                          <div className="font-medium">{s.message.type}{s.message.code ? ` (${s.message.code})` : ""}</div>
                          <div className="text-foreground-500">{s.message.description || ""}</div>
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
          {data?.productResponse && data.productResponse.length > 0 ? (
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
                {data.productResponse.map((p: ProductResponse, idx: number) => (
                  <TableRow key={`${p.number}-${idx}`}>
                    <TableCell>{p.number}</TableCell>
                    <TableCell>{p.code}</TableCell>
                    <TableCell>{p.description}</TableCell>
                    <TableCell>{currency}{p.subTotalValues?.claimed || "-"}</TableCell>
                    <TableCell>{currency}{p.totalValues?.claimed || "-"}</TableCell>
                    <TableCell>
                      {p.message?.type ? (
                        <div className="text-xs">
                          <div className="font-medium">{p.message.type}{p.message.code ? ` (${p.message.code})` : ""}</div>
                          <div className="text-foreground-500">{p.message.description || ""}</div>
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


