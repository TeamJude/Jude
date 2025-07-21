import { Card, CardBody, CardHeader } from "@heroui/react";
import type React from "react";
import {
	Area,
	AreaChart,
	CartesianGrid,
	Legend,
	ResponsiveContainer,
	Tooltip,
	XAxis,
	YAxis,
} from "recharts";

interface ClaimsChartProps {
	title: string;
	data: Array<{
		day: string;
		newClaims: number;
		processed: number;
		approved: number;
		rejected: number;
	}>;
	isLoading?: boolean;
}

export const ClaimsChart: React.FC<ClaimsChartProps> = ({ title, data, isLoading }) => {
	const chartData = data?.map((d) => ({
		date: d.day,
		newClaims: d.newClaims,
		processed: d.processed,
		approved: d.approved,
		rejected: d.rejected,
	})) ?? [];

	return (
		<Card className="shadow-sm ">
			<CardHeader className="pb-0 pt-4 px-4 flex-col items-start">
				<h4 className="font-medium text-large">{title}</h4>
				<p className="text-small text-foreground-500">
					Claims activity for the selected period
				</p>
			</CardHeader>
			<CardBody className="overflow-visible py-2">
				{isLoading ? (
					<div className="flex items-center justify-center h-[300px]">Loading...</div>
				) : (
					<ResponsiveContainer width="100%" height={300}>
						<AreaChart
							data={chartData}
							margin={{
								top: 10,
								right: 30,
								left: 0,
								bottom: 0,
							}}
						>
							<CartesianGrid
								strokeDasharray="3 3"
								vertical={false}
								stroke="hsl(var(--heroui-default-200))"
							/>
							<XAxis
								dataKey="date"
								axisLine={false}
								tickLine={false}
								style={{ fontSize: "var(--heroui-font-size-tiny)" }}
							/>
							<YAxis
								axisLine={false}
								tickLine={false}
								style={{ fontSize: "var(--heroui-font-size-tiny)" }}
							/>
							<Tooltip
								contentStyle={{
									backgroundColor: "hsl(var(--heroui-content1))",
									borderColor: "hsl(var(--heroui-divider))",
									borderRadius: "var(--heroui-radius-medium)",
									boxShadow: "var(--heroui-shadow-small)",
								}}
								labelStyle={{ fontWeight: 600, marginBottom: "4px" }}
							/>
							<Legend />
							<Area type="monotone" dataKey="newClaims" name="New Claims" stroke="hsl(var(--heroui-primary))" fill="hsl(var(--heroui-primary-100))" strokeWidth={2} activeDot={{ r: 6 }} />
							<Area type="monotone" dataKey="processed" name="Processed" stroke="hsl(var(--heroui-secondary))" fill="hsl(var(--heroui-secondary-100))" strokeWidth={2} activeDot={{ r: 6 }} />
							<Area type="monotone" dataKey="approved" name="Approved" stroke="hsl(var(--heroui-success))" fill="hsl(var(--heroui-success-100))" strokeWidth={2} activeDot={{ r: 6 }} />
							<Area type="monotone" dataKey="rejected" name="Rejected" stroke="hsl(var(--heroui-danger))" fill="hsl(var(--heroui-danger-100))" strokeWidth={2} activeDot={{ r: 6 }} />
						</AreaChart>
					</ResponsiveContainer>
				)}
			</CardBody>
		</Card>
	);
};
