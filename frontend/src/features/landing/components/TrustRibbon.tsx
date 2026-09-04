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
        <div key={metric.value} className="rounded-lg border border-outline bg-surface-card p-4">
          <dt className="font-mono text-label-md font-semibold text-fleet-blue">{metric.value}</dt>
          <dd className="text-body-sm text-steel-gray">{metric.label}</dd>
        </div>
      ))}
    </dl>
  );
}
