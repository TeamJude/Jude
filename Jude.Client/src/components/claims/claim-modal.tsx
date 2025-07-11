import React, { useState, useEffect } from 'react';
import { 
  Modal, 
  ModalContent, 
  ModalHeader, 
  ModalBody, 
  ModalFooter, 
  Button, 
  Progress, 
  Card,
  Chip,
  Divider,
  Accordion,
  AccordionItem
} from "@heroui/react";
import { 
  X, 
  Bot, 
  Clock, 
  Play, 
  RefreshCw, 
  DollarSign, 
  User, 
  Building2,
  AlertTriangle,
  CheckCircle,
  TrendingUp,
  Calendar,
  FileText
} from "lucide-react";

interface ClaimModalProps {
  claim: {
    id: string;
    transactionNumber: string;
    patientName: string;
    membershipNumber: string;
    claimAmount: number;
    currency: string;
    approvedAmount?: number;
    providerPractice: string;
    status: 'inQueue' | 'inProgress' | 'awaitingReview';
    fraudRiskLevel: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
    isFlagged: boolean;
    source: string;
    ingestedAt: string;
    processingStartedAt?: string;
    lastUpdatedAt: string;
    patientDetails?: {
      dateOfBirth: string;
      gender: string;
      idNumber: string;
    };
    providerDetails?: {
      name: string;
      contactInfo: string;
    };
    items?: Array<{
      type: 'product' | 'service';
      code: string;
      description: string;
      amount: number;
      status: string;
    }>;
    agentProgress?: number;
    agentReasoning?: string[];
  };
  onClose: () => void;
}

