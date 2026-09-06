import { VerticalIcon } from '@/features/landing/components/VerticalIcon';
import type { LoadServiceType } from '../types';
import { LOAD_SERVICE_TYPES } from '../types';

const SERVICE_ICON_SLUG: Record<LoadServiceType, string> = {
  ColdChain: 'refrigerated-cold-chain',
  HeavyHaul: 'flatbed-heavy-haul',
  Flatbed: 'flatbed-heavy-haul',
  FdotConcrete: 'fdot-concrete-barricades',
  StandardDryVan: 'standard-dry-van',
};

const SERVICE_LABEL: Record<LoadServiceType, string> = Object.fromEntries(
  LOAD_SERVICE_TYPES.map((entry) => [entry.value, entry.label]),
) as Record<LoadServiceType, string>;

interface ServiceTypeBadgeProps {
  serviceType: LoadServiceType;
  className?: string;
}

/** Corporate-color chip + icon for a load's service line (reuses the vertical icon set). */
export function ServiceTypeBadge({ serviceType, className }: ServiceTypeBadgeProps): JSX.Element {
  return (
    <span
      className={
        'inline-flex items-center gap-1.5 rounded-full border border-slate-200 bg-surface-muted px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wider text-on-surface-variant ' +
        (className ?? '')
      }
    >
      <VerticalIcon slug={SERVICE_ICON_SLUG[serviceType]} className="h-3.5 w-3.5 shrink-0 text-fleet-blue" />
      {SERVICE_LABEL[serviceType]}
    </span>
  );
}
