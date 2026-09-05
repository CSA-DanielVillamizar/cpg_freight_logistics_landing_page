import { useState } from 'react';
import { toast } from 'sonner';
import { ApiError } from '@/shared/api/client';
import { cn } from '@/shared/lib/cn';
import { Badge, Button } from '@/shared/ui';
import { loadsApi } from '../api/loadsApi';
import type { Load } from '../types';
import { statusLabel, STATUS_TONE } from '../types';
import { ServiceTypeBadge } from './ServiceTypeBadge';

interface LoadDetailsDrawerProps {
  load: Load | null;
  onClose: () => void;
  onAccepted: () => void;
}

const currency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});
const dateTimeFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: '2-digit',
  hour: 'numeric',
  minute: '2-digit',
});

/** Purely decorative route sketch — no real geocoding backs this. */
function StaticRouteMap({ load }: { load: Load }): JSX.Element {
  return (
    <svg viewBox="0 0 400 160" className="h-40 w-full rounded-lg border border-outline bg-[#0E1C2F]">
      <defs>
        <pattern id="load-map-grid" width="20" height="20" patternUnits="userSpaceOnUse">
          <path d="M 20 0 L 0 0 0 20" fill="none" stroke="rgba(255,255,255,0.06)" strokeWidth="1" />
        </pattern>
      </defs>
      <rect width="400" height="160" fill="url(#load-map-grid)" />
      <path
        d="M 48 118 C 140 60, 240 132, 352 46"
        fill="none"
        stroke="#EA580C"
        strokeWidth="2.5"
        strokeDasharray="7 6"
        strokeLinecap="round"
      />
      <circle cx="48" cy="118" r="6" fill="#1C3766" stroke="white" strokeWidth="2" />
      <circle cx="352" cy="46" r="6" fill="#EA580C" stroke="white" strokeWidth="2" />
      <text x="48" y="138" textAnchor="middle" fontSize="9" fill="#CBD5E1">
        {load.originState}
      </text>
      <text x="352" y="34" textAnchor="middle" fontSize="9" fill="#CBD5E1">
        {load.destinationState}
      </text>
    </svg>
  );
}

export function LoadDetailsDrawer({ load, onClose, onAccepted }: LoadDetailsDrawerProps): JSX.Element {
  const [accepting, setAccepting] = useState(false);
  const isOpen = load !== null;

  function handleBid(): void {
    toast.info('Bidding is not available yet — call dispatch to negotiate this lane.');
  }

  async function handleAccept(): Promise<void> {
    if (!load) {
      return;
    }
    setAccepting(true);
    try {
      const updated = await loadsApi.accept(load.id);
      toast.success(`Load ${updated.reference} accepted — status is now ${statusLabel(updated.status)}.`);
      onAccepted();
      onClose();
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) {
        toast.error('That load was just taken by another carrier.');
        onAccepted();
      } else if (error instanceof ApiError && error.status === 403) {
        toast.error('Only carrier accounts can accept loads.');
      } else {
        toast.error('Could not accept the load — please try again.');
      }
    } finally {
      setAccepting(false);
    }
  }

  return (
    <>
      <div
        onClick={onClose}
        aria-hidden
        className={cn(
          'fixed inset-0 z-40 bg-primary/40 transition-opacity duration-300',
          isOpen ? 'opacity-100' : 'pointer-events-none opacity-0',
        )}
      />
      <aside
        role="dialog"
        aria-modal="true"
        aria-hidden={!isOpen}
        className={cn(
          'fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col overflow-y-auto border-l border-outline bg-surface-card shadow-elevated transition-transform duration-300 ease-out',
          isOpen ? 'translate-x-0' : 'translate-x-full',
        )}
      >
        {load ? (
          <div className="flex flex-col gap-6 p-6">
            <div className="flex items-start justify-between gap-3">
              <div className="flex flex-col gap-2">
                <span className="font-mono text-headline-sm font-semibold text-fleet-blue">{load.reference}</span>
                <div className="flex flex-wrap items-center gap-2">
                  <Badge tone={STATUS_TONE[load.status]}>{statusLabel(load.status)}</Badge>
                  <ServiceTypeBadge serviceType={load.serviceType} />
                </div>
              </div>
              <button
                type="button"
                onClick={onClose}
                aria-label="Close load details"
                className="flex h-9 w-9 shrink-0 items-center justify-center rounded border border-outline text-steel-gray hover:bg-surface-muted"
              >
                ×
              </button>
            </div>

            <StaticRouteMap load={load} />

            <dl className="grid grid-cols-2 gap-x-4 gap-y-3 font-mono text-body-sm">
              <div className="col-span-2 flex flex-col gap-0.5">
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Route</dt>
                <dd className="text-on-surface">
                  {load.originCity}, {load.originState} {load.originZip} &rarr; {load.destinationCity},{' '}
                  {load.destinationState} {load.destinationZip}
                </dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Equipment</dt>
                <dd className="text-on-surface">{load.equipmentType}</dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Distance</dt>
                <dd className="tabular-nums text-on-surface">{load.distanceMiles.toLocaleString()} mi</dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Weight</dt>
                <dd className="tabular-nums text-on-surface">{load.weightLbs.toLocaleString()} lb</dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">All-in rate</dt>
                <dd className="tabular-nums font-semibold text-primary">{currency.format(load.rateUsd)}</dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Pickup</dt>
                <dd className="tabular-nums text-on-surface">
                  {dateTimeFormatter.format(new Date(load.pickupAtUtc))}
                </dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Delivery</dt>
                <dd className="tabular-nums text-on-surface">
                  {dateTimeFormatter.format(new Date(load.deliveryAtUtc))}
                </dd>
              </div>
              {load.targetTemperatureF !== null ? (
                <div>
                  <dt className="flex items-center gap-1 text-label-sm uppercase tracking-wide text-steel-gray">
                    ❄ Target temp
                  </dt>
                  <dd className="tabular-nums text-on-surface">{load.targetTemperatureF}°F</dd>
                </div>
              ) : null}
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Shipper</dt>
                <dd className="text-on-surface">{load.shipperName}</dd>
              </div>
              {load.carrierName ? (
                <div>
                  <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Carrier</dt>
                  <dd className="text-on-surface">{load.carrierName}</dd>
                </div>
              ) : null}
            </dl>

            {load.specialInstructions ? (
              <div className="flex flex-col gap-2">
                <h3 className="font-mono text-label-sm uppercase tracking-wide text-steel-gray">
                  Special instructions
                </h3>
                <p className="font-body text-body-sm text-on-surface-variant">{load.specialInstructions}</p>
              </div>
            ) : null}

            <div className="mt-auto flex gap-3 border-t border-outline pt-5">
              <Button variant="outline" className="flex-1" onClick={handleBid} disabled={accepting}>
                Bid
              </Button>
              <Button
                variant="primary"
                className="flex-1"
                onClick={() => void handleAccept()}
                disabled={accepting || load.status !== 'Available'}
              >
                {accepting ? 'Accepting…' : load.status === 'Available' ? 'Accept load' : 'Not available'}
              </Button>
            </div>
          </div>
        ) : null}
      </aside>
    </>
  );
}
