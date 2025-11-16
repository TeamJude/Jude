import { getClaimAuditLogs } from "@/lib/services/claims.service";
import { AuditActorType, type GetClaimDetailResponse } from "@/lib/types/claim";
import { Card, CardBody, Chip, type ChipProps, Spinner } from "@heroui/react";
import { useQuery } from "@tanstack/react-query";
import { Cpu, Inbox, User } from "lucide-react";
import React from "react";

interface ClaimAuditTabProps {
	claim: GetClaimDetailResponse;
}

const getActorIcon = (actorType: AuditActorType) => {
	switch (actorType) {
		case AuditActorType.User:
			return <User className="w-4 h-4" />;
		case AuditActorType.AiAgent:
			return <Cpu className="w-4 h-4" />;
		case AuditActorType.System:
			return <Inbox className="w-4 h-4" />;
		default:
			return <Inbox className="w-4 h-4" />;
	}
};

const getActorColor = (actorType: AuditActorType): ChipProps["color"] => {
	switch (actorType) {
		case AuditActorType.User:
			return "default";
		case AuditActorType.AiAgent:
			return "secondary";
		case AuditActorType.System:
			return "primary";
		default:
			return "default";
	}
};

const getActorLabel = (actorType: AuditActorType): string => {
	switch (actorType) {
		case AuditActorType.User:
			return "User";
		case AuditActorType.AiAgent:
			return "AI Agent";
		case AuditActorType.System:
			return "System";
		default:
			return "Unknown";
	}
};

export const ClaimAuditTab: React.FC<ClaimAuditTabProps> = ({ claim }) => {
	const {
		data: auditLogsResponse,
		isLoading,
		error,
	} = useQuery({
		queryKey: ["claimAuditLogs", claim.id],
		queryFn: () => getClaimAuditLogs(claim.id),
	});

	if (isLoading) {
		return (
			<Card shadow="none">
				<CardBody>
					<div className="flex items-center justify-center p-8">
						<Spinner label="Loading audit logs..." />
					</div>
				</CardBody>
			</Card>
		);
	}

	if (error || !auditLogsResponse?.success) {
		return (
			<Card shadow="none">
				<CardBody>
					<div className="flex flex-col items-center justify-center p-8">
						<p className="text-danger">Failed to load audit logs</p>
					</div>
				</CardBody>
			</Card>
		);
	}

	const auditLogs = auditLogsResponse.data.auditLogs;

	if (auditLogs.length === 0) {
		return (
			<Card shadow="none">
				<CardBody>
					<h3 className="text-lg font-medium mb-4">Claim Activity Log</h3>
					<div className="text-center py-8 text-foreground-500">
						<p>No audit logs available for this claim.</p>
					</div>
				</CardBody>
			</Card>
		);
	}

	return (
		<Card shadow="none">
			<CardBody>
				<h3 className="text-lg font-medium mb-4">Claim Activity Log</h3>

				<div className="space-y-6">
					{auditLogs.map((log, index) => (
						<div key={log.id} className="flex gap-4">
							<div className="flex flex-col items-center">
								<div
									className={`w-8 h-8 rounded-full flex items-center justify-center text-white ${
										log.actorType === AuditActorType.User
											? "bg-default"
											: log.actorType === AuditActorType.AiAgent
												? "bg-secondary"
												: "bg-primary"
									}`}
								>
									{getActorIcon(log.actorType)}
								</div>
								{index < auditLogs.length - 1 && (
									<div className="flex-grow w-0.5 bg-divider my-2"></div>
								)}
							</div>
							<div className="flex-1">
								<div className="flex items-center gap-2 mb-1">
									<h4 className="font-medium">{log.action}</h4>
									<Chip
										size="sm"
										variant="flat"
										color={getActorColor(log.actorType)}
									>
										{log.actorName
											? `${getActorLabel(log.actorType)}: ${log.actorName}`
											: getActorLabel(log.actorType)}
									</Chip>
								</div>
								<p className="text-sm text-foreground-500 mb-2">
									{new Date(log.timestamp).toLocaleString()}
								</p>
								<p className="text-sm">{log.description}</p>
							</div>
						</div>
					))}
				</div>
			</CardBody>
		</Card>
	);
};
