/**
 * Turns a raw backend enum value into human-readable display text.
 *
 *   formatEnum('ColdChain')      -> 'Cold Chain'
 *   formatEnum('InTransit')      -> 'In Transit'
 *   formatEnum('FdotConcrete')   -> 'FDOT Barricades'
 *   formatEnum('STANDARDDRYVAN') -> 'Standard Dry Van'
 *
 * Known acronym / branded values are handled by the override table; everything
 * else falls back to splitting on case boundaries + separators and title-casing.
 */
const OVERRIDES: Record<string, string> = {
  // Service lines
  coldchain: 'Cold Chain',
  heavyhaul: 'Heavy Haul',
  flatbed: 'Flatbed',
  fdotconcrete: 'FDOT Barricades',
  standarddryvan: 'Standard Dry Van',
  // Load statuses
  available: 'Available',
  dispatched: 'Dispatched',
  intransit: 'In Transit',
  delivered: 'Delivered',
  // Compliance statuses
  pendingcompliance: 'Pending Compliance',
  underreview: 'Under Review',
  verified: 'Verified',
  rejected: 'Rejected',
  // Compliance document types
  certificateofinsurance: 'Certificate of Insurance',
  generalliabilityinsurance: 'General Liability Insurance',
  fdotpermit: 'FDOT Permit',
  operatingauthority: 'Operating Authority',
  w9: 'W-9',
  // Invoice statuses
  draft: 'Draft',
  pending: 'Pending',
  paid: 'Paid',
  overdue: 'Overdue',
};

export function formatEnum(value: string | null | undefined): string {
  if (!value) {
    return '';
  }

  const key = value.replace(/[\s_-]+/g, '').toLowerCase();
  const override = OVERRIDES[key];
  if (override !== undefined) {
    return override;
  }

  const spaced = value
    .replace(/[_-]+/g, ' ')
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2') // camelCase / PascalCase boundary
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2') // ACRONYMWord boundary
    .replace(/\s+/g, ' ')
    .trim();

  return spaced.replace(/\b\w/g, (char) => char.toUpperCase());
}
