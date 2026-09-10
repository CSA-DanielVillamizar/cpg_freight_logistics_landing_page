/** Mirrors the backend LoadSummaryResponse DTO (GET /api/loads). */

import { formatEnum } from '@/shared/lib/formatEnum';
import type { BadgeTone } from '@/shared/ui';

export type LoadStatus = 'Available' | 'Dispatched' | 'InTransit' | 'Delivered';

export type LoadServiceType =
  | 'ColdChain'
  | 'HeavyHaul'
  | 'Flatbed'
  | 'FdotConcrete'
  | 'StandardDryVan';

export interface Load {
  id: string;
  reference: string;
  status: LoadStatus;
  serviceType: LoadServiceType;
  equipmentType: string;
  originCity: string;
  originState: string;
  originZip: string;
  destinationCity: string;
  destinationState: string;
  destinationZip: string;
  distanceMiles: number;
  weightLbs: number;
  rateUsd: number;
  shipperName: string;
  carrierName: string | null;
  pickupAtUtc: string;
  deliveryAtUtc: string;
  targetTemperatureF: number | null;
  specialInstructions: string | null;
}

/** POST /api/loads body. Mirrors the backend CreateLoadRequest DTO. */
export interface CreateLoadInput {
  reference?: string;
  serviceType: LoadServiceType;
  equipmentType: string;
  originCity: string;
  originState: string;
  originZip: string;
  destinationCity: string;
  destinationState: string;
  destinationZip: string;
  distanceMiles: number;
  weightLbs: number;
  rateUsd: number;
  shipperName: string;
  pickupAtUtc: string;
  deliveryAtUtc: string;
  targetTemperatureF?: number;
  specialInstructions?: string;
}

export const LOAD_STATUSES: readonly LoadStatus[] = [
  'Available',
  'Dispatched',
  'InTransit',
  'Delivered',
];

export const LOAD_SERVICE_TYPES: readonly { value: LoadServiceType; label: string }[] = [
  { value: 'ColdChain', label: 'Cold Chain' },
  { value: 'HeavyHaul', label: 'Heavy Haul' },
  { value: 'FdotConcrete', label: 'FDOT Barricades' },
  { value: 'Flatbed', label: 'Flatbed' },
  { value: 'StandardDryVan', label: 'Standard Dry Van' },
];

export function statusLabel(status: LoadStatus): string {
  return formatEnum(status);
}

export const STATUS_TONE: Record<LoadStatus, BadgeTone> = {
  Available: 'available',
  Dispatched: 'dispatched',
  InTransit: 'transit',
  Delivered: 'delivered',
};

/** GET /api/loads/{id}/location response (T-SDD Epica 2A). */
export interface LoadLocationResponse {
  loadId: string;
  latitude: number | null;
  longitude: number | null;
  lastTelemetryAtUtc: string | null;
}

/** A row of GET /api/loads/{id}/telemetry-history (T-SDD Epica 2A). */
export interface TelemetryLogEntryResponse {
  latitude: number;
  longitude: number;
  speedMph: number | null;
  headingDegrees: number | null;
  recordedAtUtc: string;
}