export const ClaimModal: React.FC<ClaimModalProps> = ({ claim, onClose }) => {
  const [currentProgress, setCurrentProgress] = useState(claim.agentProgress || 0);
  const [reasoningLogs, setReasoningLogs] = useState(claim.agentReasoning || []);
  const [isStreaming, setIsStreaming] = useState(claim.status === 'inProgress');

  // Simulate streaming reasoning for in-progress claims
  useEffect(() => {
    if (claim.status === 'inProgress' && isStreaming) {
      const interval = setInterval(() => {
        setCurrentProgress(prev => {
          const newProgress = prev + Math.random() * 5;
          return newProgress > 95 ? 95 : newProgress;
        });

        // Simulate new reasoning logs
        const newReasonings = [
          "Validating medical codes against ICD-10 standards...",
          "Cross-referencing member eligibility database...",
          "Analyzing provider billing patterns...",
          "Detecting potential fraud indicators...",
          "Calculating confidence score based on historical data...",
          "Reviewing claim against policy guidelines...",
          "Finalizing risk assessment...",
        ];

        if (Math.random() < 0.3) { // 30% chance to add new reasoning
          const randomReasoning = newReasonings[Math.floor(Math.random() * newReasonings.length)];
          setReasoningLogs(prev => [...prev, randomReasoning]);
        }
      }, 2000);

      return () => clearInterval(interval);
    }
  }, [claim.status, isStreaming]);

  const getRiskColor = () => {
    switch (claim.fraudRiskLevel) {
      case 'CRITICAL':
        return 'danger';
      case 'HIGH':
        return 'danger';
      case 'MEDIUM':
        return 'warning';
      case 'LOW':
        return 'success';
      default:
        return 'default';
    }
  };

  const getStatusDisplay = () => {
    switch (claim.status) {
      case 'inQueue':
        return { text: 'In Queue', color: 'primary', icon: Clock };
      case 'inProgress':
        return { text: 'In Progress', color: 'secondary', icon: RefreshCw };
      case 'awaitingReview':
        return { text: 'Awaiting Review', color: 'warning', icon: AlertTriangle };
      default:
        return { text: 'Unknown', color: 'default', icon: Clock };
    }
  };

  const statusInfo = getStatusDisplay();
  const StatusIcon = statusInfo.icon;

  return (
    <Modal 
      isOpen={true} 
      onClose={onClose}
      scrollBehavior="inside"
      size="4xl"
      classNames={{
        base: "max-h-[90vh]",
        body: "py-4"
      }}
    >
      <ModalContent>
        <ModalHeader className="flex flex-col gap-2 border-b border-gray-200 pb-4">
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-semibold text-gray-800">
              Claim Details - {claim.transactionNumber}
            </h2>
            <Button isIconOnly color="danger" variant="light" onPress={onClose}>
              <X className="w-4 h-4" />
            </Button>
          </div>
          <div className="flex items-center gap-4 text-sm">
            <div className="flex items-center gap-2">
              <StatusIcon className="w-4 h-4" />
              <Chip color={statusInfo.color} variant="flat" size="sm">
                {statusInfo.text}
              </Chip>
            </div>
            <div className="flex items-center gap-2">
              <span>Risk Level:</span>
              <Chip color={getRiskColor()} variant="flat" size="sm">
                {claim.fraudRiskLevel}
              </Chip>
            </div>
            {claim.isFlagged && (
              <Chip color="danger" variant="flat" size="sm" startContent={<AlertTriangle className="w-3 h-3" />}>
                Flagged
              </Chip>
            )}
          </div>
        </ModalHeader>

        <ModalBody className="space-y-6">
          {/* Patient & Provider Information */}
          <section>
            <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
              <User className="w-5 h-5" />
              Patient & Provider Information
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Card className="p-4">
                <h4 className="font-medium mb-2">Patient Details</h4>
                <div className="space-y-1 text-sm">
                  <p><strong>Name:</strong> {claim.patientName}</p>
                  <p><strong>Member ID:</strong> {claim.membershipNumber}</p>
                  {claim.patientDetails && (
                    <>
                      <p><strong>DOB:</strong> {claim.patientDetails.dateOfBirth}</p>
                      <p><strong>Gender:</strong> {claim.patientDetails.gender}</p>
                      <p><strong>ID Number:</strong> {claim.patientDetails.idNumber}</p>
                    </>
                  )}
                </div>
              </Card>
              <Card className="p-4">
                <h4 className="font-medium mb-2">Provider Details</h4>
                <div className="space-y-1 text-sm">
                  <p><strong>Practice:</strong> {claim.providerPractice}</p>
                  <p><strong>Source:</strong> {claim.source}</p>
                  {claim.providerDetails && (
                    <>
                      <p><strong>Name:</strong> {claim.providerDetails.name}</p>
                      <p><strong>Contact:</strong> {claim.providerDetails.contactInfo}</p>
                    </>
                  )}
                </div>
              </Card>
            </div>
          </section>

          {/* Financial Summary */}
          <section>
            <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
              <DollarSign className="w-5 h-5" />
              Financial Summary
            </h3>
            <Card className="p-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="text-center">
                  <p className="text-sm text-gray-500">Claimed Amount</p>
                  <p className="text-xl font-semibold text-gray-800">
                    {claim.currency}{claim.claimAmount.toLocaleString()}
                  </p>
                </div>
                <div className="text-center">
                  <p className="text-sm text-gray-500">Approved Amount</p>
                  <p className="text-xl font-semibold text-green-600">
                    {claim.approvedAmount ? `${claim.currency}${claim.approvedAmount.toLocaleString()}` : 'Pending'}
                  </p>
                </div>
                <div className="text-center">
                  <p className="text-sm text-gray-500">Difference</p>
                  <p className="text-xl font-semibold text-gray-600">
                    {claim.approvedAmount 
                      ? `${claim.currency}${(claim.claimAmount - claim.approvedAmount).toLocaleString()}`
                      : 'TBD'
                    }
                  </p>
                </div>
              </div>
            </Card>
          </section>

          {/* AI Processing Section - Only for in-progress claims */}
          {claim.status === 'inProgress' && (
            <section>
              <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                <Bot className="w-5 h-5" />
                AI Agent Processing
              </h3>
              <Card className="p-4">
                <div className="flex items-center mb-4">
                  <Bot className="w-5 h-5 mr-2 text-purple-500" />
                  <span className="font-medium">AI Agent Processing...</span>
                  <div className="ml-auto flex items-center gap-2">
                    <span className="text-sm text-gray-500">Live</span>
                    <div className="w-2 h-2 bg-purple-500 rounded-full animate-pulse"></div>
                  </div>
                </div>
                
                <Progress 
                  value={currentProgress} 
                  className="mb-4"
                  color="secondary"
                  showValueLabel={true}
                  label="Processing Progress"
                />
                
                <div className="bg-gray-50 p-4 rounded-lg">
                  <p className="font-medium mb-2 flex items-center gap-2">
                    <TrendingUp className="w-4 h-4" />
                    Latest Reasoning:
                  </p>
                  <div className="space-y-1 max-h-32 overflow-y-auto">
                    {reasoningLogs.map((log, index) => (
                      <div key={index} className="text-sm text-gray-700 flex items-start gap-2">
                        <span className="text-purple-500 font-mono">›</span>
                        <span className={index === reasoningLogs.length - 1 ? 'animate-pulse' : ''}>
                          {log}
                        </span>
                      </div>
                    ))}
                    {isStreaming && (
                      <div className="text-sm text-gray-500 flex items-center gap-2">
                        <span className="text-purple-500 font-mono">›</span>
                        <span className="animate-pulse">Thinking...</span>
                        <div className="flex gap-1">
                          <div className="w-1 h-1 bg-purple-500 rounded-full animate-bounce"></div>
                          <div className="w-1 h-1 bg-purple-500 rounded-full animate-bounce" style={{animationDelay: '0.1s'}}></div>
                          <div className="w-1 h-1 bg-purple-500 rounded-full animate-bounce" style={{animationDelay: '0.2s'}}></div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </Card>
            </section>
          )}

          {/* Claim Items */}
          {claim.items && claim.items.length > 0 && (
            <section>
              <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                <FileText className="w-5 h-5" />
                Claim Items
              </h3>
              <Accordion>
                <AccordionItem key="items" title={`${claim.items.length} items`}>
                  <div className="space-y-2">
                    {claim.items.map((item, index) => (
                      <Card key={index} className="p-3">
                        <div className="flex justify-between items-start">
                          <div>
                            <p className="font-medium">{item.description}</p>
                            <p className="text-sm text-gray-500">Code: {item.code}</p>
                          </div>
                          <div className="text-right">
                            <p className="font-medium">{claim.currency}{item.amount}</p>
                            <Chip size="sm" variant="flat" color={item.status === 'approved' ? 'success' : 'warning'}>
                              {item.status}
                            </Chip>
                          </div>
                        </div>
                      </Card>
                    ))}
                  </div>
                </AccordionItem>
              </Accordion>
            </section>
          )}

          {/* Timeline */}
          <section>
            <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
              <Calendar className="w-5 h-5" />
              Timeline
            </h3>
            <Card className="p-4">
              <div className="space-y-3">
                <div className="flex items-center gap-3">
                  <Clock className="w-4 h-4 text-gray-500" />
                  <div>
                    <p className="text-sm font-medium">Claim Ingested</p>
                    <p className="text-xs text-gray-500">{claim.ingestedAt}</p>
                  </div>
                </div>
                {claim.processingStartedAt && (
                  <div className="flex items-center gap-3">
                    <Play className="w-4 h-4 text-purple-500" />
                    <div>
                      <p className="text-sm font-medium">Processing Started</p>
                      <p className="text-xs text-gray-500">{claim.processingStartedAt}</p>
                    </div>
                  </div>
                )}
                <div className="flex items-center gap-3">
                  <RefreshCw className="w-4 h-4 text-blue-500" />
                  <div>
                    <p className="text-sm font-medium">Last Updated</p>
                    <p className="text-xs text-gray-500">{claim.lastUpdatedAt}</p>
                  </div>
                </div>
              </div>
            </Card>
          </section>
        </ModalBody>

        <ModalFooter className="border-t border-gray-200 pt-4">
          <Button color="danger" variant="light" onPress={onClose}>
            Close
          </Button>
          {claim.status === 'awaitingReview' && (
            <>
              <Button color="warning" variant="flat">
                Request More Info
              </Button>
              <Button color="danger" variant="flat">
                Reject
              </Button>
              <Button color="success" className="bg-success text-white">
                Approve
              </Button>
            </>
          )}
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}; 