import type { LoadServiceType, LoadStatus } from '@/features/load-board/types';
import { apiClient } from '@/shared/api/client';

export type InvitationStatus = 'Sent' | 'Accepted' | 'Expired' | 'Revoked';

export interface AgentClientView {
    invitationId: string;
    invitedEmail: string;
    status: InvitationStatus;
    invitedAtUtc: string;
    expiresAtUtc: string;
    acceptedByUserId: string | null;
}

export interface AgentClientInvitationResponse {
    invitationId: string;
    invitedEmail: string;
    status: InvitationStatus;
    expiresAtUtc: string;
}

export interface AgentLoadView {
    loadId: string;
    reference: string;
    status: LoadStatus;
    serviceType: LoadServiceType;
    originCity: string;
    originState: string;
    destinationCity: string;
    destinationState: string;
    rateUsd: number;
    projectedAgentCommissionUsd: number | null;
    pickupAtUtc: string;
}

export interface AgentDashboardResponse {
    agentId: string;
    companyName: string;
    status: 'PendingActivation' | 'Active' | 'Suspended';
    commissionRatePercent: number;
    activeLoadsCount: number;
    deliveredLoadsCount: number;
    projectedCommissionUsd: number;
    accruedCommissionUsd: number;
    recentLoads: AgentLoadView[];
}

export const agentApi = {
    /** GET /api/agents/{id}/dashboard */
    getDashboard: (agentId: string): Promise<AgentDashboardResponse> =>
        apiClient.get<AgentDashboardResponse>(`/agents/${agentId}/dashboard`),

    /** GET /api/agents/clients */
    getClients: (): Promise<AgentClientView[]> => apiClient.get<AgentClientView[]>('/agents/clients'),

    /** POST /api/agents/clients/invite */
    inviteClient: (invitedEmail: string): Promise<AgentClientInvitationResponse> =>
        apiClient.post<AgentClientInvitationResponse>('/agents/clients/invite', { invitedEmail }),

    /** POST /api/agents/invitations/{token}/accept */
    acceptInvitation: (token: string): Promise<void> =>
        apiClient.post<void>(`/agents/invitations/${token}/accept`, undefined),
};
