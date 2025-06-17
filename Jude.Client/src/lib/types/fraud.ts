export enum IndicatorStatus {
    Active = 'Active',
    Inactive = 'Inactive',
    Archived = 'Archived'
}

export interface FraudIndicator {
    id: string;
    createdAt: string;
    name: string;
    description: string;
    status: IndicatorStatus;
    createdById: string;
} 