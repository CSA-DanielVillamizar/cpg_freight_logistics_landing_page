import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'sonner';
import { ServiceTypeBadge } from '@/features/load-board/components/ServiceTypeBadge';
import { STATUS_TONE } from '@/features/load-board/types';
import { ApiError } from '@/shared/api/client';
import { cn } from '@/shared/lib/cn';
import { Badge, Button, Card } from '@/shared/ui';
import { ShipperNav } from './ShipperNav';
import { shipperApi } from './shipperApi';
import type { ShipperLoadView, ShipperLoadsResponse } from './shipperApi';

type PageStatus = 'loading' | 'ready' | 'forbidden' | 'error';

const currency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});
const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', year: 'numeric' });

const formatDate = (iso: string): string => dateFormatter.format(new Date(iso));
const statusLabel = (status: ShipperLoadView['status']): string =>
  status === 'InTransit' ? 'In Transit' : status;

export function ShipperDashboardPage(): JSX.Element {
  const [status, setStatus] = useState<PageStatus>('loading');
  const [data, setData] = useState<ShipperLoadsResponse | null>(null);
  const [tab, setTab] = useState<'active' | 'history'>('active');

  useEffect(() => {
    const controller = new AbortController();
    shipperApi
      .getLoads()
      .then((response) => {
        if (controller.signal.aborted) {
          return;
        }
        setData(response);
        setStatus('ready');
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return;
        }
        setStatus(error instanceof ApiError && error.status === 403 ? 'forbidden' : 'error');
      });
    return () => controller.abort();
  }, []);

  if (status === 'forbidden') {
    return (
      <div className="mx-auto max-w-container px-4 py-10">
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">Access denied</Card>
      </div>
    );
  }

  if (status === 'error') {
    return (
      <div className="mx-auto max-w-container px-4 py-10">
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          Unable to load your shipments right now.
        </Card>
      </div>
    );
  }

  if (status === 'loading' || !data) {
    return (
      <div className="mx-auto max-w-container px-4 py-10">
        <Card className="flex h-40 items-center justify-center p-6 font-mono text-body-sm text-steel-gray">
          Loading your shipments…
        </Card>
      </div>
    );
  }

  const rows = tab === 'active' ? data.active : data.history;

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-8">
      <header className="flex flex-col gap-2">
        <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
          Shipper portal
        </span>
        <h1 className="text-headline-lg">Your Shipments</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Track the freight you have moving through the CPG Orlando network and pull proof of
          delivery for completed loads.
        </p>
      </header>

      <ShipperNav />

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <Metric label="Active shipments" value={String(data.metrics.activeCount)} />
        <Metric label="In transit" value={String(data.metrics.inTransitCount)} />
        <Metric label="Delivered" value={String(data.metrics.deliveredCount)} />
        <Metric label="Active spend" value={currency.format(data.metrics.activeSpendUsd)} />
      </div>

      <div className="flex gap-2">
        {(['active', 'history'] as const).map((value) => (
          <button
            key={value}
            type="button"
            onClick={() => setTab(value)}
            aria-pressed={tab === value}
            className={cn(
              'rounded border px-3 py-1.5 font-mono text-label-sm uppercase tracking-wide transition-colors',
              tab === value
                ? 'border-primary bg-primary text-white'
                : 'border-outline bg-surface-card text-steel-gray hover:bg-surface-muted',
            )}
          >
            {value === 'active' ? `Active shipments (${data.active.length})` : `History (${data.history.length})`}
          </button>
        ))}
      </div>

      {rows.length === 0 ? (
        <Card className="flex h-40 items-center justify-center p-6 font-mono text-body-sm text-steel-gray">
          {tab === 'active' ? 'No active shipments right now.' : 'No delivered shipments yet.'}
        </Card>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-outline bg-surface-card">
          <table className="w-full min-w-[820px] text-left">
            <thead>
              <tr className="border-b border-outline bg-surface-muted">
                {['Load', 'Service', 'Lane', 'Carrier', tab === 'active' ? 'Window' : 'Delivered', 'Status', ''].map(
                  (heading) => (
                    <th
                      key={heading}
                      className="whitespace-nowrap px-3 py-2 font-mono text-label-sm uppercase tracking-wider text-steel-gray"
                    >
                      {heading}
                    </th>
                  ),
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-outline">
              {rows.map((load) => (
                <tr key={load.id}>
                  <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm font-semibold text-fleet-blue">
                    {load.reference}
                  </td>
                  <td className="whitespace-nowrap px-3 py-3">
                    <ServiceTypeBadge serviceType={load.serviceType} />
                  </td>
                  <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm text-on-surface-variant">
                    {load.originCity}, {load.originState} &rarr; {load.destinationCity}, {load.destinationState}
                  </td>
                  <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm text-steel-gray">
                    {load.carrierName ?? '—'}
                  </td>
                  <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                    {tab === 'active'
                      ? `${formatDate(load.pickupAtUtc)} → ${formatDate(load.deliveryAtUtc)}`
                      : formatDate(load.deliveryAtUtc)}
                  </td>
                  <td className="whitespace-nowrap px-3 py-3">
                    <Badge tone={STATUS_TONE[load.status]}>{statusLabel(load.status)}</Badge>
                  </td>
                  <td className="whitespace-nowrap px-3 py-3 text-right">
                    {tab === 'active' ? (
                      <Link
                        to="/tracking"
                        className="font-mono text-label-sm uppercase tracking-wide text-hazard-orange hover:underline"
                      >
                        Track live
                      </Link>
                    ) : (
                      <PodButton load={load} />
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function PodButton({ load }: { load: ShipperLoadView }): JSX.Element {
  const [busy, setBusy] = useState(false);

  async function download(): Promise<void> {
    setBusy(true);
    try {
      await shipperApi.openPod(load.id);
    } catch {
      toast.error('Proof of delivery is not available for this load.');
    } finally {
      setBusy(false);
    }
  }

  if (!load.podAvailable) {
    return <span className="font-mono text-label-sm uppercase tracking-wide text-steel-gray">POD pending</span>;
  }

  return (
    <Button variant="outline" onClick={() => void download()} disabled={busy}>
      {busy ? 'Opening…' : 'Download POD'}
    </Button>
  );
}

function Metric({ label, value }: { label: string; value: string }): JSX.Element {
  return (
    <Card anchored className="p-3">
      <div className="font-heading text-headline-sm tabular-nums text-fleet-blue">{value}</div>
      <div className="font-mono text-label-sm uppercase text-steel-gray">{label}</div>
    </Card>
  );
}
