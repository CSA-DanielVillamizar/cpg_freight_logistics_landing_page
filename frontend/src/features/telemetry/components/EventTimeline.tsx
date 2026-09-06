import { cn } from '@/shared/lib/cn';
import type { TimelineEvent, TimelineKind } from '../types';

interface EventTimelineProps {
  events: readonly TimelineEvent[];
}

const dateTimeFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

const KIND_DOT: Record<TimelineKind, string> = {
  dispatched: 'bg-fleet-blue',
  loaded: 'bg-fleet-blue',
  checkpoint: 'bg-hazard-orange',
  delay: 'bg-signal-red',
  arrival: 'bg-steel-gray',
};

export function EventTimeline({ events }: EventTimelineProps): JSX.Element {
  return (
    <ol className="flex flex-col">
      {events.map((event, index) => {
        const isLast = index === events.length - 1;
        return (
          <li key={`${event.label}-${event.atUtc}`} className="flex gap-3">
            <div className="flex flex-col items-center">
              <span
                className={cn(
                  'mt-1 h-3 w-3 shrink-0 rounded-full',
                  event.complete ? KIND_DOT[event.kind] : 'border-2 border-outline-strong bg-surface-card',
                )}
              />
              {!isLast ? (
                <span
                  className={cn('w-px flex-1', event.complete ? 'bg-outline-strong' : 'bg-outline')}
                />
              ) : null}
            </div>
            <div className={cn('flex flex-col gap-0.5 pb-6', isLast && 'pb-0')}>
              <span
                className={cn(
                  'text-xs font-semibold uppercase tracking-wider',
                  event.kind === 'delay' ? 'text-signal-red' : 'text-on-surface',
                  !event.complete && 'text-steel-gray',
                )}
              >
                {event.label}
              </span>
              <span className="font-mono text-[11px] tabular-nums text-steel-gray">
                {dateTimeFormatter.format(new Date(event.atUtc))} UTC
                {!event.complete ? ' · projected' : ''}
              </span>
              <span className="text-body-sm text-on-surface-variant">{event.detail}</span>
            </div>
          </li>
        );
      })}
    </ol>
  );
}
