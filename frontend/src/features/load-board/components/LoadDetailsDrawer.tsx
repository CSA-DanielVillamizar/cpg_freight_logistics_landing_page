import { toast } from 'sonner';
import { cn } from '@/shared/lib/cn';
import { Badge, Button } from '@/shared/ui';
import type { Load } from '../mockLoads';
import { ServiceTypeBadge, STATUS_TONE } from './ServiceTypeBadge';

interface LoadDetailsDrawerProps {
  load: Load | null;
  onClose: () => void;
}

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
const dateTimeFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: '2-digit',
  hour: 'numeric',
  minute: '2-digit',
});

/** Purely decorative route sketch — no real geocoding backs this prototype. */
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
      <text x="48" y="138" textAnchor="middle" className="font-mono" fontSize="9" fill="#CBD5E1">
        {load.origin.state}
      </text>
      <text x="352" y="34" textAnchor="middle" className="font-mono" fontSize="9" fill="#CBD5E1">
        {load.destination.state}
      </text>
    </svg>
  );
}

export function LoadDetailsDrawer({ load, onClose }: LoadDetailsDrawerProps): JSX.Element {
  const isOpen = load !== null;

  function handleBid(): void {
    toast.success(`Bid submitted for ${load?.id} (demo — no backend wired yet).`);
  }

  function handleAccept(): void {
    toast.success(`${load?.id} accepted (demo — no backend wired yet).`);
    onClose();
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
                <span className="font-mono text-headline-sm font-semibold text-fleet-blue">{load.id}</span>
                <div className="flex flex-wrap items-center gap-2">
                  <Badge tone={STATUS_TONE[load.status]}>
                    {load.status === 'InTransit' ? 'In Transit' : load.status}
                  </Badge>
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
                  {load.origin.city}, {load.origin.state} {load.origin.zip} &rarr; {load.destination.city},{' '}
                  {load.destination.state} {load.destination.zip}
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
                <dd className="tabular-nums text-on-surface">{dateTimeFormatter.format(new Date(load.pickupDateUtc))}</dd>
              </div>
              <div>
                <dt className="text-label-sm uppercase tracking-wide text-steel-gray">Delivery</dt>
                <dd className="tabular-nums text-on-surface">{dateTimeFormatter.format(new Date(load.deliveryDateUtc))}</dd>
              </div>
              {load.targetTemperatureF !== undefined ? (
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

            {load.specialRequirements.length > 0 ? (
              <div className="flex flex-col gap-2">
                <h3 className="font-mono text-label-sm uppercase tracking-wide text-steel-gray">
                  Special requirements
                </h3>
                <ul className="flex flex-col gap-1 font-body text-body-sm text-on-surface-variant">
                  {load.specialRequirements.map((requirement) => (
                    <li key={requirement} className="flex items-start gap-2">
                      <span className="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-hazard-orange" />
                      {requirement}
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}

            <div className="mt-auto flex gap-3 border-t border-outline pt-5">
              <Button variant="outline" className="flex-1" onClick={handleBid}>
                Bid
              </Button>
              <Button variant="primary" className="flex-1" onClick={handleAccept}>
                Accept load
              </Button>
            </div>
          </div>
        ) : null}
      </aside>
    </>
  );
}
