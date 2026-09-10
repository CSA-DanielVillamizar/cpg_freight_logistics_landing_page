import { useCallback, useEffect, useState } from 'react';
import type { AgentClientView } from './agentApi';
import { agentApi } from './agentApi';

export type AgentClientsStatus = 'loading' | 'ready' | 'error';

export function useAgentClients(): {
    status: AgentClientsStatus;
    clients: AgentClientView[];
    refresh: () => void;
} {
    const [status, setStatus] = useState<AgentClientsStatus>('loading');
    const [clients, setClients] = useState<AgentClientView[]>([]);
    const [refreshToken, setRefreshToken] = useState(0);

    useEffect(() => {
        let cancelled = false;
        setStatus('loading');
        agentApi
            .getClients()
            .then((data) => {
                if (!cancelled) {
                    setClients(data);
                    setStatus('ready');
                }
            })
            .catch(() => {
                if (!cancelled) {
                    setStatus('error');
                }
            });
        return () => {
            cancelled = true;
        };
    }, [refreshToken]);

    const refresh = useCallback(() => setRefreshToken((token) => token + 1), []);

    return { status, clients, refresh };
}
