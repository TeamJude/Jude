import { createFileRoute } from '@tanstack/react-router';
import { 
  Card, 
  CardBody, 
  CardHeader,
  Button,
  Tabs,
  Tab,
  Dropdown,
  DropdownTrigger,
  DropdownMenu,
  DropdownItem,
  Select,
  SelectItem
} from '@heroui/react';
import { DynamicIcon } from 'lucide-react/dynamic';

// Import existing components
import { ClaimsChart } from '@/components/dashboard/claims-chart';
import { MetricsCard } from '@/components/dashboard/metric-card';
import { CircleChartCard } from '@/components/dashboard/circle-chart-card';
import { RecentClaimsTable } from '@/components/dashboard/recent-claims-table';



export const Route = createFileRoute('/__app/dashboard/')({
  component: Dashboard,
});

function Dashboard() {
  const categoryChartData = [
    {
      title: "Claim Categories",
      categories: ["Outpatient", "Inpatient", "Pharmacy", "Dental"],
      color: "primary",
      chartData: [
        {name: "Outpatient", value: 420},
        {name: "Inpatient", value: 280},
        {name: "Pharmacy", value: 190},
        {name: "Dental", value: 110},
      ],
    },
    {
      title: "Claim Sources",
      categories: ["Providers", "Members", "Partners", "Other"],
      color: "warning",
      chartData: [
        {name: "Providers", value: 580},
        {name: "Members", value: 240},
        {name: "Partners", value: 130},
        {name: "Other", value: 50},
      ],
    },
    {
      title: "Adjudication Types",
      categories: ["Auto-Approved", "Manual Review", "Flagged", "Rejected"],
      color: "danger",
      chartData: [
        {name: "Auto-Approved", value: 650},
        {name: "Manual Review", value: 200},
        {name: "Flagged", value: 100},
        {name: "Rejected", value: 50},
      ],
    },
  ];
  
  return (
    <main className="min-h-screen  py-6 px-4">
      <div className="max-w-7xl mx-auto px-4 space-y-6">
        {/* Header with Title and Actions */}
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-semibold text-gray-800">Claims Dashboard</h1>
            <p className="text-sm text-gray-500">Real-time overview of claims processing</p>
          </div>
          <div className="flex items-center gap-3">
            <Select 
              aria-label="Date Range"
              size="sm"
              defaultSelectedKeys={["week"]}
              className="w-40"
            >
              <SelectItem key="today">Today</SelectItem>
              <SelectItem key="week">Last 7 Days</SelectItem>
              <SelectItem key="month">Last 30 Days</SelectItem>
              <SelectItem key="quarter">Last Quarter</SelectItem>
            </Select>
            <Button color="primary" startContent={<DynamicIcon name="refresh-cw" size={16} />}>
              Refresh
            </Button>
            <Dropdown>
              <DropdownTrigger>
                <Button variant="flat" startContent={<DynamicIcon name="download" size={16} />}>
                  Export
                </Button>
              </DropdownTrigger>
              <DropdownMenu aria-label="Export Options">
                <DropdownItem key="pdf">PDF Report</DropdownItem>
                <DropdownItem key="excel">Excel Spreadsheet</DropdownItem>
                <DropdownItem key="csv">CSV Data</DropdownItem>
              </DropdownMenu>
            </Dropdown>
          </div>
        </div>

        {/* Summary Metrics Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
          <MetricsCard 
            title="Total Claims" 
            value="1,525" 
            change={8} 
            icon="file-text" 
            color="primary"
            helpText="Total number of claims received in the selected period"
          />
          <MetricsCard 
            title="Auto-Approved Rate" 
            value="65%" 
            change={3} 
            icon="check-circle" 
            color="success"
            helpText="Percentage of claims automatically approved by the AI system"
          />
          <MetricsCard 
            title="Avg. Processing Time" 
            value="2.4" 
            subtitle="minutes"
            change={-12} 
            icon="clock" 
            color="secondary"
            helpText="Average time from claim submission to final decision"
          />
          <MetricsCard 
            title="Claims Flagged" 
            value="47" 
            change={5} 
            icon="alert-triangle" 
            color="warning"
            helpText="Claims flagged for potential fraud or policy violations"
          />
        </div>

               {/* Main Charts Section */}
               <div className="w-full">
          {/* Weekly Activity Chart - Spans 2 columns */}
          <Card className="lg:col-span-2 shadow-sm  border-zinc-200 border-1">
            <CardHeader className="flex justify-between items-center pb-2">
              <div>
                <h3 className="text-base font-medium">Claims Activity</h3>
                <p className="text-xs text-gray-500">Daily claim volume and processing status</p>
              </div>
              <Tabs size="sm" aria-label="Time periods">
                <Tab key="weekly" title="Weekly" />
                <Tab key="monthly" title="Monthly" />
                <Tab key="quarterly" title="Quarterly" />
              </Tabs>
            </CardHeader>
            <CardBody>
              <ClaimsChart title="" />
            </CardBody>
          </Card>
        </div>


        {/* Recent Claims Table */}
        <Card className="shadow-sm p-2 border-zinc-200 border-1">
          <CardHeader className="flex justify-between items-center pb-2">
            <div>
              <h3 className="text-base font-medium">Recent Claims</h3>
              <p className="text-xs text-gray-500">Latest claims requiring attention</p>
            </div>
          </CardHeader>
          <CardBody>
            <RecentClaimsTable />
          </CardBody>
        </Card>


 
        {/* Category Charts */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          {categoryChartData.map((chart, index) => (
            <CircleChartCard 
              key={index}
              title={chart.title}
              categories={chart.categories}
              color={chart.color as "primary" | "warning" | "danger" | "default" | "secondary" | "success"}
              chartData={chart.chartData}
            />
          ))}
        </div>

        {/* AI Performance Metrics */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          <Card className="shadow-sm">
            <CardHeader className="pb-2">
              <h3 className="text-base font-medium">AI Agent Performance</h3>
              <p className="text-xs text-gray-500">Model accuracy and confidence metrics</p>
            </CardHeader>
            <CardBody>
              <div className="space-y-4">
                <div>
                  <div className="flex justify-between mb-2">
                    <span className="text-sm">Model Accuracy</span>
                    <span className="text-sm font-medium">94%</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-2">
                    <div className="bg-blue-500 h-2 rounded-full" style={{ width: '94%' }}></div>
                  </div>
                  <p className="text-xs text-gray-500 mt-1">
                    Percentage of correct adjudication decisions vs. human reviewers
                  </p>
                </div>
                
                <div>
                  <div className="flex justify-between mb-2">
                    <span className="text-sm">Fraud Detection Rate</span>
                    <span className="text-sm font-medium">89%</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-2">
                    <div className="bg-green-500 h-2 rounded-full" style={{ width: '89%' }}></div>
                  </div>
                  <p className="text-xs text-gray-500 mt-1">
                    Percentage of fraudulent claims correctly identified
                  </p>
                </div>
                
                <div>
                  <div className="flex justify-between mb-2">
                    <span className="text-sm">False Positive Rate</span>
                    <span className="text-sm font-medium">3.2%</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-2">
                    <div className="bg-yellow-500 h-2 rounded-full" style={{ width: '3.2%' }}></div>
                  </div>
                  <p className="text-xs text-gray-500 mt-1">
                    Percentage of legitimate claims incorrectly flagged as suspicious
                  </p>
                </div>
              </div>
            </CardBody>
          </Card>
          
          <Card className="shadow-sm">
            <CardHeader className="pb-2">
              <h3 className="text-base font-medium">Processing Queue Status</h3>
              <p className="text-xs text-gray-500">Current claims awaiting review</p>
            </CardHeader>
            <CardBody>
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-500">Awaiting First Review</div>
                  <div className="text-2xl font-semibold mt-1">27</div>
                  <div className="flex items-center mt-2">
                    <DynamicIcon name="clock" className="text-yellow-500" size={16} />
                    <span className="text-xs text-gray-500 ml-1">Avg wait: 14 mins</span>
                  </div>
                </div>
                
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-500">Under Investigation</div>
                  <div className="text-2xl font-semibold mt-1">12</div>
                  <div className="flex items-center mt-2">
                    <DynamicIcon name="alert-circle" className="text-red-500" size={16} />
                    <span className="text-xs text-gray-500 ml-1">High priority: 5</span>
                  </div>
                </div>
                
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-500">Pending Documentation</div>
                  <div className="text-2xl font-semibold mt-1">18</div>
                  <div className="flex items-center mt-2">
                    <DynamicIcon name="file-text" className="text-blue-500" size={16} />
                    <span className="text-xs text-gray-500 ml-1">Overdue: 3</span>
                  </div>
                </div>
                
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-500">Ready for Payment</div>
                  <div className="text-2xl font-semibold mt-1">43</div>
                  <div className="flex items-center mt-2">
                    <DynamicIcon name="check-circle" className="text-green-500" size={16} />
                    <span className="text-xs text-gray-500 ml-1">Total: $127,450</span>
                  </div>
                </div>
              </div>
            </CardBody>
          </Card>
        </div>
      </div>
    </main>
  );
}