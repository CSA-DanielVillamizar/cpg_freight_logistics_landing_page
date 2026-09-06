import { cn } from '@/shared/lib/cn';
import type { ColdChainTelemetry } from '../types';
import { isTemperatureBreached } from '../types';
import { Sparkline } from './Sparkline';

interface SensorCardProps {
  telemetry: ColdChainTelemetry;
  lastPingUtc: string;
}

const timeFormatter = new Intl.DateTimeFormat('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });

export function SensorCard({ telemetry, lastPingUtc }: SensorCardProps): JSX.Element {
  const breached = isTemperatureBreached(telemetry);

  return (
    <div
      className={cn(
        'flex flex-col gap-4 rounded-lg border bg-surface-card p-5 shadow-sm',
        breached ? 'border-2 border-signal-red' : 'border-slate-200',
      )}
    >
      <div className="flex items-start justify-between">
        <div className="flex flex-col">
          <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
            Reefer temperature
          </span>
          <span className="text-body-sm text-steel-gray">
            Setpoint <span className="font-mono tabular-nums">{telemetry.setpointCelsius.toFixed(1)}°C</span> · band{' '}
            <span className="font-mono tabular-nums">
              {telemetry.minCelsius}°C to {telemetry.maxCelsius}°C
            </span>
          </span>
        </div>
        {breached ? (
          <span className="inline-flex animate-pulse items-center gap-1 rounded-full bg-signal-red px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wider text-white">
            ● Excursion
          </span>
        ) : (
          <span className="inline-flex items-center gap-1 rounded-full bg-success-container px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wider text-success">
            ● In band
          </span>
        )}
      </div>

      <div className="flex items-end gap-2">
        <span
          className={cn(
            'font-mono text-[3.25rem] font-semibold leading-none tabular-nums',
            breached ? 'text-signal-red' : 'text-primary',
          )}
        >
          {telemetry.currentCelsius.toFixed(1)}
        </span>
        <span className="pb-1 font-mono text-headline-sm text-steel-gray">°C</span>
      </div>

      <Sparkline telemetry={telemetry} />

      <span className="text-body-sm text-steel-gray">
        Last telemetry{' '}
        <span className="font-mono tabular-nums">
          {timeFormatter.format(new Date(lastPingUtc))} UTC
        </span>{' '}
        · 15 min interval
      </span>
    </div>
  );
}
