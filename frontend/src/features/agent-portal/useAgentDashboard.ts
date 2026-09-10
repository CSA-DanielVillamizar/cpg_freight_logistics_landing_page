import { authApi } from '@/features/auth/authApi';
import { ApiError } from '@/shared/api/client';
import { useEffect, useState } from 'react';
import type { AgentDashboardResponse } from './agentApi';
import { agentApi } from './agentApi';

export type AgentDashboardStatus = 'loading' | 'ready' | 'error' | 'not-activated';

/** Resolves the caller's own Agent id via GET /api/me, then loads their dashboard. */
export function useAgentDashboard(): {
    status: AgentDashboardStatus;
    dashboard: AgentDashboardResponse | null;
} {
    const [status, setStatus] = useState<AgentDashboardStatus>('loading');
    const [dashboard, setDashboard] = useState<AgentDashboardResponse | null>(null);

    useEffect(() => {
        let cancelled = false;

        authApi
            .getMe()
            .then((profile) => {
                if (cancelled) {
                    return undefined;
                }
                if (!profile.agent) {
                    setStatus('not-activated');
                    return undefined;
                }
                return agentApi.getDashboard(profile.agent.agentId);
            })
            .then((data) => {
                if (!cancelled && data) {
                    setDashboard(data);
                    setStatus('ready');
                }
            })
            .catch((caught: unknown) => {
                if (!cancelled) {
                    setStatus(caught instanceof ApiError ? 'error' : 'error');
                }
            });

        return () => {
            cancelled = true;
        };
    }, []);

    return { status, dashboard };
}
