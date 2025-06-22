export enum IndicatorStatus {
    Active = 0,
    Inactive = 1,
    Archived = 2
}

export interface FraudIndicator {
    id: string;
    createdAt: string;
    name: string;
    description: string;
    status: IndicatorStatus;
    createdById: string;
} 