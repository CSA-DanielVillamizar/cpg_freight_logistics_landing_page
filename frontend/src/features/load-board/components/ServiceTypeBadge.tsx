import { VerticalIcon } from '@/features/landing/components/VerticalIcon';
import type { LoadServiceType, LoadStatus } from '../mockLoads';
import { LOAD_SERVICE_TYPES } from '../mockLoads';
import type { BadgeTone } from '@/shared/ui';

const SERVICE_ICON_SLUG: Record<LoadServiceType, string> = {
  ColdChain: 'refrigerated-cold-chain',
  HeavyHaul: 'flatbed-heavy-haul',
  FdotConcrete: 'fdot-concrete-barricades',
  StandardDryVan: 'standard-dry-van',
};

const SERVICE_LABEL: Record<LoadServiceType, string> = Object.fromEntries(
  LOAD_SERVICE_TYPES.map((entry) => [entry.value, entry.label]),
) as Record<LoadServiceType, string>;

export const STATUS_TONE: Record<LoadStatus, BadgeTone> = {
  Available: 'available',
  Dispatched: 'dispatched',
  InTransit: 'transit',
  Delivered: 'delivered',
};

interface ServiceTypeBadgeProps {
  serviceType: LoadServiceType;
  className?: string;
}

/** Corporate-color chip + icon for a load's service line (reuses the vertical icon set). */
export function ServiceTypeBadge({ serviceType, className }: ServiceTypeBadgeProps): JSX.Element {
  return (
    <span
      className={
        'inline-flex items-center gap-1.5 rounded border border-outline bg-surface-muted px-2 py-1 font-mono text-label-sm uppercase tracking-wide text-on-surface-variant ' +
        (className ?? '')
      }
    >
      <VerticalIcon slug={SERVICE_ICON_SLUG[serviceType]} className="h-3.5 w-3.5 shrink-0 text-fleet-blue" />
      {SERVICE_LABEL[serviceType]}
    </span>
  );
}
