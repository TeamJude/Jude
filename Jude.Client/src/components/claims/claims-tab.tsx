import React from 'react';
import { Tabs, Tab, Card, CardBody, Divider, Chip, Progress, Button, Textarea, Select, SelectItem } from '@heroui/react';
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
  const [finalDecision, setFinalDecision] = React.useState("");
  const [reviewerComments, setReviewerComments] = React.useState("");
  const [rejectionReason, setRejectionReason] = React.useState("");

  const handleSelectionChange = (key: React.Key) => {
    setSelected(key.toString());
  };

  const decisions = [
    { key: "Approve", label: "Approve", description: "Approve the claim as submitted" },
    { key: "Reject", label: "Reject", description: "Reject the claim" },
    { key: "RequestMoreInfo", label: "Request More Info", description: "Request additional information" },
    { key: "Escalate", label: "Escalate", description: "Escalate to supervisor" }
  ];

  const rejectionReasons = [
    { key: "service_not_covered", label: "Service Not Covered" },
    { key: "member_not_eligible", label: "Member Not Eligible" },
    { key: "insufficient_documentation", label: "Insufficient Documentation" },
    { key: "duplicate_claim", label: "Duplicate Claim" },
    { key: "provider_not_in_network", label: "Provider Not In Network" },
    { key: "pre_authorization_required", label: "Pre-authorization Required" },
    { key: "policy_exclusion", label: "Policy Exclusion" },
    { key: "fraudulent_activity", label: "Fraudulent Activity" }
  ];

  const handleSubmitDecision = () => {
    if (!finalDecision || !reviewerComments) {
      alert('Please select a decision and provide comments.');
      return;
    }
    
    if (finalDecision === 'Reject' && !rejectionReason) {
      alert('Please select a rejection reason for rejected claims.');
      return;
    }

    // Here you would typically call an API to submit the decision
    const reviewData = {
      claimId,
      finalDecision,
      reviewerComments,
      rejectionReason: finalDecision === 'Reject' ? rejectionReason : null,
      reviewedAt: new Date().toISOString()
    };
    
    console.log('Submitting review:', reviewData);
    alert('Decision submitted successfully!');
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
                  <Card className="bg-warning-50 shadow-none border-1 border-gray-300 ">
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
                <h3 className="text-lg font-medium mb-4">Review Decision</h3>
                
                <div className="space-y-6">
                  {/* Final Decision */}
                  <div>
                    <h4 className="text-sm font-medium mb-2 text-gray-700">Final Decision *</h4>
                    <Select
                      placeholder="Select your decision"
                      selectedKeys={finalDecision ? [finalDecision] : []}
                      onSelectionChange={(keys) => setFinalDecision(Array.from(keys)[0] as string)}
                      className="max-w-md"
                      variant="bordered"
                    >
                      {decisions.map((decision) => (
                        <SelectItem key={decision.key}>
                          <div>
                            <div className="font-medium">{decision.label}</div>
                            <div className="text-sm text-gray-500">{decision.description}</div>
                          </div>
                        </SelectItem>
                      ))}
                    </Select>
                  </div>

                  {/* Rejection Reason - Only show if decision is Reject */}
                  {finalDecision === 'Reject' && (
                    <div>
                      <h4 className="text-sm font-medium mb-2 text-gray-700">Rejection Reason *</h4>
                      <Select
                        placeholder="Select rejection reason"
                        selectedKeys={rejectionReason ? [rejectionReason] : []}
                        onSelectionChange={(keys) => setRejectionReason(Array.from(keys)[0] as string)}
                        className="max-w-md"
                        variant="bordered"
                      >
                        {rejectionReasons.map((reason) => (
                          <SelectItem key={reason.key}>
                            {reason.label}
                          </SelectItem>
                        ))}
                      </Select>
                    </div>
                  )}

                  {/* Reviewer Comments */}
                  <div>
                    <h4 className="text-sm font-medium mb-2 text-gray-700">Reviewer Comments *</h4>
                    <Textarea
                      placeholder="Provide detailed reasoning for your decision..."
                      value={reviewerComments}
                      onValueChange={setReviewerComments}
                      minRows={4}
                      variant="bordered"
                      className="max-w-2xl"
                    />
                    <p className="text-xs text-gray-500 mt-1">
                      Please provide clear justification for your decision, especially if overriding the AI recommendation.
                    </p>
                  </div>
                  
                  {/* Decision Summary */}
                  {(finalDecision || reviewerComments) && (
                    <div className="bg-gray-50 rounded-lg p-4 border">
                      <h4 className="text-sm font-medium mb-2 text-gray-700">Decision Summary</h4>
                      <div className="space-y-1 text-sm">
                        <div><span className="font-medium">Decision:</span> {finalDecision || 'Not selected'}</div>
                        {finalDecision === 'Reject' && rejectionReason && (
                          <div><span className="font-medium">Rejection Reason:</span> {rejectionReasons.find(r => r.key === rejectionReason)?.label}</div>
                        )}
                        <div><span className="font-medium">Comments:</span> {reviewerComments ? `${reviewerComments.substring(0, 100)}${reviewerComments.length > 100 ? '...' : ''}` : 'No comments provided'}</div>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex justify-end gap-3 pt-4 border-t">
                <Button 
                  variant="flat" 
                  color="default"
                  className="min-w-24"
                >
                  Save Draft
                </Button>
                <Button 
                  color="primary"
                  className="min-w-24"
                  isDisabled={!finalDecision || !reviewerComments || (finalDecision === 'Reject' && !rejectionReason)}
                  onPress={handleSubmitDecision}
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
