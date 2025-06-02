import type React from 'react';
import { Card, CardBody, Button, Avatar } from '@heroui/react';
import { Building2, Calendar, DollarSign, Printer } from 'lucide-react';

interface ClaimDetailHeaderProps {
  claimId: string;
  memberName: string;
  memberId: string;
  providerName: string;
  providerId: string;
  dateReceived: string;
  dateOfService: string;
  amount: number;
  status: string;
  memberAvatar?: string;
}

const statusColorMap: Record<string, string> = {
  approved: 'ring-green-500',
  rejected: 'ring-red-500',
  'pending human review': 'ring-yellow-500',
  'flagged for investigation': 'ring-red-500',
  default: 'ring-gray-300',
};

export const ClaimDetailHeader: React.FC<ClaimDetailHeaderProps> = ({
  memberName,
  memberId,
  providerName,
  providerId,
  dateReceived,
  dateOfService,
  amount,
  status,
  memberAvatar,
}) => {
  const statusKey = status.toLowerCase();
  const ringColor = statusColorMap[statusKey] || statusColorMap.default;

  return (
    <Card shadow="none" className="w-full">
      <CardBody>
        <div className="flex flex-row items-center gap-8 flex-wrap">
          {/* Avatar with status ring and label */}
          <div className="flex flex-col items-center justify-center mr-2">
            <Avatar
              src={memberAvatar || `https://img.heroui.chat/image/avatar?w=48&h=48&u=${memberId}`}
              name={memberName}
              size="md"
              className={`ring-2 ${ringColor}`}
            />
          </div>

          {/* Member Info */}
          <div className="flex flex-col min-w-[120px]">
            <p className="text-xs text-foreground-500">Member</p>
            <p className="font-medium text-sm">{memberName}</p>
            <p className="text-xs text-foreground-500">{memberId}</p>
          </div>

          {/* Provider Info */}
          <div className="flex flex-col min-w-[140px]">
            <div className="flex items-center gap-2">
              <Building2 className="text-primary w-4 h-4" />
              <div>
                <p className="text-xs text-foreground-500">Provider</p>
                <p className="font-medium text-sm">{providerName}</p>
                <p className="text-xs text-foreground-500">{providerId}</p>
              </div>
            </div>
          </div>

          {/* Amount */}
          <div className="flex flex-col min-w-[100px]">
            <div className="flex items-center gap-2">
              <DollarSign className="text-primary w-4 h-4" />
              <div>
                <p className="text-xs text-foreground-500">Amount</p>
                <p className="font-medium text-sm">${amount.toLocaleString()}</p>
              </div>
            </div>
          </div>

          {/* Dates */}
          <div className="flex flex-col min-w-[140px]">
            <div className="flex items-center gap-2">
              <Calendar className="text-primary w-4 h-4" />
              <div>
                <p className="text-xs text-foreground-500">Service Date</p>
                <p className="font-medium text-sm">{dateOfService}</p>
                <p className="text-xs text-foreground-500">Received: {dateReceived}</p>
              </div>
            </div>
          </div>

          {/* Print Button */}
          <div className="ml-auto">
            <Button
              size="sm"
              variant="flat"
              color="primary"
              startContent={<Printer className="w-4 h-4" />}
            >
              Print
            </Button>
          </div>
        </div>
      </CardBody>
    </Card>
  );
};