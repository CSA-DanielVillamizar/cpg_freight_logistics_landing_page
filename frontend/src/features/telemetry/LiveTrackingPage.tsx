import { useCallback, useEffect, useState } from 'react';
import { loadsApi } from '@/features/load-board/api/loadsApi';
import { cn } from '@/shared/lib/cn';
import { formatEnum } from '@/shared/lib/formatEnum';
import { Card, EmptyState } from '@/shared/ui';
import { EventTimeline } from './components/EventTimeline';
import { SensorCard } from './components/SensorCard';
import { SimulatedMap } from './components/SimulatedMap';
import { buildTrackedLoad } from './mockTelemetry';
import type { GpsPoint, TelemetryReading, TrackedLoad } from './types';
import { isTemperatureBreached } from './types';
import { useTelemetrySignalR, type TelemetryConnectionState } from './useTelemetrySignalR';

const MAX_TRAIL = 40;

const etaFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

const CONNECTION_META: Record<TelemetryConnectionState, { label: string; dot: string }> = {
  connecting: { label: 'Connecting', dot: 'bg-safety-amber' },
  connected: { label: 'Live', dot: 'bg-success' },
  reconnecting: { label: 'Reconnecting', dot: 'bg-safety-amber animate-pulse' },
  disconnected: { label: 'Offline', dot: 'bg-signal-red' },
};

/** Merge a SignalR reading into a tracked load immutably. */
function applyReading(load: TrackedLoad, reading: TelemetryReading): TrackedLoad {
  const point: GpsPoint = {
    lat: reading.latitude,
    lng: reading.longitude,
    atUtc: reading.timestampUtc,
  };
  const coordinateHistory = [...load.coordinateHistory, point].slice(-MAX_TRAIL);

  let temperature = load.temperature;
  if (temperature && reading.temperatureCelsius !== null) {
    temperature = {
      ...temperature,
      currentCelsius: reading.temperatureCelsius,
      history: [
        ...temperature.history,
        { atUtc: reading.timestampUtc, celsius: reading.temperatureCelsius },
      ].slice(-MAX_TRAIL),
    };
  }

  return {
    ...load,
    coordinateHistory,
    currentPosition: point,
    speedMph: reading.speedMph,
    lastPingUtc: reading.timestampUtc,
    temperature,
  };
}

