export enum RuleStatus {
    Active = 0,
    Inactive = 1,
    Archived = 2
}

export interface Rule {
    id: string;
    createdAt: string;
    name: string;
    description: string;
    status: RuleStatus;
    createdById: string;
}