import React from 'react';
import { Tabs, Tab, Card, CardBody, Divider, Chip, Progress, Button, Textarea } from '@heroui/react';
import { 
  ClipboardList, 
  FileText, 
  BookOpen, 
  CheckSquare, 
  History,
  CheckCircle,
  AlertCircle,
  Info,
  BarChart,
  UserCheck,
  Eye,
  Download,
  Check,
  Edit,
  Clock,
  X,
  Inbox,
  Cpu,
  User
} from 'lucide-react';


interface ClaimTabsProps {
  claimId: string;
}

export const ClaimTabs: React.FC<ClaimTabsProps> = ({ claimId }) => {
  const [selected, setSelected] = React.useState("summary");
  const [decision, setDecision] = React.useState("");
  const [notes, setNotes] = React.useState("");

  const handleSelectionChange = (key: React.Key) => {
    setSelected(key.toString());
  };

  return (
    <div className="flex w-full flex-col">
      <Tabs 
        aria-label="Claim details tabs" 
        selectedKey={selected} 
        onSelectionChange={handleSelectionChange}
        color="primary"
        variant="underlined"
        classNames={{
          tabList: "gap-6",
          cursor: "w-full bg-primary",
          tab: "max-w-fit px-0 h-12",
        }}
      >
        <Tab 
          key="summary" 
          title={
            <div className="flex items-center gap-2">
              <ClipboardList width={18} />
              <span>Claim Summary & Agent Output</span>
            </div>
          }
        >
          <Card shadow="none">
            <CardBody className="gap-6">
              <div>
                <h3 className="text-lg font-medium mb-3">Extracted Data</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
                  <div>
                    <h4 className="text-sm font-medium">Claim Details</h4>
                    <div className="mt-2 space-y-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Service Type</span>
                        <span className="text-sm">Consultation</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Service Code</span>
                        <span className="text-sm">CON-2023</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Diagnosis Code</span>
                        <span className="text-sm">J45.909</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Place of Service</span>
                        <span className="text-sm">Office</span>
                      </div>
                    </div>
                  </div>
                  
                  <div>
                    <h4 className="text-sm font-medium">Financial Details</h4>
                    <div className="mt-2 space-y-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Billed Amount</span>
                        <span className="text-sm">$1,250.00</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Allowed Amount</span>
                        <span className="text-sm">$950.00</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Member Responsibility</span>
                        <span className="text-sm">$150.00</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-foreground-500">Plan Payment</span>
                        <span className="text-sm">$800.00</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              <Divider />
              
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <h3 className="text-lg font-medium">Agent's Reasoning Log</h3>
                  <Chip color="secondary" variant="flat" size="sm">AI Generated</Chip>
                </div>
                
                <div className="bg-content2 p-4 rounded-md space-y-3">
                  <div className="flex gap-2">
                    <CheckCircle className="text-success mt-0.5" width={18} />
                    <div>
                      <p className="text-sm"><span className="font-medium">Checked Policy Document 'Medical Coverage v2.3':</span> Section 4.B - Service covered under standard consultation benefits.</p>
                    </div>
                  </div>
                  
                  <div className="flex gap-2">
                    <CheckCircle className="text-success mt-0.5" width={18} />
                    <div>
                      <p className="text-sm"><span className="font-medium">Applied Rule 'Provider Network Check':</span> Provider ID #PRV-28765 confirmed in-network for member's plan.</p>
                    </div>
                  </div>
                  
                  <div className="flex gap-2">
                    <AlertCircle className="text-warning mt-0.5" width={18} />
                    <div>
                      <p className="text-sm"><span className="font-medium">Applied Rule 'High Value Check':</span> Amount {'>'} $1,000 - Flagged for review.</p>
                    </div>
                  </div>
                  
                  <div className="flex gap-2">
                    <Info className="text-primary mt-0.5" width={18} />
                    <div>
                      <p className="text-sm"><span className="font-medium">Historical Data:</span> Member had similar claim #CL-2023-45678 approved on 03/15/2023.</p>
                    </div>
                  </div>
                  
                  <div className="flex gap-2">
                    <BarChart className="text-secondary mt-0.5" width={18} />
                    <div>
                      <p className="text-sm"><span className="font-medium">Model Prediction:</span> Confidence Score 0.85 for 'Approve', 0.15 for 'Potential Fraud'.</p>
                    </div>
                  </div>
                </div>
              </div>
              
              <Divider />
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <h3 className="text-lg font-medium mb-3">Agent's Recommendation</h3>
                  <Card className="bg-warning-50 border-warning">
                    <CardBody>
                      <div className="flex items-center gap-2 mb-2">
                        <UserCheck className="text-warning" width={20} />
                        <h4 className="font-medium">Requires Human Review - High Value Claim</h4>
                      </div>
                      <p className="text-sm">
                        This claim exceeds the automatic approval threshold of $1,000 and requires human verification. 
                        All other policy checks have passed. Historical data shows similar claims were approved.
                      </p>
                    </CardBody>
                  </Card>
                </div>
                
                <div>
                  <h3 className="text-lg font-medium mb-3">Risk Assessment</h3>
                  <div className="space-y-4">
                    <div>
                      <div className="flex justify-between mb-1">
                        <span className="text-sm">Fraud Risk</span>
                        <span className="text-sm font-medium">15%</span>
                      </div>
                      <Progress value={15} color="success" className="h-2" />
                    </div>
                    
                    <div>
                      <div className="flex justify-between mb-1">
                        <span className="text-sm">Policy Compliance</span>
                        <span className="text-sm font-medium">95%</span>
                      </div>
                      <Progress value={95} color="success" className="h-2" />
                    </div>
                    
                    <div>
                      <div className="flex justify-between mb-1">
                        <span className="text-sm">Overall Confidence</span>
                        <span className="text-sm font-medium">85%</span>
                      </div>
                      <Progress value={85} color="primary" className="h-2" />
                    </div>
                  </div>
                </div>
              </div>
            </CardBody>
          </Card>
        </Tab>
        
        <Tab 
          key="documents" 
          title={
            <div className="flex items-center gap-2">
              <FileText width={18} />
              <span>Original Claim Documents</span>
            </div>
          }
        >
          <Card shadow="none">
            <CardBody>
              <div className="flex flex-col items-center justify-center p-8 border-2 border-dashed rounded-lg">
                <div className="mb-4">
                  <FileText width={48} className="text-foreground-400" />
                </div>
                <h3 className="text-lg font-medium">Claim Document Preview</h3>
                <p className="text-sm text-foreground-500 mt-1 mb-4">
                  View and download the original claim documents
                </p>
                <div className="flex gap-3">
                  <Button 
                    color="primary"
                    startContent={<Eye width={16} />}
                  >
                    View Document
                  </Button>
                  <Button 
                    variant="flat" 
                    color="primary"
                    startContent={<Download width={16} />}
                  >
                    Download
                  </Button>
                </div>
              </div>
            </CardBody>
          </Card>
        </Tab>
        
        <Tab 
          key="policy" 
          title={
            <div className="flex items-center gap-2">
              <BookOpen width={18} />
              <span>Policy Context</span>
            </div>
          }
        >
          <Card shadow="none">
            <CardBody>
              <h3 className="text-lg font-medium mb-4">Relevant Policies</h3>
              
              <div className="space-y-4">
                <Card className="shadow-none border border-divider">
                  <CardBody>
                    <h4 className="font-medium">Medical Coverage Policy v2.3</h4>
                    <p className="text-sm text-foreground-500 mt-1">Section 4.B - Consultation Coverage</p>
                    <div className="mt-3 p-3 bg-content2 rounded-md">
                      <p className="text-sm">
                        "Standard consultations with in-network providers are covered at 80% after deductible is met. 
                        Specialist consultations may require pre-authorization for certain conditions as outlined in 
                        Appendix A. Maximum allowable amount for standard consultations is determined by provider 
                        contract rates."
                      </p>
                    </div>
                    <div className="mt-3">
                      <h5 className="text-sm font-medium">Agent's Interpretation:</h5>
                      <p className="text-sm mt-1">
                        This consultation is covered under the member's plan as it was provided by an in-network 
                        provider. The service code CON-2023 matches standard consultation coverage criteria.
                      </p>
                    </div>
                  </CardBody>
                </Card>
                
                <Card className="shadow-none border border-divider">
                  <CardBody>
                    <h4 className="font-medium">Claims Processing Guidelines v1.8</h4>
                    <p className="text-sm text-foreground-500 mt-1">Section 2.C - High Value Claims</p>
                    <div className="mt-3 p-3 bg-content2 rounded-md">
                      <p className="text-sm">
                        "Claims exceeding $1,000 in total billed amount must undergo additional verification steps, 
                        including manual review by a claims adjudicator. This applies even when all other automated 
                        checks have passed successfully."
                      </p>
                    </div>
                    <div className="mt-3">
                      <h5 className="text-sm font-medium">Agent's Interpretation:</h5>
                      <p className="text-sm mt-1">
                        This claim's total amount of $1,250 exceeds the $1,000 threshold for automatic approval, 
                        requiring human review per policy guidelines.
                      </p>
                    </div>
                  </CardBody>
                </Card>
              </div>
            </CardBody>
          </Card>
        </Tab>
        
        <Tab 
          key="adjudication" 
          title={
            <div className="flex items-center gap-2">
              <CheckSquare width={18} />
              <span>Human Review & Adjudication</span>
            </div>
          }
        >
          <Card shadow="none">
            <CardBody className="gap-6">

              <div>
                <h3 className="text-lg font-medium mb-4">Adjudication Decision</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium mb-2">Decision</h4>
                    <div className="flex flex-wrap gap-2">
                      <Button 
                        color="success" 
                        variant={decision === "approve" ? "solid" : "flat"}
                        startContent={<Check width={16} />}
                        onPress={() => setDecision("approve")}
                        title="Approve the claim"
                      >
                        Approve
                      </Button>
                      <Button 
                        color="primary" 
                        variant={decision === "partial" ? "solid" : "flat"}
                        startContent={<Edit width={16} />}
                        onPress={() => setDecision("partial")}
                        title="Partially approve the claim"
                      >
                        Partial Approve
                      </Button>
                      <Button 
                        color="warning" 
                        variant={decision === "pend" ? "solid" : "flat"}
                        startContent={<Clock width={16} />}
                        onPress={() => setDecision("pend")}
                        title="Pend and request more information"
                      >
                        Pend
                      </Button>
                      <Button 
                        color="danger" 
                        variant={decision === "reject" ? "solid" : "flat"}
                        startContent={<X width={16} />}
                        onPress={() => setDecision("reject")}
                        title="Reject the claim"
                      >
                        Reject
                      </Button>
                    </div>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium mb-2">Reason Codes</h4>
                    <div className="flex flex-wrap gap-2">
                      {[
                        { label: "Policy Compliant", color: "success" as const },
                        { label: "Member Eligible", color: "primary" as const },
                        { label: "Service Not Covered", color: "danger" as const },
                        { label: "Missing Information", color: "danger" as const },
                        { label: "Requires Additional Review", color: "warning" as const },
                        { label: "Possible Duplicate", color: "warning" as const },
                      ].map((reason) => (
                        <Chip
                          key={reason.label}
                          variant={notes.includes(reason.label) ? "solid" : "flat"}
                          color={reason.color}
                          className="cursor-pointer"
                          onClick={() => setNotes(notes.includes(reason.label) ? notes.replace(reason.label, "") : notes + (notes ? ", " : "") + reason.label)}
                        >
                          {reason.label}
                        </Chip>
                      ))}
                    </div>
                  </div>
                </div>
              </div>

              <div className="mt-6 p-4 bg-content2 rounded-md border border-zinc-100">
                <h4 className="text-sm font-medium mb-2">Current Selection</h4>
                <div className="flex flex-wrap gap-4 items-center">
                  <span className="text-sm">Decision: <span className="font-semibold">{decision ? decision.charAt(0).toUpperCase() + decision.slice(1) : "None"}</span></span>
                  <span className="text-sm">Reasons: <span className="font-semibold">{notes || "None"}</span></span>
                </div>
              </div>

              <div className="mt-6">
                <h4 className="text-sm font-medium mb-2">Notes/Comments</h4>
                <Textarea
                  placeholder="Enter your detailed reasoning for this decision..."
                  value={notes}
                  onValueChange={setNotes}
                  minRows={4}
                />
                <p className="text-xs text-foreground-500 mt-1">
                  Please provide detailed reasoning, especially if overriding the agent's recommendation.
                </p>
              </div>

              <div className="flex justify-end gap-3 mt-4">
                <Button 
                  variant="flat" 
                  color="primary"
                >
                  Save Draft
                </Button>
                <Button 
                  color="primary"
                  isDisabled={!decision || !notes}
                  onPress={() => alert('Decision submitted!')}
                >
                  Submit Decision
                </Button>
              </div>
            </CardBody>
          </Card>
        </Tab>
        
        <Tab 
          key="audit" 
          title={
            <div className="flex items-center gap-2">
              <History width={18} />
              <span>Audit Trail</span>
            </div>
          }
        >
          <Card shadow="none">
            <CardBody>
              <h3 className="text-lg font-medium mb-4">Claim Activity Log</h3>
              
              <div className="space-y-6">
                <div className="flex gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-white">
                      <Inbox width={16} />
                    </div>
                    <div className="flex-grow w-0.5 bg-divider my-2"></div>
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="font-medium">Claim Ingested</h4>
                      <Chip size="sm" variant="flat" color="primary">System</Chip>
                    </div>
                    <p className="text-sm text-foreground-500 mt-1">May 15, 2023 - 09:32 AM</p>
                    <p className="text-sm mt-2">
                      Claim #{claimId} was received via Portal and entered into the system.
                    </p>
                  </div>
                </div>
                
                <div className="flex gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full bg-secondary flex items-center justify-center text-white">
                      <Cpu width={16} />
                    </div>
                    <div className="flex-grow w-0.5 bg-divider my-2"></div>
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="font-medium">Agent Processing Started</h4>
                      <Chip size="sm" variant="flat" color="secondary">AI Agent</Chip>
                    </div>
                    <p className="text-sm text-foreground-500 mt-1">May 15, 2023 - 09:35 AM</p>
                    <p className="text-sm mt-2">
                      AI Agent began processing the claim.
                    </p>
                  </div>
                </div>
                
                <div className="flex gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full bg-secondary flex items-center justify-center text-white">
                      <Cpu width={16} />
                    </div>
                    <div className="flex-grow w-0.5 bg-divider my-2"></div>
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="font-medium">Agent Processing Completed</h4>
                      <Chip size="sm" variant="flat" color="secondary">AI Agent</Chip>
                    </div>
                    <p className="text-sm text-foreground-500 mt-1">May 15, 2023 - 09:36 AM</p>
                    <p className="text-sm mt-2">
                      AI Agent completed processing and flagged for human review due to high value claim.
                    </p>
                  </div>
                </div>
                
                <div className="flex gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full bg-warning flex items-center justify-center text-white">
                      <UserCheck width={16} />
                    </div>
                    <div className="flex-grow w-0.5 bg-divider my-2"></div>
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="font-medium">Status Changed to Pending Human Review</h4>
                      <Chip size="sm" variant="flat" color="warning">System</Chip>
                    </div>
                    <p className="text-sm text-foreground-500 mt-1">May 15, 2023 - 09:36 AM</p>
                    <p className="text-sm mt-2">
                      Claim status was updated to "Pending Human Review".
                    </p>
                  </div>
                </div>
                
                <div className="flex gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full bg-default flex items-center justify-center text-white">
                      <User width={16} />
                    </div>
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="font-medium">Viewed by John Smith</h4>
                      <Chip size="sm" variant="flat" color="default">User</Chip>
                    </div>
                    <p className="text-sm text-foreground-500 mt-1">May 15, 2023 - 10:15 AM</p>
                    <p className="text-sm mt-2">
                      Claim details were viewed by John Smith (Claims Adjudicator).
                    </p>
                  </div>
                </div>
              </div>
            </CardBody>
          </Card>
        </Tab>
      </Tabs>
    </div>
  );
};
