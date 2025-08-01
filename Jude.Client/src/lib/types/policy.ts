export enum PolicyStatus {
	Active = 0,
	Archived = 1,
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