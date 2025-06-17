export enum RuleStatus {
    Active = 'Active',
    Inactive = 'Inactive',
    Archived = 'Archived'
}

export interface Rule {
    id: string;
    createdAt: string;
    name: string;
    description: string;
    status: RuleStatus;
    priority: number;
    createdById: string;
}