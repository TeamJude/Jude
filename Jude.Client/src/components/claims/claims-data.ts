// Dummy data representing claims for the kanban view
// This structure should match your backend ClaimModel and ClaimDataModel

export const kanbanClaims = [
  {
    id: 'claim-001',
    transactionNumber: 'TN-20250310141735-3791',
    patientName: 'PASSION FRUIT CRIB, Flower',
    membershipNumber: '11067105-0',
    claimAmount: 1.51,
    currency: '$',
    approvedAmount: undefined,
    providerPractice: '12345',
    timeAgo: '2 hours ago',
    itemSummary: '2 products, 0 services',
    status: 'inQueue' as const,
    fraudRiskLevel: 'MEDIUM' as const,
    isFlagged: false,
    source: 'CIMAS',
    ingestedAt: '2025-03-10T14:17:35Z',
    processingStartedAt: undefined,
    lastUpdatedAt: '2025-03-10T14:17:35Z',
    patientDetails: {
      dateOfBirth: '1974-12-01',
      gender: 'F',
      idNumber: 'ID123456789'
    },
    providerDetails: {
      name: 'City Medical Center',
      contactInfo: 'contact@citymedical.com'
    },
    items: [
      {
        type: 'product' as const,
        code: '603790',
        description: 'GAUZE BANDAGE SELVEDGED 15CMX4.5M*20S',
        amount: 0.81,
        status: 'pending'
      },
      {
        type: 'product' as const,
        code: '609150',
        description: 'PARAFFIN GAUZE 10CM X 10CM 1S MC4A',
        amount: 0.70,
        status: 'pending'
      }
    ]
  },
  {
    id: 'claim-002',
    transactionNumber: 'TN-20250310140812-4611',
    patientName: 'SMITH, John',
    membershipNumber: '11153422-1',
    claimAmount: 875.50,
    currency: '$',
    approvedAmount: undefined,
    providerPractice: '67890',
    timeAgo: '45 minutes ago',
    itemSummary: '1 product, 2 services',
    status: 'inProgress' as const,
    fraudRiskLevel: 'LOW' as const,
    isFlagged: false,
    source: 'CIMAS',
    ingestedAt: '2025-03-10T15:30:12Z',
    processingStartedAt: '2025-03-10T15:35:00Z',
    lastUpdatedAt: '2025-03-10T16:15:12Z',
    agentProgress: 65,
    agentReasoning: [
      'Validating medical codes against ICD-10 standards...',
      'Cross-referencing member eligibility database...',
      'Analyzing provider billing patterns...'
    ],
    patientDetails: {
      dateOfBirth: '1985-06-15',
      gender: 'M',
      idNumber: 'ID987654321'
    },
    items: [
      {
        type: 'service' as const,
        code: '02957',
        description: 'DRESSING SIMPLE',
        amount: 300.00,
        status: 'approved'
      },
      {
        type: 'service' as const,
        code: '02958',
        description: 'WOUND CARE CONSULTATION',
        amount: 450.00,
        status: 'reviewing'
      },
      {
        type: 'product' as const,
        code: '603791',
        description: 'SURGICAL GAUZE PREMIUM',
        amount: 125.50,
        status: 'approved'
      }
    ]
  },
  {
    id: 'claim-003',
    transactionNumber: 'TN-20250310135809-4494',
    patientName: 'JOHNSON, Emily',
    membershipNumber: '11098765-2',
    claimAmount: 2450.00,
    currency: '$',
    approvedAmount: 2200.00,
    providerPractice: '54321',
    timeAgo: '3 hours ago',
    itemSummary: '3 products, 1 service',
    status: 'awaitingReview' as const,
    fraudRiskLevel: 'HIGH' as const,
    isFlagged: true,
    source: 'CIMAS',
    ingestedAt: '2025-03-10T13:58:09Z',
    processingStartedAt: '2025-03-10T14:00:00Z',
    lastUpdatedAt: '2025-03-10T15:45:00Z',
    patientDetails: {
      dateOfBirth: '1992-03-22',
      gender: 'F',
      idNumber: 'ID456789123'
    },
    items: [
      {
        type: 'service' as const,
        code: '12345',
        description: 'SPECIALIST CONSULTATION',
        amount: 1500.00,
        status: 'flagged'
      },
      {
        type: 'product' as const,
        code: '67890',
        description: 'ADVANCED MEDICAL DEVICE',
        amount: 800.00,
        status: 'approved'
      },
      {
        type: 'product' as const,
        code: '11111',
        description: 'PRESCRIPTION MEDICATION',
        amount: 150.00,
        status: 'approved'
      }
    ]
  },
  {
    id: 'claim-004',
    transactionNumber: 'TN-20250310142820-1234',
    patientName: 'WILLIAMS, Robert',
    membershipNumber: '11234567-0',
    claimAmount: 125.75,
    currency: '$',
    approvedAmount: undefined,
    providerPractice: '98765',
    timeAgo: '1 hour ago',
    itemSummary: '1 product',
    status: 'inQueue' as const,
    fraudRiskLevel: 'LOW' as const,
    isFlagged: false,
    source: 'CIMAS',
    ingestedAt: '2025-03-10T14:28:20Z',
    lastUpdatedAt: '2025-03-10T14:28:20Z',
    patientDetails: {
      dateOfBirth: '1978-11-08',
      gender: 'M',
      idNumber: 'ID789123456'
    },
    items: [
      {
        type: 'product' as const,
        code: '55555',
        description: 'BASIC MEDICAL SUPPLIES',
        amount: 125.75,
        status: 'pending'
      }
    ]
  },
  {
    id: 'claim-005',
    transactionNumber: 'TN-20250310143015-5678',
    patientName: 'BROWN, Sarah',
    membershipNumber: '11345678-1',
    claimAmount: 3200.00,
    currency: '$',
    approvedAmount: undefined,
    providerPractice: '11111',
    timeAgo: '30 minutes ago',
    itemSummary: '2 services, 4 products',
    status: 'inProgress' as const,
    fraudRiskLevel: 'CRITICAL' as const,
    isFlagged: true,
    source: 'CIMAS',
    ingestedAt: '2025-03-10T14:30:15Z',
    processingStartedAt: '2025-03-10T14:32:00Z',
    lastUpdatedAt: '2025-03-10T16:00:15Z',
    agentProgress: 85,
    agentReasoning: [
      'Detected unusual billing pattern...',
      'Cross-checking provider credentials...',
      'Analyzing claim frequency for this patient...',
      'Reviewing medical necessity indicators...',
      'Calculating fraud risk score...'
    ],
    patientDetails: {
      dateOfBirth: '1965-09-12',
      gender: 'F',
      idNumber: 'ID321654987'
    },
    items: [
      {
        type: 'service' as const,
        code: '99999',
        description: 'COMPLEX SURGICAL PROCEDURE',
        amount: 2000.00,
        status: 'flagged'
      },
      {
        type: 'service' as const,
        code: '88888',
        description: 'POST-OP CONSULTATION',
        amount: 500.00,
        status: 'reviewing'
      }
    ]
  },
  {
    id: 'claim-006',
    transactionNumber: 'TN-20250310144122-9999',
    patientName: 'DAVIS, Michael',
    membershipNumber: '11456789-3',
    claimAmount: 450.25,
    currency: '$',
    approvedAmount: 450.25,
    providerPractice: '22222',
    timeAgo: '4 hours ago',
    itemSummary: '2 products',
    status: 'awaitingReview' as const,
    fraudRiskLevel: 'LOW' as const,
    isFlagged: false,
    source: 'CIMAS',
    ingestedAt: '2025-03-10T14:41:22Z',
    processingStartedAt: '2025-03-10T14:45:00Z',
    lastUpdatedAt: '2025-03-10T15:30:00Z',
    patientDetails: {
      dateOfBirth: '1990-04-18',
      gender: 'M',
      idNumber: 'ID654321987'
    },
    items: [
      {
        type: 'product' as const,
        code: '33333',
        description: 'ROUTINE MEDICAL SUPPLIES',
        amount: 225.00,
        status: 'approved'
      },
      {
        type: 'product' as const,
        code: '44444',
        description: 'ADDITIONAL SUPPLIES',
        amount: 225.25,
        status: 'approved'
      }
    ]
  }
];

// Helper function to get claims by status for easy filtering
export const getClaimsByStatus = (status: 'inQueue' | 'inProgress' | 'awaitingReview') => {
  return kanbanClaims.filter(claim => claim.status === status);
};

// Helper function to get summary statistics
export const getClaimsStats = () => {
  const totalClaims = kanbanClaims.length;
  const totalAmount = kanbanClaims.reduce((sum, claim) => sum + claim.claimAmount, 0);
  const highRiskClaims = kanbanClaims.filter(claim => 
    claim.fraudRiskLevel === 'HIGH' || claim.fraudRiskLevel === 'CRITICAL'
  ).length;
  const flaggedClaims = kanbanClaims.filter(claim => claim.isFlagged).length;

  return {
    total: totalClaims,
    inQueue: getClaimsByStatus('inQueue').length,
    inProgress: getClaimsByStatus('inProgress').length,
    awaitingReview: getClaimsByStatus('awaitingReview').length,
    totalAmount,
    highRiskCount: highRiskClaims,
    flaggedCount: flaggedClaims
  };
}; 