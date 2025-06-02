import { ClaimDetailHeader } from '@/components/claims/claim-detail-header';
import { ClaimTabs } from '@/components/claims/claims-tab';
import { createFileRoute } from '@tanstack/react-router'
import { Card, CardBody } from '@heroui/react';

export const Route = createFileRoute('/__app/claims/$id/')({
  component: RouteComponent,
  validateSearch: (search: Record<string, unknown>) => {
    return search;
  },
  loader: async ({ params }) => {
    // In a real application, this would fetch the claim details from an API
    return {
      claimId: params.id,
      memberName: 'John Smith',
      memberId: 'MEM-12345',
      providerName: 'City Medical Center',
      providerId: 'PRV-28765',
      dateReceived: '2023-05-15',
      dateOfService: '2023-05-10',
      amount: 1250,
      status: 'Pending Human Review'
    };
  }
});

function RouteComponent() {
  const { claimId, memberName, memberId, providerName, providerId, dateReceived, dateOfService, amount, status } = Route.useLoaderData();

  return (
    <main className="min-h-screen py-6 px-4">
      <div className="max-w-7xl mx-auto px-4 space-y-6">
        {/* Header with Title and Actions */}
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-semibold text-gray-800">Claim Details</h1>
            <p className="text-sm text-gray-500">Real-time overview of claims processing</p>
          </div>

        </div>
        <div className="space-y-6">
          <ClaimDetailHeader 
            claimId={claimId}
            memberName={memberName}
            memberId={memberId}
            providerName={providerName}
            providerId={providerId}
            dateReceived={dateReceived}
            dateOfService={dateOfService}
            amount={amount}
            status={status}
          />
          <div className="mt-6 border border-zinc-200 rounded-lg p-4 bg-white">
            <ClaimTabs claimId={claimId} />
          </div>
       
    </div>

      </div>
    </main>

    
    


  
  );
}
