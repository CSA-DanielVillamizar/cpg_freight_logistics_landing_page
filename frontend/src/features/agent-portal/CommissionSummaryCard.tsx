import { Card } from '@/shared/ui';

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

interface CommissionSummaryCardProps {
    label: string;
    valueUsd: number;
    hint?: string;
    accent?: 'primary' | 'muted';
}

/** A single KPI tile on the Agent dashboard (T-SDD Epica 4). */
export function CommissionSummaryCard({
    label,
    valueUsd,
    hint,
    accent = 'muted',
}: CommissionSummaryCardProps): JSX.Element {
    return (
        <Card raised={accent === 'primary'} className="flex flex-col gap-1 p-5">
            <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">{label}</span>
            <span
                className={
                    accent === 'primary'
                        ? 'font-heading text-display-lg leading-none tabular-nums text-primary'
                        : 'font-heading text-headline-lg tabular-nums text-fleet-blue'
                }
            >
                {currency.format(valueUsd)}
            </span>
            {hint ? <span className="mt-1 text-xs text-steel-gray">{hint}</span> : null}
        </Card>
    );
}
