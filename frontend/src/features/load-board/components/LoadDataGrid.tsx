import { Badge } from '@/shared/ui';
import type { Load } from '../types';
import { statusLabel, STATUS_TONE } from '../types';
import { ServiceTypeBadge } from './ServiceTypeBadge';

interface LoadDataGridProps {
  loads: readonly Load[];
  onSelect: (load: Load) => void;
}

const currency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});
const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit' });

export function LoadDataGrid({ loads, onSelect }: LoadDataGridProps): JSX.Element {
  if (loads.length === 0) {
    return (
      <div className="flex h-48 flex-col items-center justify-center gap-1 rounded-lg border border-outline bg-surface-card">
        <p className="font-mono text-body-sm text-steel-gray">No loads match the current filters.</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-outline bg-surface-card">
      <table className="w-full min-w-[960px] border-collapse text-left">
        <thead>
          <tr className="border-b border-outline bg-surface-muted">
            {['Load ID', 'Status', 'Service', 'Origin', 'Destination', 'Dist.', 'Weight', 'Rate', 'Pickup', 'Delivery'].map(
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
          {loads.map((load) => (
            <tr
              key={load.id}
              onClick={() => onSelect(load)}
              className="cursor-pointer transition-colors hover:bg-surface-muted"
            >
              <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm font-semibold text-fleet-blue">
                {load.reference}
              </td>
              <td className="whitespace-nowrap px-3 py-3">
                <Badge tone={STATUS_TONE[load.status]}>{statusLabel(load.status)}</Badge>
              </td>
              <td className="whitespace-nowrap px-3 py-3">
                <ServiceTypeBadge serviceType={load.serviceType} />
              </td>
              <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm text-on-surface-variant">
                {load.originCity}, {load.originState} {load.originZip}
              </td>
              <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm text-on-surface-variant">
                {load.destinationCity}, {load.destinationState} {load.destinationZip}
              </td>
              <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-on-surface-variant">
                {load.distanceMiles.toLocaleString()} mi
              </td>
              <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-on-surface-variant">
                {load.weightLbs.toLocaleString()} lb
              </td>
              <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm font-semibold tabular-nums text-primary">
                {currency.format(load.rateUsd)}
              </td>
              <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                {dateFormatter.format(new Date(load.pickupAtUtc))}
              </td>
              <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                {dateFormatter.format(new Date(load.deliveryAtUtc))}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
