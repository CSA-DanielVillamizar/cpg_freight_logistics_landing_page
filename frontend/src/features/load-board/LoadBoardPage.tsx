import { useMemo, useState } from 'react';
import { cn } from '@/shared/lib/cn';
import { Card, EmptyState } from '@/shared/ui';
import { BoardSummary } from './components/BoardSummary';
import { LoadDataGrid } from './components/LoadDataGrid';
import { LoadDetailsDrawer } from './components/LoadDetailsDrawer';
import type { LoadFilters } from './components/LoadFiltersSidebar';
import { LoadFiltersSidebar } from './components/LoadFiltersSidebar';
import type { Load } from './types';
import { useLoads } from './useLoads';

const EMPTY_FILTERS: LoadFilters = {
  statuses: new Set(),
  serviceTypes: new Set(),
  originQuery: '',
  destinationQuery: '',
};

type SortKey = 'rate' | 'perMile' | 'pickup';

const SORT_OPTIONS: readonly { key: SortKey; label: string }[] = [
  { key: 'rate', label: 'Rate' },
  { key: 'perMile', label: '$ / mi' },
  { key: 'pickup', label: 'Pickup' },
];

function perMile(load: Load): number {
  return load.distanceMiles > 0 ? load.rateUsd / load.distanceMiles : 0;
}

function sortLoads(loads: readonly Load[], key: SortKey): Load[] {
  const next = [...loads];
  switch (key) {
    case 'rate':
      return next.sort((a, b) => b.rateUsd - a.rateUsd);
    case 'perMile':
      return next.sort((a, b) => perMile(b) - perMile(a));
    case 'pickup':
      return next.sort(
        (a, b) => new Date(a.pickupAtUtc).getTime() - new Date(b.pickupAtUtc).getTime(),
      );
  }
}

export function LoadBoardPage(): JSX.Element {
  const [filters, setFilters] = useState<LoadFilters>(EMPTY_FILTERS);
  const [selectedLoadId, setSelectedLoadId] = useState<string | null>(null);
  const [sort, setSort] = useState<SortKey>('rate');

  const { loads, status, errorMessage, refetch } = useLoads(filters);
  const sortedLoads = useMemo(() => sortLoads(loads, sort), [loads, sort]);
  const selectedLoad = loads.find((load) => load.id === selectedLoadId) ?? null;
  const showToolbar = status === 'ready' && loads.length > 0;

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-10">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Logistics Operations
        </span>
        <h1 className="text-headline-lg">Carrier &amp; Shipper Load Workspace</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Live board of freight moving through the CPG Orlando network. Filter by status, equipment
          and lane, then open a load to review the full spec and accept it.
        </p>
      </header>

      {status === 'ready' ? <BoardSummary loads={loads} /> : null}

      <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
        <LoadFiltersSidebar filters={filters} onChange={setFilters} resultCount={loads.length} />

        {status === 'error' ? (
          <Card className="border-error bg-error-container p-4 text-body-sm text-error">
            {errorMessage}
          </Card>
        ) : status === 'loading' ? (
          <EmptyState icon="progress_activity" title="Loading board…" />
        ) : (
          <div className="flex flex-col gap-3">
            {showToolbar ? (
              <div className="flex items-center justify-between gap-3">
                <span className="text-[11px] font-semibold uppercase tracking-wider text-steel-gray">
                  Sorted by
                </span>
                <div className="flex items-center gap-0.5 rounded-lg border border-slate-200 bg-surface-card p-0.5">
                  {SORT_OPTIONS.map((option) => (
                    <button
                      key={option.key}
                      type="button"
                      onClick={() => setSort(option.key)}
                      aria-pressed={sort === option.key}
                      className={cn(
                        'rounded-md px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wider transition-colors',
                        sort === option.key
                          ? 'bg-fleet-blue text-white'
                          : 'text-steel-gray hover:text-on-surface',
                      )}
                    >
                      {option.label}
                    </button>
                  ))}
                </div>
              </div>
            ) : null}
            <LoadDataGrid loads={sortedLoads} onSelect={(load) => setSelectedLoadId(load.id)} />
          </div>
        )}
      </div>

      <LoadDetailsDrawer
        load={selectedLoad}
        onClose={() => setSelectedLoadId(null)}
        onAccepted={refetch}
      />
    </div>
  );
}
