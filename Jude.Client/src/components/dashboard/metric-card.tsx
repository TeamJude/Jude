import type React from 'react';
import { Card, CardBody, CardFooter, Tooltip } from '@heroui/react';
import { DynamicIcon, type IconName } from 'lucide-react/dynamic';

interface MetricsCardProps {
  title: string;
  value: string | number;
  change?: number;
  icon: IconName;
  color: "default" | "primary" | "secondary" | "success" | "warning" | "danger";
  subtitle?: string;
  helpText?: string;
}

export const MetricsCard: React.FC<MetricsCardProps> = ({
  title,
  value,
  change,
  icon,
  color,
  subtitle,
  helpText
}) => {
  return (
    <Card className="dashboard-card shadow-sm border-zinc-200 border-1">
      <CardBody className="gap-2">
        <div className="flex justify-between items-start">
          <div>
            <div className="flex items-center gap-1">
              <span className="text-sm text-foreground-500">{title}</span>
              {helpText && (
                <Tooltip content={helpText}>
                  <DynamicIcon name="info" className="text-foreground-400" size={14} />
                </Tooltip>
              )}
            </div>
            <div className="flex items-baseline gap-2 mt-1">
              <span className="text-2xl font-semibold">{value}</span>
              {subtitle && <span className="text-xs text-foreground-500">{subtitle}</span>}
            </div>
          </div>
          <div className={`p-2 rounded-md bg-${color}-100 text-${color}`}>
            <DynamicIcon name={icon} size={20} />
          </div>
        </div>
      </CardBody>
      {change !== undefined && (
        <CardFooter className="pt-0 pb-3 px-4">
          <div className="flex items-center gap-1">
            <DynamicIcon 
              name={change >= 0 ? "trending-up" : "trending-down"} 
              className={change >= 0 ? "text-success" : "text-danger"} 
              size={16} 
            />
            <span className={`text-xs ${change >= 0 ? "text-success" : "text-danger"}`}>
              {Math.abs(change)}% {change >= 0 ? "increase" : "decrease"}
            </span>
            <span className="text-xs text-foreground-400">vs last week</span>
          </div>
        </CardFooter>
      )}
    </Card>
  );
};
