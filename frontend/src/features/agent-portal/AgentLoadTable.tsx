import { formatEnum } from '@/shared/lib/formatEnum';
import type { BadgeTone } from '@/shared/ui';
import { Badge, EmptyState } from '@/shared/ui';
import type { AgentLoadView } from './agentApi';

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit' });

const STATUS_TONE: Record<AgentLoadView['status'], BadgeTone> = {
    Available: 'available',
    Dispatched: 'dispatched',
    InTransit: 'transit',
    Delivered: 'delivered',
};

interface AgentLoadTableProps {
    loads: AgentLoadView[];
}

/** The Agent's recent loads, with projected commission per row (T-SDD Epica 4). */
export function AgentLoadTable({ loads }: AgentLoadTableProps): JSX.Element {
    if (loads.length === 0) {
        return (
            <EmptyState
                icon="local_shipping"
                title="No loads published yet"
                hint="Loads you publish for your clients show up here with their projected commission."
            />
        );
    }

    return (
        <div className="overflow-x-auto rounded-lg border border-slate-200 bg-surface-card shadow-sm">
            <table className="w-full min-w-[760px] text-left">
                <thead>
                    <tr className="border-b border-slate-200 bg-surface-muted">
                        {['Load', 'Lane', 'Pickup', 'Rate', 'Commission', 'Status'].map((heading) => (
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
                    {loads.map((load) => (
                        <tr key={load.loadId}>
                            <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm font-semibold text-fleet-blue">
                                {load.reference}
                            </td>
                            <td className="whitespace-nowrap px-3 py-3 text-body-sm text-steel-gray">
                                {load.originCity}, {load.originState} &rarr; {load.destinationCity}, {load.destinationState}
                            </td>
                            <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                                {dateFormatter.format(new Date(load.pickupAtUtc))}
                            </td>
                            <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-steel-gray">
                                {currency.format(load.rateUsd)}
                            </td>
                            <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm font-semibold tabular-nums text-primary">
                                {load.projectedAgentCommissionUsd !== null ? currency.format(load.projectedAgentCommissionUsd) : '—'}
                            </td>
                            <td className="whitespace-nowrap px-3 py-3">
                                <Badge tone={STATUS_TONE[load.status]}>{formatEnum(load.status)}</Badge>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
