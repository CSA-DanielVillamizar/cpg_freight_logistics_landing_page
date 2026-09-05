export type TrackedServiceType = 'ColdChain' | 'HeavyHaul' | 'Flatbed' | 'FdotConcrete' | 'StandardDryVan';

export interface GpsPoint {
  lat: number;
  lng: number;
  atUtc: string;
}

export interface TempReading {
  atUtc: string;
  celsius: number;
}

export type TimelineKind = 'dispatched' | 'loaded' | 'checkpoint' | 'delay' | 'arrival';

export interface TimelineEvent {
  label: string;
  detail: string;
  atUtc: string;
  kind: TimelineKind;
  complete: boolean;
}

export interface ColdChainTelemetry {
  currentCelsius: number;
  setpointCelsius: number;
  minCelsius: number;
  maxCelsius: number;
  history: TempReading[];
}

/** A load in transit, rendered by the Live Tracking panel. Seeded from GET /api/loads and
 *  then mutated in place by SignalR ReceiveTelemetryUpdate events. */
export interface TrackedLoad {
  id: string;
  reference: string;
  serviceType: TrackedServiceType;
  driver: string;
  tractorUnit: string;
  originLabel: string;
  destinationLabel: string;
  distanceRemainingMiles: number;
  speedMph: number;
  headingDeg: number;
  headingLabel: string;
  etaUtc: string;
  lastPingUtc: string;
  progressPct: number;
  currentPosition: GpsPoint;
  coordinateHistory: GpsPoint[];
  temperature?: ColdChainTelemetry | undefined;
  timeline: TimelineEvent[];
}

/** The live payload pushed over the SignalR hub (mirrors the backend TelemetryReading record). */
export interface TelemetryReading {
  loadId: string;
  latitude: number;
  longitude: number;
  temperatureCelsius: number | null;
  speedMph: number;
  timestampUtc: string;
}

export function isTemperatureBreached(temp: ColdChainTelemetry): boolean {
  return temp.currentCelsius > temp.maxCelsius || temp.currentCelsius < temp.minCelsius;
}
