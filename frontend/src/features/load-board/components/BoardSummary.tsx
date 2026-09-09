import type { Load } from '../types';

interface BoardSummaryProps {
  loads: readonly Load[];
}

const currency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});
const perMileCurrency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/** Derived operator HUD — live totals computed from the loads currently on the board. */
export function BoardSummary({ loads }: BoardSummaryProps): JSX.Element | null {
  if (loads.length === 0) {
    return null;
  }

  const available = loads.filter((load) => load.status === 'Available');
  const openGross = available.reduce((sum, load) => sum + load.rateUsd, 0);
  const milesTotal = loads.reduce((sum, load) => sum + load.distanceMiles, 0);
  const rateTotal = loads.reduce((sum, load) => sum + load.rateUsd, 0);
  const avgPerMile = milesTotal > 0 ? rateTotal / milesTotal : 0;
  const avgHaul = Math.round(milesTotal / loads.length);

  const tiles: readonly { label: string; value: string }[] = [
    { label: 'On the board', value: loads.length.toLocaleString() },
    { label: 'Available now', value: available.length.toLocaleString() },
    { label: 'Open gross', value: currency.format(openGross) },
    { label: 'Avg rate / mi', value: perMileCurrency.format(avgPerMile) },
    { label: 'Avg length of haul', value: `${avgHaul.toLocaleString()} mi` },
  ];

  return (
    <dl className="grid grid-cols-2 gap-px overflow-hidden rounded-lg border border-slate-200 bg-slate-200 sm:grid-cols-3 lg:grid-cols-5">
      {tiles.map((tile) => (
        <div key={tile.label} className="flex flex-col-reverse gap-1 bg-surface-card p-4">
          <dt className="text-[11px] font-semibold uppercase tracking-wider text-steel-gray">
            {tile.label}
          </dt>
          <dd className="font-mono text-headline-sm tabular-nums text-fleet-blue">{tile.value}</dd>
        </div>
      ))}
    </dl>
  );
}
