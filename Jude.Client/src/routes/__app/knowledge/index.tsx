import { PolicyDocumentManager } from '@/components/knowledge/knowledge-base';
import { RulesManager } from '@/components/knowledge/rules-manager';
import { Tab, Tabs } from '@heroui/react';
import { createFileRoute } from '@tanstack/react-router'
import  { useState } from 'react';
import { BookOpen, GitBranch, AlertTriangle, Construction } from 'lucide-react';

export const Route = createFileRoute('/__app/knowledge/')({
  component: RouteComponent,
})

function RouteComponent() {
  
  const [selected, setSelected] = useState("policies");

  return (
    <main className="min-h-screen py-6 px-4">
      <div className="max-w-7xl mx-auto px-4 space-y-6">
        <div>
          <h1 className="text-2xl font-semibold text-gray-800">Knowledge Base Management</h1>
          <p className="text-sm text-gray-500">Manage policy documents and processing rules used by the AI Agent</p>
        </div>
        <Tabs 
          aria-label="Knowledge Base Management Tabs" 
          selectedKey={selected} 
          onSelectionChange={(key) => setSelected(String(key))}
          color="primary"
          variant="underlined"
          classNames={{
            tabList: "gap-6",
            cursor: "w-full bg-primary",
            tab: "max-w-fit px-0 h-12",
          }}
        >
          <Tab 
            key="policies" 
            title={
              <div className="flex items-center gap-2">
                <BookOpen width={18} />
                <span>Policy Documents</span>
              </div>
            }
          >
            <PolicyDocumentManager />
          </Tab>
          <Tab 
            key="rules" 
            title={
              <div className="flex items-center gap-2">
                <GitBranch width={18} />
                <span>Processing Rules</span>
              </div>
            }
          >
            <RulesManager />
          </Tab>
          <Tab 
            key="fraud" 
            title={
              <div className="flex items-center gap-2">
                <AlertTriangle width={18} />
                <span>Fraud Criteria</span>
              </div>
            }
          >
            <div className="flex flex-col items-center justify-center p-12 text-center">
              <Construction className="text-foreground-400 mb-4" width={48} />
              <h3 className="text-xl font-medium mb-2">Coming Soon</h3>
              <p className="text-foreground-500 max-w-md">
                The fraud criteria management module is currently under development and will be available in a future update.
              </p>
            </div>
          </Tab>
        </Tabs>
      </div>
    </main>
  )
}
