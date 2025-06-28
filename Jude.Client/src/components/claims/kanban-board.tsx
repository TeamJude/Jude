import React from 'react';
import { Card, Chip, Badge } from "@heroui/react";
import { ClaimCard } from './claim-card';
import { Clock, RefreshCw, AlertTriangle, Loader2 } from 'lucide-react';

interface KanbanBoardProps {
  claims: Array<{
    id: string;
    transactionNumber: string;
    patientName: string;
    membershipNumber: string;
    claimAmount: number;
    currency: string;
    providerPractice: string;
    timeAgo: string;
    itemSummary: string;
    status: 'inQueue' | 'inProgress' | 'awaitingReview';
    fraudRiskLevel: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
    isFlagged: boolean;
    source: string;
  }>;
  onClaimClick: (claim: any) => void;
}

export const KanbanBoard: React.FC<KanbanBoardProps> = ({ claims, onClaimClick }) => {
  const lanes = [
    { 
      title: "In Queue", 
      status: "inQueue" as const, 
      color: "blue",
      description: "Claims waiting to be processed",
      icon: Clock,
      bgClass: "bg-blue-50",
      borderClass: "border-blue-500",
      textClass: "text-blue-700"
    },
    { 
      title: "In Progress", 
      status: "inProgress" as const, 
      color: "purple",
      description: "Claims currently being processed by AI",
      icon: RefreshCw,
      bgClass: "bg-purple-50",
      borderClass: "border-purple-500",
      textClass: "text-purple-700"
    },
    { 
      title: "Awaiting Review", 
      status: "awaitingReview" as const, 
      color: "orange",
      description: "Claims pending human review",
      icon: AlertTriangle,
      bgClass: "bg-orange-50",
      borderClass: "border-orange-500",
      textClass: "text-orange-700"
    }
  ];

  const getClaimsForLane = (status: string) => {
    return claims.filter((claim) => claim.status === status);
  };

  const getTotalAmount = (laneClaims: any[]) => {
    return laneClaims.reduce((total, claim) => total + claim.claimAmount, 0);
  };

  const getHighRiskCount = (laneClaims: any[]) => {
    return laneClaims.filter(claim => 
      claim.fraudRiskLevel === 'HIGH' || claim.fraudRiskLevel === 'CRITICAL'
    ).length;
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      {lanes.map((lane) => {
        const laneClaims = getClaimsForLane(lane.status);
        const totalAmount = getTotalAmount(laneClaims);
        const highRiskCount = getHighRiskCount(laneClaims);
        const LaneIcon = lane.icon;
        
        return (
          <div key={lane.status} className="flex flex-col h-full">
            {/* Lane Header */}
            <Card className={`p-4 mb-4 border-t-4 ${lane.borderClass} ${lane.bgClass}`}>
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-2">
                  <LaneIcon className={`w-5 h-5 ${lane.textClass}`} />
                  <h2 className={`text-xl font-semibold ${lane.textClass}`}>
                    {lane.title}
                  </h2>
                  <Badge 
                    content={laneClaims.length}
                    color={lane.color as any}
                    variant="flat"
                    className="ml-2"
                  >
                    <div className="w-2 h-2"></div>
                  </Badge>
                </div>
                {lane.status === 'inProgress' && laneClaims.length > 0 && (
                  <Loader2 className="w-4 h-4 text-purple-500 animate-spin" />
                )}
              </div>
              
              <p className="text-sm text-gray-600 mb-3">{lane.description}</p>
              
              {/* Lane Statistics */}
              <div className="grid grid-cols-2 gap-2 text-sm">
                <div className="bg-white/50 p-2 rounded">
                  <p className="text-gray-500">Total Value</p>
                  <p className="font-semibold">
                    {laneClaims.length > 0 ? `$${totalAmount.toLocaleString()}` : '$0'}
                  </p>
                </div>
                <div className="bg-white/50 p-2 rounded">
                  <p className="text-gray-500">High Risk</p>
                  <div className="flex items-center gap-1">
                    <p className="font-semibold">{highRiskCount}</p>
                    {highRiskCount > 0 && (
                      <AlertTriangle className="w-3 h-3 text-danger-500" />
                    )}
                  </div>
                </div>
              </div>
            </Card>

            {/* Claims Container */}
            <div className="flex-1 min-h-[400px]">
              {laneClaims.length === 0 ? (
                <Card className="p-8 text-center border-2 border-dashed border-gray-200 bg-gray-50/50">
                  <LaneIcon className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                  <p className="text-gray-500">No claims in this stage</p>
                  <p className="text-sm text-gray-400 mt-1">
                    Claims will appear here as they move through the process
                  </p>
                </Card>
              ) : (
                <div className="space-y-4">
                  {laneClaims.map((claim) => (
                    <ClaimCard 
                      key={claim.transactionNumber} 
                      claim={claim} 
                      onClick={() => onClaimClick(claim)} 
                    />
                  ))}
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}; 