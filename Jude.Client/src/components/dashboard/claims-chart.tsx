import type React from 'react';
import { Card, CardBody, CardHeader } from '@heroui/react';
import { 
  AreaChart, 
  Area, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  Legend
} from 'recharts';

interface ClaimsChartProps {
  title: string;
}

export const ClaimsChart: React.FC<ClaimsChartProps> = ({ title }) => {
  const data = [
    { date: 'Mon', newClaims: 42, processed: 38, approved: 30, rejected: 8 },
    { date: 'Tue', newClaims: 53, processed: 45, approved: 36, rejected: 9 },
    { date: 'Wed', newClaims: 38, processed: 42, approved: 32, rejected: 10 },
    { date: 'Thu', newClaims: 47, processed: 43, approved: 35, rejected: 8 },
    { date: 'Fri', newClaims: 61, processed: 52, approved: 43, rejected: 9 },
    { date: 'Sat', newClaims: 29, processed: 31, approved: 25, rejected: 6 },
    { date: 'Sun', newClaims: 18, processed: 24, approved: 19, rejected: 5 },
  ];

  return (
    <Card className="shadow-sm ">
      <CardHeader className="pb-0 pt-4 px-4 flex-col items-start">
        <h4 className="font-medium text-large">{title}</h4>
        <p className="text-small text-foreground-500">Claims activity for the past week</p>
      </CardHeader>
      <CardBody className="overflow-visible py-2">
        <ResponsiveContainer width="100%" height={300}>
          <AreaChart
            data={data}
            margin={{
              top: 10,
              right: 30,
              left: 0,
              bottom: 0,
            }}
          >
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="hsl(var(--heroui-default-200))" />
            <XAxis 
              dataKey="date" 
              axisLine={false}
              tickLine={false}
              style={{
                fontSize: "var(--heroui-font-size-tiny)",
              }}
            />
            <YAxis 
              axisLine={false}
              tickLine={false}
              style={{
                fontSize: "var(--heroui-font-size-tiny)",
              }}
            />
            <Tooltip
              contentStyle={{
                backgroundColor: "hsl(var(--heroui-content1))",
                borderColor: "hsl(var(--heroui-divider))",
                borderRadius: "var(--heroui-radius-medium)",
                boxShadow: "var(--heroui-shadow-small)",
              }}
              labelStyle={{
                fontWeight: 600,
                marginBottom: "4px",
              }}
            />
            <Legend />
            <Area 
              type="monotone" 
              dataKey="newClaims" 
              name="New Claims"
              stroke="hsl(var(--heroui-primary))" 
              fill="hsl(var(--heroui-primary-100))" 
              strokeWidth={2}
              activeDot={{ r: 6 }}
            />
            <Area 
              type="monotone" 
              dataKey="processed" 
              name="Processed"
              stroke="hsl(var(--heroui-secondary))" 
              fill="hsl(var(--heroui-secondary-100))" 
              strokeWidth={2}
              activeDot={{ r: 6 }}
            />
            <Area 
              type="monotone" 
              dataKey="approved" 
              name="Approved"
              stroke="hsl(var(--heroui-success))" 
              fill="hsl(var(--heroui-success-100))" 
              strokeWidth={2}
              activeDot={{ r: 6 }}
            />
            <Area 
              type="monotone" 
              dataKey="rejected" 
              name="Rejected"
              stroke="hsl(var(--heroui-danger))" 
              fill="hsl(var(--heroui-danger-100))" 
              strokeWidth={2}
              activeDot={{ r: 6 }}
            />
          </AreaChart>
        </ResponsiveContainer>
      </CardBody>
    </Card>
  );
};
