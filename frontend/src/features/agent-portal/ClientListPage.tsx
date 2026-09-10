import { formatEnum } from '@/shared/lib/formatEnum';
import type { BadgeTone } from '@/shared/ui';
import { Badge, Button, Card, EmptyState } from '@/shared/ui';
import { useState } from 'react';
import type { InvitationStatus } from './agentApi';
import { InviteClientModal } from './InviteClientModal';
import { useAgentClients } from './useAgentClients';

const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
const formatDate = (iso: string): string => dateFormatter.format(new Date(iso));

const STATUS_TONE: Record<InvitationStatus, BadgeTone> = {
    Sent: 'dispatched',
    Accepted: 'delivered',
    Expired: 'rejected',
    Revoked: 'rejected',
};

/** /agent/clients — the Agent's roster of invited and accepted clients (T-SDD Epica 4). */
export function ClientListPage(): JSX.Element {
    const { status, clients, refresh } = useAgentClients();
    const [inviteOpen, setInviteOpen] = useState(false);

    return (
        <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-8">
            <header className="flex flex-wrap items-start justify-between gap-3">
                <div className="flex flex-col gap-1">
                    <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                        Independent Agent Workspace
                    </span>
                    <h1 className="text-headline-lg">Clients</h1>
                </div>
                <Button variant="primary" onClick={() => setInviteOpen(true)}>
                    Invite a client
                </Button>
            </header>

            {status === 'error' ? (
                <Card className="border-error bg-error-container p-4 text-body-sm text-error">
                    Unable to load your clients right now.
                </Card>
            ) : status === 'loading' ? (
                <EmptyState icon="progress_activity" title="Loading clients…" />
            ) : clients.length === 0 ? (
                <EmptyState
                    icon="group_add"
                    title="No clients invited yet"
                    hint="Invite a shipper to start publishing loads on their behalf."
                />
            ) : (
                <div className="overflow-x-auto rounded-lg border border-slate-200 bg-surface-card shadow-sm">
                    <table className="w-full min-w-[640px] text-left">
                        <thead>
                            <tr className="border-b border-slate-200 bg-surface-muted">
                                {['Email', 'Invited', 'Expires', 'Status'].map((heading) => (
                                    <th
                                        key={heading}
                                        className="whitespace-nowrap px-3 py-2.5 text-[11px] font-semibold uppercase tracking-wider text-steel-gray"
                                    >
                                        {heading}
                                    </th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-200">
                            {clients.map((client) => (
                                <tr key={client.invitationId}>
                                    <td className="whitespace-nowrap px-3 py-3 text-body-sm text-on-surface">
                                        {client.invitedEmail}
                                    </td>
                                    <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                                        {formatDate(client.invitedAtUtc)}
                                    </td>
                                    <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                                        {formatDate(client.expiresAtUtc)}
                                    </td>
                                    <td className="whitespace-nowrap px-3 py-3">
                                        <Badge tone={STATUS_TONE[client.status]}>{formatEnum(client.status)}</Badge>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            <InviteClientModal open={inviteOpen} onClose={() => setInviteOpen(false)} onInvited={refresh} />
        </div>
    );
}
