import { useState } from 'react';
import { cn } from '@/shared/lib/cn';
import { Card } from '@/shared/ui';
import { EventTimeline } from './components/EventTimeline';
import { SensorCard } from './components/SensorCard';
import { SimulatedMap } from './components/SimulatedMap';
import { isTemperatureBreached, MOCK_TRACKED_LOADS } from './mockTelemetry';

const etaFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

const SERVICE_LABEL = { ColdChain: 'Cold Chain', HeavyHaul: 'Heavy Haul' } as const;

export function LiveTrackingPage(): JSX.Element {
  const [selectedId, setSelectedId] = useState<string | undefined>(MOCK_TRACKED_LOADS[0]?.id);
  const load =
    MOCK_TRACKED_LOADS.find((entry) => entry.id === selectedId) ?? MOCK_TRACKED_LOADS[0];

  if (!load) {
    return (
      <div className="mx-auto max-w-container px-4 py-10 font-mono text-body-sm text-steel-gray">
        No freight is currently in transit.
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-container flex-col gap-5 px-4 py-8">
      <header className="flex flex-col gap-2">
        <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
          Prototype · Simulated telemetry
        </span>
        <h1 className="text-headline-lg">Live Tracking &amp; Telemetry</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Real-time position, cold-chain sensor health and milestone timeline for freight currently
          in transit across the CPG Orlando network.
        </p>
      </header>

      {/* Active-load selector */}
      <div className="flex gap-2 overflow-x-auto pb-1">
        {MOCK_TRACKED_LOADS.map((entry) => {
          const alert = entry.temperature ? isTemperatureBreached(entry.temperature) : false;
          const active = entry.id === load.id;
          return (
            <button
              key={entry.id}
              type="button"
              onClick={() => setSelectedId(entry.id)}
              className={cn(
                'flex shrink-0 flex-col items-start gap-1 rounded border px-4 py-2 text-left transition-colors',
                active ? 'border-primary bg-primary text-white' : 'border-outline bg-surface-card hover:bg-surface-muted',
              )}
            >
              <span className="flex items-center gap-2 font-mono text-label-md">
                {entry.reference}
                {alert ? <span className="h-2 w-2 rounded-full bg-signal-red" aria-label="alert" /> : null}
              </span>
              <span className={cn('font-mono text-label-sm', active ? 'text-white/70' : 'text-steel-gray')}>
                {SERVICE_LABEL[entry.serviceType]} · {entry.progressPct}%
              </span>
            </button>
          );
        })}
      </div>

      {/* Split screen */}
      <div className="grid gap-5 lg:grid-cols-[1.6fr_1fr]">
        <div className="flex flex-col gap-3">
          <Card anchored className="overflow-hidden">
            <div className="aspect-[4/3] w-full sm:aspect-[16/10]">
              <SimulatedMap load={load} />
            </div>
          </Card>

          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <Metric label="Speed" value={`${load.speedMph} mph`} />
            <Metric label="Heading" value={`${load.headingLabel} ${load.headingDeg}°`} />
            <Metric label="Distance left" value={`${load.distanceRemainingMiles} mi`} />
            <Metric label="ETA" value={`${etaFormatter.format(new Date(load.etaUtc))}`} />
          </div>
        </div>

        <div className="flex flex-col gap-4">
          <Card anchored className="flex flex-col gap-2 p-5">
            <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
              {SERVICE_LABEL[load.serviceType]} · {load.reference}
            </span>
            <h2 className="text-headline-sm">
              {load.originLabel} &rarr; {load.destinationLabel}
            </h2>
            <dl className="mt-1 grid grid-cols-2 gap-x-4 gap-y-2 font-mono text-body-sm">
              <Detail term="Driver" value={load.driver} />
              <Detail term="Unit" value={load.tractorUnit} />
              <Detail
                term="Position"
                value={`${load.currentPosition.lat.toFixed(3)}, ${load.currentPosition.lng.toFixed(3)}`}
              />
              <Detail term="Last ping" value={`${etaFormatter.format(new Date(load.lastPingUtc))} UTC`} />
            </dl>
            <div className="mt-2 h-2 overflow-hidden rounded-full bg-surface-muted">
              <div className="h-full rounded-full bg-hazard-orange" style={{ width: `${load.progressPct}%` }} />
            </div>
          </Card>

          {load.temperature ? (
            <SensorCard telemetry={load.temperature} lastPingUtc={load.lastPingUtc} />
          ) : (
            <Card className="flex flex-col gap-1 p-5">
              <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
                Sensors
              </span>
              <p className="text-body-sm text-steel-gray">
                No environmental sensors on this load — heavy-haul units report position and load-securement
                checks only.
              </p>
            </Card>
          )}

          <Card className="flex flex-col gap-3 p-5">
            <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
              Event timeline
            </span>
            <EventTimeline events={load.timeline} />
          </Card>
        </div>
      </div>
    </div>
  );
}

function Metric({ label, value }: { label: string; value: string }): JSX.Element {
  return (
    <div className="rounded border border-outline bg-surface-card p-3">
      <div className="font-mono text-label-md font-semibold tabular-nums text-fleet-blue">{value}</div>
      <div className="font-mono text-label-sm uppercase text-steel-gray">{label}</div>
    </div>
  );
}

function Detail({ term, value }: { term: string; value: string }): JSX.Element {
  return (
    <div className="flex flex-col">
      <dt className="text-label-sm uppercase tracking-wide text-steel-gray">{term}</dt>
      <dd className="text-on-surface">{value}</dd>
    </div>
  );
}
