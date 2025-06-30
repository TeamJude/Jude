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
  FileText,
  Eye
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
    agentRecommendation?: string;
  };
  onClose: () => void;
  onReview?: (claimId: string) => void;
}

export const ClaimModal: React.FC<ClaimModalProps> = ({ claim, onClose, onReview }) => {
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

        const newReasonings = [
          "Validating medical codes against ICD-10 sstandards...",
          "Cross-referencing member eligibility database...",
          "Analyzing provider billing patterns...",
          "Detecting potential fraud indicators...",
          "Calculating confidence score based on historical data...",
          "Reviewing claim against policy guidelines...",
          "Finalizing risk assessment...",
        ];

        if (Math.random() < 0.3) {
          const randomReasoning = newReasonings[Math.floor(Math.random() * newReasonings.length)];
          setReasoningLogs(prev => [...prev, randomReasoning]);
        }
      }, 2000);

      return () => clearInterval(interval);
    }
  }, [claim.status, isStreaming]);

  const getRiskColor = (): "default" | "primary" | "secondary" | "success" | "warning" | "danger" => {
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
        return { text: 'In Queue', color: 'default' as const, icon: Clock };
      case 'inProgress':
        return { text: 'In Progress', color: 'secondary' as const, icon: RefreshCw };
      case 'awaitingReview':
        return { text: 'Awaiting Review', color: 'warning' as const, icon: AlertTriangle };
      default:
        return { text: 'Unknown', color: 'default' as const, icon: Clock };
    }
  };

  const statusInfo = getStatusDisplay();
  const StatusIcon = statusInfo.icon;

  const handleReview = () => {
    if (onReview) {
      onReview(claim.id);
    }
    onClose();
  };

  return (
    <Modal 
      isOpen={true} 
      onClose={onClose}
      scrollBehavior="inside"
      size="4xl"
      classNames={{
        base: "max-h-[90vh]",
        body: "py-6",
        backdrop: "bg-black/20"
      }}
    >
      <ModalContent className="bg-white">
        <ModalHeader className="flex flex-col gap-3 border-b border-gray-100 pb-4">
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-medium text-gray-900">
              Claim {claim.transactionNumber}
            </h2>
     
          </div>
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <StatusIcon className="w-4 h-4 text-gray-600" />
              <Chip color={statusInfo.color} variant="flat" size="sm">
                {statusInfo.text}
              </Chip>
            </div>
            <div className="flex items-center gap-2">
              <Chip color={getRiskColor()} variant="flat" size="sm">
                {claim.fraudRiskLevel} Risk
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
          {/* Basic Claim Information - Always shown */}
          <section>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-4">
                <div>
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Patient Information</h3>
                  <div className="bg-gray-50 rounded-lg p-4 space-y-2">
                    <p className="font-medium text-gray-900">{claim.patientName}</p>
                    <p className="text-sm text-gray-600">Member: {claim.membershipNumber}</p>
                    {claim.patientDetails && (
                      <>
                        <p className="text-sm text-gray-600">DOB: {claim.patientDetails.dateOfBirth}</p>
                        <p className="text-sm text-gray-600">ID: {claim.patientDetails.idNumber}</p>
                      </>
                    )}
                  </div>
                </div>
              </div>

              <div className="space-y-4">
                <div>
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Provider Information</h3>
                  <div className="bg-gray-50 rounded-lg p-4 space-y-2">
                    <p className="font-medium text-gray-900">{claim.providerPractice}</p>
                    <p className="text-sm text-gray-600">Source: {claim.source}</p>
                    {claim.providerDetails && (
                      <p className="text-sm text-gray-600">{claim.providerDetails.contactInfo}</p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </section>

          {/* Financial Summary */}
          <section>
            <h3 className="text-sm font-medium text-gray-500 mb-3">Financial Summary</h3>
            <div className="bg-gray-50 rounded-lg p-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <p className="text-sm text-gray-500">Claimed Amount</p>
                  <p className="text-lg font-semibold text-gray-900">
                    {claim.currency}{claim.claimAmount.toLocaleString()}
                  </p>
                </div>
                {claim.approvedAmount && (
                  <div>
                    <p className="text-sm text-gray-500">Approved Amount</p>
                    <p className="text-lg font-semibold text-green-600">
                      {claim.currency}{claim.approvedAmount.toLocaleString()}
                    </p>
                  </div>
                )}
                {claim.approvedAmount && (
                  <div>
                    <p className="text-sm text-gray-500">Difference</p>
                    <p className="text-lg font-semibold text-gray-600">
                      {claim.currency}{(claim.claimAmount - claim.approvedAmount).toLocaleString()}
                    </p>
                  </div>
                )}
              </div>
            </div>
          </section>

          {/* Status-specific content */}
          {claim.status === 'inQueue' && (
            <section>
              <div className="text-center py-8">
                <Clock className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">Claim in Queue</h3>
                <p className="text-gray-600">This claim is waiting to be processed by our AI agent.</p>
                <p className="text-sm text-gray-500 mt-2">Estimated processing time: 2-5 minutes</p>
              </div>
            </section>
          )}

          {claim.status === 'inProgress' && (
            <section>
              <h3 className="text-sm font-medium text-gray-500 mb-3">AI Processing</h3>
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="flex items-center mb-4">
                  <Bot className="w-5 h-5 mr-2 text-blue-600" />
                  <span className="font-medium text-gray-900">AI Agent Processing</span>
                  <div className="ml-auto flex items-center gap-2">
                    <span className="text-sm text-gray-500">Live</span>
                    <div className="w-2 h-2 bg-blue-500 rounded-full animate-pulse"></div>
                  </div>
                </div>
                
                <Progress 
                  value={currentProgress} 
                  className="mb-4"
                  color="secondary"
                  showValueLabel={true}
                  label="Processing Progress"
                />
                
                <div className="bg-white rounded-lg p-4 border">
                  <p className="font-medium mb-3 text-gray-900">Latest Activity:</p>
                  <div className="space-y-2 max-h-32 overflow-y-auto">
                    {reasoningLogs.map((log, index) => (
                      <div key={index} className="text-sm text-gray-700 flex items-start gap-2">
                        <span className="text-blue-500 font-mono">•</span>
                        <span className={index === reasoningLogs.length - 1 ? 'animate-pulse' : ''}>
                          {log}
                        </span>
                      </div>
                    ))}
                    {isStreaming && (
                      <div className="text-sm text-gray-500 flex items-center gap-2">
                        <span className="text-blue-500 font-mono">•</span>
                        <span className="animate-pulse">Processing...</span>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </section>
          )}

          {claim.status === 'awaitingReview' && (
            <>
              {/* Agent Analysis Results */}
              <section>
                <h3 className="text-sm font-medium text-gray-500 mb-3">AI Analysis Complete</h3>
                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="flex items-center mb-3">
                    <CheckCircle className="w-5 h-5 mr-2 text-green-600" />
                    <span className="font-medium text-gray-900">Analysis Complete</span>
                  </div>
                  
                  {claim.agentRecommendation && (
                    <div className="bg-white rounded-lg p-4 border border-green-200 mb-4">
                      <div className="flex items-center mb-2">
                        <Bot className="w-4 h-4 mr-2 text-green-600" />
                        <p className="font-medium text-gray-900">AI Recommendation</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <Chip 
                          color={claim.agentRecommendation.toLowerCase().includes('approve') ? 'success' : 'warning'} 
                          variant="flat" 
                          size="sm"
                        >
                          {claim.agentRecommendation}
                        </Chip>
                        {currentProgress === 100 && (
                          <span className="text-xs text-green-600">100% Confidence</span>
                        )}
                      </div>
                    </div>
                  )}

                  {reasoningLogs.length > 0 && (
                    <div className="bg-white rounded-lg p-4 border">
                      <p className="font-medium mb-3 text-gray-900">Key Findings:</p>
                      <div className="space-y-2">
                        {reasoningLogs.slice(-3).map((log, index) => (
                          <div key={index} className="text-sm text-gray-700 flex items-start gap-2">
                            <span className="text-green-500 font-mono">✓</span>
                            <span>{log}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </section>

              {/* Review Required */}
              <section>
                <div className="bg-orange-50 border border-orange-200 rounded-lg p-4">
                  <div className="flex items-center mb-2">
                    <AlertTriangle className="w-5 h-5 mr-2 text-orange-600" />
                    <span className="font-medium text-orange-900">Human Review Required</span>
                  </div>
                  <p className="text-sm text-orange-800">
                    This claim requires human review based on our analysis. Please review the details and make a decision.
                  </p>
                </div>
              </section>
            </>
          )}

          {/* Timeline */}
          <section>
            <h3 className="text-sm font-medium text-gray-500 mb-3">Timeline</h3>
            <div className="bg-gray-50 rounded-lg p-4 space-y-3">
              <div className="flex items-center gap-3">
                <div className="w-2 h-2 bg-gray-400 rounded-full"></div>
                <div>
                  <p className="text-sm font-medium text-gray-900">Claim Received</p>
                  <p className="text-xs text-gray-500">{claim.ingestedAt}</p>
                </div>
              </div>
              {claim.processingStartedAt && (
                <div className="flex items-center gap-3">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <div>
                    <p className="text-sm font-medium text-gray-900">Processing Started</p>
                    <p className="text-xs text-gray-500">{claim.processingStartedAt}</p>
                  </div>
                </div>
              )}
              <div className="flex items-center gap-3">
                <div className="w-2 h-2 bg-gray-400 rounded-full"></div>
                <div>
                  <p className="text-sm font-medium text-gray-900">Last Updated</p>
                  <p className="text-xs text-gray-500">{claim.lastUpdatedAt}</p>
                </div>
              </div>
            </div>
          </section>
        </ModalBody>

        <ModalFooter className="border-t border-gray-100 pt-4">
          <Button 
            variant="light" 
            onPress={onClose}
            className="text-gray-600 hover:text-gray-800"
          >
            Close
          </Button>
          {claim.status === 'awaitingReview' && (
            <Button 
              color="primary" 
              onPress={handleReview}
              startContent={<Eye className="w-4 h-4" />}
              className="bg-blue-600 text-white hover:bg-blue-700"
            >
              Review Claim
            </Button>
          )}
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}; 