export function LiveTrackingPage(): JSX.Element {
  const [tracked, setTracked] = useState<TrackedLoad[]>([]);
  const [status, setStatus] = useState<'loading' | 'ready' | 'error'>('loading');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    loadsApi
      .list({ statuses: ['InTransit'] })
      .then((loads) => {
        if (cancelled) {
          return;
        }
        const models = loads.map(buildTrackedLoad);
        setTracked(models);
        setSelectedId((previous) => previous ?? models[0]?.id ?? null);
        setStatus('ready');
      })
      .catch(() => {
        if (!cancelled) {
          setStatus('error');
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const onReading = useCallback((reading: TelemetryReading) => {
    setTracked((previous) =>
      previous.map((load) => (load.id === reading.loadId ? applyReading(load, reading) : load)),
    );
  }, []);

  const connectionState = useTelemetrySignalR({
    enabled: status === 'ready' && tracked.length > 0,
    onReading,
  });

  const selected = tracked.find((load) => load.id === selectedId) ?? tracked[0] ?? null;
  const connection = CONNECTION_META[connectionState];

  return (
    <div className="mx-auto flex max-w-container flex-col gap-5 px-4 py-8">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex flex-col gap-2">
          <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
            Real-Time Fleet Tracking
          </span>
          <h1 className="text-headline-lg">Live Tracking &amp; Telemetry</h1>
          <p className="max-w-2xl text-body-sm text-steel-gray">
            Position, cold-chain sensor health and milestone timeline for freight currently
            <span> in transit</span>. Updates stream in every few seconds — no reload.
          </p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-full border border-slate-200 bg-surface-card px-3 py-1.5 text-xs font-semibold uppercase tracking-wider text-steel-gray">
          <span className={cn('h-2 w-2 rounded-full', connection.dot)} />
          {connection.label}
        </span>
      </header>

      {status === 'error' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          Unable to load the tracking board — check that you are signed in.
        </Card>
      ) : status === 'loading' ? (
        <EmptyState icon="progress_activity" title="Loading fleet…" />
      ) : !selected ? (
        <EmptyState
          icon="satellite_alt"
          title="No freight is currently in transit"
          hint="Live position and sensor telemetry appear here once a carrier marks a load in transit."
        />
      ) : (
        <>
          <div className="flex gap-2 overflow-x-auto pb-1">
            {tracked.map((entry) => {
              const alert = entry.temperature ? isTemperatureBreached(entry.temperature) : false;
              const active = entry.id === selected.id;
              return (
                <button
                  key={entry.id}
                  type="button"
                  onClick={() => setSelectedId(entry.id)}
                  className={cn(
                    'flex shrink-0 flex-col items-start gap-1 rounded-lg border px-4 py-2 text-left transition-colors',
                    active
                      ? 'border-fleet-blue bg-fleet-blue text-white'
                      : 'border-slate-200 bg-surface-card hover:bg-surface-muted',
                  )}
                >
                  <span className="flex items-center gap-2 font-mono text-sm font-semibold">
                    {entry.reference}
                    {alert ? <span className="h-2 w-2 rounded-full bg-signal-red" aria-label="alert" /> : null}
                  </span>
                  <span className={cn('text-xs', active ? 'text-white/70' : 'text-steel-gray')}>
                    {formatEnum(entry.serviceType)} ·{' '}
                    <span className="font-mono tabular-nums">{entry.speedMph} mph</span>
                  </span>
                </button>
              );
            })}
          </div>

          <div className="grid gap-5 lg:grid-cols-[1.6fr_1fr]">
            <div className="flex flex-col gap-3">
              <Card raised className="overflow-hidden">
                <div className="aspect-[4/3] w-full sm:aspect-[16/10]">
                  <SimulatedMap load={selected} />
                </div>
              </Card>

              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <Metric label="Speed" value={`${selected.speedMph} mph`} />
                <Metric label="Heading" value={`${selected.headingLabel} ${selected.headingDeg}°`} />
                <Metric label="Distance left" value={`${selected.distanceRemainingMiles} mi`} />
                <Metric label="ETA" value={etaFormatter.format(new Date(selected.etaUtc))} />
              </div>
            </div>

            <div className="flex flex-col gap-4">
              <Card className="flex flex-col gap-2 p-5">
                <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                  {formatEnum(selected.serviceType)} ·{' '}
                  <span className="font-mono normal-case tracking-normal">{selected.reference}</span>
                </span>
                <h2 className="text-headline-sm">
                  {selected.originLabel} &rarr; {selected.destinationLabel}
                </h2>
                <dl className="mt-1 grid grid-cols-2 gap-x-4 gap-y-2 text-body-sm">
                  <Detail term="Driver" value={selected.driver} />
                  <Detail term="Unit" value={selected.tractorUnit} mono />
                  <Detail
                    term="Position"
                    value={`${selected.currentPosition.lat.toFixed(3)}, ${selected.currentPosition.lng.toFixed(3)}`}
                    mono
                  />
                  <Detail
                    term="Last ping"
                    value={`${etaFormatter.format(new Date(selected.lastPingUtc))} UTC`}
                    mono
                  />
                </dl>
              </Card>

              {selected.temperature ? (
                <SensorCard telemetry={selected.temperature} lastPingUtc={selected.lastPingUtc} />
              ) : (
                <Card className="flex flex-col gap-1 p-5">
                  <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    Sensors
                  </span>
                  <p className="text-body-sm text-steel-gray">
                    No environmental sensors on this load — heavy-haul units report position and
                    load-securement checks only.
                  </p>
                </Card>
              )}

              <Card className="flex flex-col gap-3 p-5">
                <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                  Event timeline
                </span>
                <EventTimeline events={selected.timeline} />
              </Card>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function Metric({ label, value }: { label: string; value: string }): JSX.Element {
  return (
    <div className="rounded-lg border border-slate-200 bg-surface-card p-3 shadow-sm">
      <div className="font-mono text-sm font-semibold tabular-nums text-fleet-blue">{value}</div>
      <div className="text-[11px] font-semibold uppercase tracking-wider text-steel-gray">{label}</div>
    </div>
  );
}

function Detail({ term, value, mono = false }: { term: string; value: string; mono?: boolean }): JSX.Element {
  return (
    <div className="flex flex-col">
      <dt className="text-[11px] font-semibold uppercase tracking-wider text-steel-gray">{term}</dt>
      <dd className={cn('text-on-surface', mono && 'font-mono tabular-nums')}>{value}</dd>
    </div>
  );
}
