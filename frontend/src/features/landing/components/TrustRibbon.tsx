interface TrustMetric {
  value: string;
  label: string;
}

const METRICS: readonly TrustMetric[] = [
  { value: 'DBE', label: 'Certified for federal bids' },
  { value: '35+ yrs', label: 'Continuous operations' },
  { value: '11 units', label: 'Lowboys, step-decks, RGN' },
  { value: '48 states', label: 'Corridor permits pre-cleared' },
];

export function TrustRibbon(): JSX.Element {
  return (
    <dl className="grid grid-cols-2 gap-2 sm:grid-cols-4">
      {METRICS.map((metric) => (
        <div
          key={metric.value}
          className="rounded-lg border border-slate-200 bg-surface-card p-4 shadow-sm"
        >
          <dt className="font-mono text-sm font-semibold tabular-nums text-fleet-blue">
            {metric.value}
          </dt>
          <dd className="text-body-sm text-steel-gray">{metric.label}</dd>
        </div>
      ))}
    </dl>
  );
}
