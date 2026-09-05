import { useMemo, useState } from 'react';
import { LoadDataGrid } from './components/LoadDataGrid';
import { LoadDetailsDrawer } from './components/LoadDetailsDrawer';
import type { LoadFilters } from './components/LoadFiltersSidebar';
import { LoadFiltersSidebar } from './components/LoadFiltersSidebar';
import type { Load } from './mockLoads';
import { MOCK_LOADS } from './mockLoads';

const EMPTY_FILTERS: LoadFilters = {
  statuses: new Set(),
  serviceTypes: new Set(),
  originQuery: '',
  destinationQuery: '',
};

function matchesStop(stop: Load['origin'], query: string): boolean {
  if (!query.trim()) {
    return true;
  }
  const needle = query.trim().toLowerCase();
  return (
    stop.city.toLowerCase().includes(needle) ||
    stop.state.toLowerCase().includes(needle) ||
    stop.zip.includes(needle)
  );
}

export function LoadBoardPage(): JSX.Element {
  const [filters, setFilters] = useState<LoadFilters>(EMPTY_FILTERS);
  const [selectedLoad, setSelectedLoad] = useState<Load | null>(null);

  const filteredLoads = useMemo(() => {
    return MOCK_LOADS.filter((load) => {
      if (filters.statuses.size > 0 && !filters.statuses.has(load.status)) {
        return false;
      }
      if (filters.serviceTypes.size > 0 && !filters.serviceTypes.has(load.serviceType)) {
        return false;
      }
      if (!matchesStop(load.origin, filters.originQuery)) {
        return false;
      }
      if (!matchesStop(load.destination, filters.destinationQuery)) {
        return false;
      }
      return true;
    });
  }, [filters]);

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-10">
      <header className="flex flex-col gap-2">
        <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
          Prototype · Dummy data only
        </span>
        <h1 className="text-headline-lg">Carrier &amp; Shipper Load Workspace</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Live board of freight moving through the CPG Orlando network. Filter by status, equipment
          and lane, then open a load to review the full spec and bid or accept.
        </p>
      </header>

      <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
        <LoadFiltersSidebar filters={filters} onChange={setFilters} resultCount={filteredLoads.length} />
        <LoadDataGrid loads={filteredLoads} onSelect={setSelectedLoad} />
      </div>

      <LoadDetailsDrawer load={selectedLoad} onClose={() => setSelectedLoad(null)} />
    </div>
  );
}
