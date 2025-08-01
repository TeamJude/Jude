export enum PolicyStatus {
    Pending = 0,
    Active = 1,
    Failed = 2,
    Archived = 3,
}

export interface Policy {
	id: number;
	name: string;
	documentId: string;
	documentUrl: string;
	status: PolicyStatus;
	createdAt: string;
	updatedAt: string;
	createdById: string;
}