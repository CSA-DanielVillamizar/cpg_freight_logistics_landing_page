import type { BadgeTone } from '@/shared/ui';
import { Badge, Button, Card, EmptyState } from '@/shared/ui';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { AgentLoadTable } from './AgentLoadTable';
import { CommissionSummaryCard } from './CommissionSummaryCard';
import { InviteClientModal } from './InviteClientModal';
import { useAgentDashboard } from './useAgentDashboard';

const STATUS_TONE: Record<'PendingActivation' | 'Active' | 'Suspended', BadgeTone> = {
    PendingActivation: 'dispatched',
    Active: 'delivered',
    Suspended: 'rejected',
};

/** /agent — the Independent Agent's commission dashboard (T-SDD Epica 4). */
export function AgentDashboardPage(): JSX.Element {
    const { status, dashboard } = useAgentDashboard();
    const [inviteOpen, setInviteOpen] = useState(false);

    return (
        <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-8">
            <header className="flex flex-wrap items-start justify-between gap-3">
                <div className="flex flex-col gap-1">
                    <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                        Independent Agent Workspace
                    </span>
                    <h1 className="text-headline-lg">{dashboard?.companyName ?? 'Your agency dashboard'}</h1>
                </div>
                <div className="flex items-center gap-2">
                    <Button variant="outline" onClick={() => setInviteOpen(true)}>
                        Invite a client
                    </Button>
                    <Link to="/agent/loads/new">
                        <Button variant="primary">Publish a load</Button>
                    </Link>
                </div>
            </header>

            {status === 'error' ? (
                <Card className="border-error bg-error-container p-4 text-body-sm text-error">
                    Unable to load your dashboard right now.
                </Card>
            ) : status === 'not-activated' ? (
                <EmptyState
                    icon="hourglass_empty"
                    title="Your agency is pending activation"
                    hint="CPG dispatch will notify you once your agency checklist is approved. You can invite clients and publish loads once activated."
                />
            ) : status === 'loading' || !dashboard ? (
                <EmptyState icon="progress_activity" title="Loading dashboard…" />
            ) : (
                <>
                    <div className="flex items-center gap-3">
                        <span className="text-body-sm text-steel-gray">Account status</span>
                        <Badge tone={STATUS_TONE[dashboard.status]}>{dashboard.status}</Badge>
                        <span className="text-body-sm text-steel-gray">
                            Commission rate:{' '}
                            <span className="font-semibold text-on-surface">{dashboard.commissionRatePercent}%</span>
                        </span>
                    </div>

                    <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                        <CommissionSummaryCard
                            label="Accrued commission"
                            valueUsd={dashboard.accruedCommissionUsd}
                            accent="primary"
                            hint="Paid out via Stripe Connect"
                        />
                        <CommissionSummaryCard
                            label="Projected commission"
                            valueUsd={dashboard.projectedCommissionUsd}
                            hint="Loads not yet settled"
                        />
                        <Card className="flex flex-col justify-center gap-1 p-5">
                            <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                                Active loads
                            </span>
                            <span className="font-heading text-headline-lg tabular-nums text-fleet-blue">
                                {dashboard.activeLoadsCount}
                            </span>
                        </Card>
                        <Card className="flex flex-col justify-center gap-1 p-5">
                            <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                                Delivered loads
                            </span>
                            <span className="font-heading text-headline-lg tabular-nums text-fleet-blue">
                                {dashboard.deliveredLoadsCount}
                            </span>
                        </Card>
                    </div>

                    <div className="flex items-center justify-between">
                        <h2 className="text-headline-sm">Recent loads</h2>
                        <Link to="/agent/clients" className="text-xs font-semibold uppercase tracking-wider text-fleet-blue">
                            Manage clients &rarr;
                        </Link>
                    </div>
                    <AgentLoadTable loads={dashboard.recentLoads} />
                </>
            )}

            <InviteClientModal open={inviteOpen} onClose={() => setInviteOpen(false)} onInvited={() => undefined} />
        </div>
    );
}
