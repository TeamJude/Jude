import type { GetClaimDetailResponse, ClaimTotalValues, ServiceResponse, ProductResponse, ClaimSource } from "@/lib/types/claim";
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

const AmountList: React.FC<{ values?: ClaimTotalValues; currency?: string }> = ({ values, currency }) => {
  if (!values) return null;
  const c = currency || "";
  const format = (v?: string) => (v ? `${c}${v}` : "-");
  return (
    <div className="grid grid-cols-2 md:grid-cols-3 gap-x-6 gap-y-2 text-sm">
      <div className="flex justify-between"><span className="text-foreground-500">Claimed</span><span>{format(values.Claimed)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Copayment</span><span>{format(values.Copayment)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Scheme Amount</span><span>{format(values.SchemeAmount)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Savings Amount</span><span>{format(values.SavingsAmount)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Nett Member</span><span>{format(values.NettMember)}</span></div>
      <div className="flex justify-between"><span className="text-foreground-500">Nett Provider</span><span>{format(values.NettProvider)}</span></div>
    </div>
  );
};

export const ClaimOverviewTab: React.FC<ClaimOverviewTabProps> = ({ claim }) => {
  const data = claim.data;
  const currency = data?.Member?.Currency || "";
  
  // Check if this is an uploaded claim
  const isUploadedClaim = claim.source === 1; // 1 = Upload enum value
  


  // If it's an uploaded claim, show the markdown content
  if (isUploadedClaim && claim.claimMarkdown) {
    return (
      <Card shadow="none">
        <CardBody className="space-y-6">
          <div>
            <div className="flex items-center gap-2 mb-3">
              <h3 className="text-lg font-medium">Uploaded Claim Data</h3>
              <Chip color="primary" variant="flat" size="sm">
                Uploaded PDF
              </Chip>
            </div>
            <div className="bg-content2 p-4 rounded-md">
              <Markdown>{claim.claimMarkdown}</Markdown>
            </div>
          </div>
        </CardBody>
      </Card>
    );
  }

  return (
    <Card shadow="none">
      <CardBody className="space-y-6">
        <div>
          <div className="flex items-center gap-2 mb-4">
            <h2 className="text-xl font-semibold">CIMAS API Data</h2>
            <Chip color="secondary" variant="flat" size="sm">
              Structured Data
            </Chip>
          </div>
        </div>
        
        <div>
          <h3 className="text-lg font-medium mb-3">Transaction</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
            <div className="space-y-2 bg-content2 rounded-md p-3">
              <div className="flex justify-between"><span className="text-foreground-500">Type</span><span>{data?.TransactionResponse?.Type || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Number</span><span>{data?.TransactionResponse?.Number || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Claim #</span><span>{data?.TransactionResponse?.ClaimNumber || "-"}</span></div>
            </div>
            <div className="space-y-2 bg-content2 rounded-md p-3">
              <div className="flex justify-between"><span className="text-foreground-500">Submitted By</span><span>{data?.TransactionResponse?.SubmittedBy || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Date/Time</span><span>{data?.TransactionResponse?.DateTime ? new Date(data.TransactionResponse.DateTime).toLocaleString() : "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Reversed</span><span>{data?.TransactionResponse?.Reversed ? "Yes" : "No"}</span></div>
            </div>
            <div className="space-y-2 bg-content2 rounded-md p-3">
              <div className="flex justify-between"><span className="text-foreground-500">Date Reversed</span><span>{data?.TransactionResponse?.DateReversed ? new Date(data.TransactionResponse.DateReversed).toLocaleString() : "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Response Code</span><span>{data?.ClaimHeaderResponse?.ResponseCode || "-"}</span></div>
            </div>
          </div>
        </div>

        <Divider />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <h3 className="text-lg font-medium mb-3">Member</h3>
            <div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
              <div className="flex justify-between"><span className="text-foreground-500">Scheme #</span><span>{data?.Member?.MedicalSchemeNumber ?? "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Scheme Name</span><span>{data?.Member?.MedicalSchemeName || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Currency</span><span>{currency || "-"}</span></div>
            </div>
          </div>

          <div>
            <h3 className="text-lg font-medium mb-3">Patient</h3>
            <div className="space-y-2 bg-content2 rounded-md p-4 text-sm">
              <div className="flex justify-between"><span className="text-foreground-500">Dependant Code</span><span>{data?.Patient?.DependantCode ?? "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">First Name</span><span>{data?.Patient?.Personal?.FirstName || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Surname</span><span>{data?.Patient?.Personal?.Surname || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Initials</span><span>{data?.Patient?.Personal?.Initials || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">Gender</span><span>{data?.Patient?.Personal?.Gender || "-"}</span></div>
              <div className="flex justify-between"><span className="text-foreground-500">DOB</span><span>{data?.Patient?.Personal?.DateOfBirth ? new Date(data.Patient.Personal.DateOfBirth).toLocaleDateString() : "-"}</span></div>
            </div>
          </div>
        </div>

        <Divider />

        <div>
          <div className="flex items-center gap-2 mb-3">
            <h3 className="text-lg font-medium">Totals</h3>
            <Chip size="sm" variant="flat" color="default">Overall</Chip>
          </div>
          <AmountList values={data?.ClaimHeaderResponse?.TotalValues} currency={currency} />
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
                    <TableCell>{currency}{s.SubTotalValues?.Claimed || "-"}</TableCell>
                    <TableCell>{currency}{s.TotalValues?.Claimed || "-"}</TableCell>
                    <TableCell>
                       {s.Message?.Type ? (
                        <div className="text-xs">
                           <div className="font-medium">{s.Message.Type}{s.Message.Code ? ` (${s.Message.Code})` : ""}</div>
                           <div className="text-foreground-500">{s.Message.Description || ""}</div>
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
                    <TableCell>{currency}{p.SubTotalValues?.Claimed || "-"}</TableCell>
                    <TableCell>{currency}{p.TotalValues?.Claimed || "-"}</TableCell>
                    <TableCell>
                       {p.Message?.Type ? (
                        <div className="text-xs">
                           <div className="font-medium">{p.Message.Type}{p.Message.Code ? ` (${p.Message.Code})` : ""}</div>
                           <div className="text-foreground-500">{p.Message.Description || ""}</div>
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


