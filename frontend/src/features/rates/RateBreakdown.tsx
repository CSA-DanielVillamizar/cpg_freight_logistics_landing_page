import type { RateCalculationResponse } from '@/shared/api/types';
import { cn } from '@/shared/lib/cn';
import { Card } from '@/shared/ui';

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

interface RateBreakdownProps {
  result: RateCalculationResponse;
}

/** Detailed quote breakdown: base rate, cold-chain surcharge, fuel surcharge, total (SPEC.md US-02). */
export function RateBreakdown({ result }: RateBreakdownProps): JSX.Element {
  const rows: { label: string; value: number; muted?: boolean }[] = [
    { label: 'Base linehaul rate', value: result.baseRate },
    { label: 'Cold chain surcharge', value: result.coldChainSurcharge, muted: result.coldChainSurcharge === 0 },
    { label: 'Fuel surcharge', value: result.fuelSurcharge },
  ];

  return (
    <Card className="flex flex-col gap-4 border-transparent bg-primary-container p-6 text-white shadow-md">
      <div className="flex items-center justify-between">
        <span className="text-xs font-semibold uppercase tracking-wider text-white/70">
          Lane estimate confirmed
        </span>
        <span className="font-mono text-[11px] tabular-nums text-white/70">
          {new Date(result.calculatedAt).toLocaleString()}
        </span>
      </div>

      <dl className="flex flex-col gap-2 text-body-sm">
        {rows.map((row) => (
          <div
            key={row.label}
            className="flex items-center justify-between border-b border-white/10 pb-2"
          >
            <dt className={row.muted ? 'text-white/40' : 'text-white/70'}>{row.label}</dt>
            <dd
              className={cn(
                'font-mono tabular-nums',
                row.muted ? 'text-white/40' : 'text-white',
              )}
            >
              {currency.format(row.value)}
            </dd>
          </div>
        ))}
      </dl>

      <div className="flex items-baseline justify-between">
        <span className="font-heading text-headline-sm text-white">Total estimated</span>
        <span className="font-mono text-headline-lg font-semibold tabular-nums text-white">
          {currency.format(result.totalEstimated)}
        </span>
      </div>

      <p className="text-xs text-white/60">
        {result.currency} · all-inclusive (fuel &amp; specialized surcharges)
      </p>
    </Card>
  );
}
