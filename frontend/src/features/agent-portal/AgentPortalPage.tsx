import { authApi } from '@/features/auth/authApi';
import { ApiError } from '@/shared/api/client';
import type { AgentProfileSummary } from '@/shared/api/types';
import type { BadgeTone } from '@/shared/ui';
import { Badge, Card, EmptyState } from '@/shared/ui';
import { useEffect, useState } from 'react';

const STATUS_TONE: Record<AgentProfileSummary['status'], BadgeTone> = {
    PendingActivation: 'dispatched',
    Active: 'delivered',
    Suspended: 'rejected',
};

/**
 * /agent — placeholder workspace shown right after Agent sign-up. The full commission
 * dashboard, client invitations and load attribution land with T-SDD Epica 4.
 */
export function AgentPortalPage(): JSX.Element {
    const [agent, setAgent] = useState<AgentProfileSummary | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        authApi
            .getMe()
            .then((profile) => setAgent(profile.agent ?? null))
            .catch((caught) => {
                setError(caught instanceof ApiError ? caught.message : 'Unable to load your agent profile.');
            });
    }, []);

    return (
        <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-16">
            <header className="flex flex-col gap-1">
                <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    Independent Agent Workspace
                </span>
                <h1 className="text-headline-lg">
                    {agent?.companyName ?? 'Your agency dashboard'}
                </h1>
            </header>

            {error ? (
                <Card className="p-6">
                    <p className="text-body-sm text-error">{error}</p>
                </Card>
            ) : (
                <Card raised className="flex flex-col gap-4 p-6">
                    {agent ? (
                        <>
                            <div className="flex items-center gap-3">
                                <span className="text-body-sm text-steel-gray">Account status</span>
                                <Badge tone={STATUS_TONE[agent.status]}>{agent.status}</Badge>
                            </div>
                            <p className="text-body-sm text-steel-gray">
                                Commission rate: <span className="font-semibold text-on-surface">{agent.commissionRatePercent}%</span>
                            </p>
                        </>
                    ) : null}
                    <EmptyState
                        icon="handshake"
                        title="Agency Load Board is on the way"
                        hint="Client invitations, load attribution and live commission tracking arrive with the next release. CPG dispatch will notify you once your agency is activated."
                    />
                </Card>
            )}
        </div>
    );
}
