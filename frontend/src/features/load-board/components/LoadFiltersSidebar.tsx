import { cn } from '@/shared/lib/cn';
import { Card } from '@/shared/ui';
import type { LoadServiceType, LoadStatus } from '../types';
import { LOAD_SERVICE_TYPES, LOAD_STATUSES } from '../types';

export interface LoadFilters {
  statuses: Set<LoadStatus>;
  serviceTypes: Set<LoadServiceType>;
  originQuery: string;
  destinationQuery: string;
}

interface LoadFiltersSidebarProps {
  filters: LoadFilters;
  onChange: (next: LoadFilters) => void;
  resultCount: number;
}

function toggle<T>(set: Set<T>, value: T): Set<T> {
  const next = new Set(set);
  if (next.has(value)) {
    next.delete(value);
  } else {
    next.add(value);
  }
  return next;
}

export function LoadFiltersSidebar({ filters, onChange, resultCount }: LoadFiltersSidebarProps): JSX.Element {
  return (
    <Card className="flex h-fit flex-col gap-6 p-5 lg:sticky lg:top-20">
      <div className="flex items-center justify-between">
        <h2 className="text-xs font-semibold uppercase tracking-wider text-on-surface">Filters</h2>
        <span className="text-body-sm tabular-nums text-steel-gray">{resultCount} loads</span>
      </div>

      <fieldset className="flex flex-col gap-2">
        <legend className="mb-1 text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Status
        </legend>
        {LOAD_STATUSES.map((status) => (
          <label
            key={status}
            className="flex cursor-pointer items-center gap-2.5 text-body-sm text-on-surface-variant"
          >
            <input
              type="checkbox"
              className="h-4 w-4 rounded border-outline-strong text-fleet-blue accent-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
              checked={filters.statuses.has(status)}
              onChange={() => onChange({ ...filters, statuses: toggle(filters.statuses, status) })}
            />
            {status === 'InTransit' ? 'In Transit' : status}
          </label>
        ))}
      </fieldset>

      <fieldset className="flex flex-col gap-2">
        <legend className="mb-1 text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Equipment / service type
        </legend>
        {LOAD_SERVICE_TYPES.map((line) => (
          <label
            key={line.value}
            className="flex cursor-pointer items-center gap-2.5 text-body-sm text-on-surface-variant"
          >
            <input
              type="checkbox"
              className="h-4 w-4 rounded border-outline-strong text-fleet-blue accent-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
              checked={filters.serviceTypes.has(line.value)}
              onChange={() =>
                onChange({ ...filters, serviceTypes: toggle(filters.serviceTypes, line.value) })
              }
            />
            {line.label}
          </label>
        ))}
      </fieldset>

      <div className="flex flex-col gap-3">
        <div className="flex flex-col gap-1">
          <label
            htmlFor="origin-filter"
            className="text-xs font-semibold uppercase tracking-wider text-steel-gray"
          >
            Origin
          </label>
          <input
            id="origin-filter"
            type="text"
            placeholder="City, state or ZIP"
            value={filters.originQuery}
            onChange={(event) => onChange({ ...filters, originQuery: event.target.value })}
            className="h-10 rounded border border-outline-strong bg-surface-card px-3 text-body-sm outline-none transition-colors focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label
            htmlFor="destination-filter"
            className="text-xs font-semibold uppercase tracking-wider text-steel-gray"
          >
            Destination
          </label>
          <input
            id="destination-filter"
            type="text"
            placeholder="City, state or ZIP"
            value={filters.destinationQuery}
            onChange={(event) => onChange({ ...filters, destinationQuery: event.target.value })}
            className="h-10 rounded border border-outline-strong bg-surface-card px-3 text-body-sm outline-none transition-colors focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
          />
        </div>
      </div>

      <button
        type="button"
        onClick={() =>
          onChange({
            statuses: new Set(),
            serviceTypes: new Set(),
            originQuery: '',
            destinationQuery: '',
          })
        }
        className={cn(
          'text-xs font-semibold uppercase tracking-wider text-fleet-blue hover:underline',
          'self-start',
        )}
      >
        Clear all filters
      </button>
    </Card>
  );
}
