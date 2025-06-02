import { ClaimsTable } from '@/components/claims/claims-table';
import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/__app/claims/')({
  component: RouteComponent,
})

function RouteComponent() {
  return (
    <main className="min-h-screen py-6 px-4">
      <div className="max-w-7xl mx-auto px-4 space-y-6">
        {/* Header with Title and Actions */}
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-semibold text-gray-800">Claims</h1>
            <p className="text-sm text-gray-500">Real-time overview of claims processing</p>
          </div>

        </div>
 
      <div className='mt-10 w-full border-zinc-200 border-1 rounded-lg p-6'>

                <ClaimsTable />
      </div>

      </div>
    </main>
  )
}
