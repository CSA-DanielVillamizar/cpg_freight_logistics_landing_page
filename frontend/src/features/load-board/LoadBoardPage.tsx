import { useState } from 'react';
import { Card, EmptyState } from '@/shared/ui';
import { LoadDataGrid } from './components/LoadDataGrid';
import { LoadDetailsDrawer } from './components/LoadDetailsDrawer';
import type { LoadFilters } from './components/LoadFiltersSidebar';
import { LoadFiltersSidebar } from './components/LoadFiltersSidebar';
import { useLoads } from './useLoads';

const EMPTY_FILTERS: LoadFilters = {
  statuses: new Set(),
  serviceTypes: new Set(),
  originQuery: '',
  destinationQuery: '',
};

export function LoadBoardPage(): JSX.Element {
  const [filters, setFilters] = useState<LoadFilters>(EMPTY_FILTERS);
  const [selectedLoadId, setSelectedLoadId] = useState<string | null>(null);

  const { loads, status, errorMessage, refetch } = useLoads(filters);
  const selectedLoad = loads.find((load) => load.id === selectedLoadId) ?? null;

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-10">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Live · PostgreSQL-backed
        </span>
        <h1 className="text-headline-lg">Carrier &amp; Shipper Load Workspace</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Live board of freight moving through the CPG Orlando network. Filter by status, equipment
          and lane, then open a load to review the full spec and accept it.
        </p>
      </header>

      <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
        <LoadFiltersSidebar filters={filters} onChange={setFilters} resultCount={loads.length} />

        {status === 'error' ? (
          <Card className="border-error bg-error-container p-4 text-body-sm text-error">
            {errorMessage}
          </Card>
        ) : status === 'loading' ? (
          <EmptyState icon="progress_activity" title="Loading board…" />
        ) : (
          <LoadDataGrid loads={loads} onSelect={(load) => setSelectedLoadId(load.id)} />
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
