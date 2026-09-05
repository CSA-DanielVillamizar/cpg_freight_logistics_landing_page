/**
 * Dummy data for the Live Tracking & Telemetry prototype. Nothing here is wired to a
 * backend — SPEC.md has no telemetry endpoints yet. Used to validate the UX only.
 */

export type TrackedServiceType = 'ColdChain' | 'HeavyHaul';

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

export interface TrackedLoad {
  id: string;
  reference: string;
  serviceType: TrackedServiceType;
  driver: string;
  tractorUnit: string;
  originLabel: string;
  destinationLabel: string;
  progressPct: number;
  distanceRemainingMiles: number;
  speedMph: number;
  headingDeg: number;
  headingLabel: string;
  etaUtc: string;
  lastPingUtc: string;
  currentPosition: GpsPoint;
  coordinateHistory: GpsPoint[];
  temperature?: ColdChainTelemetry;
  timeline: TimelineEvent[];
}

const NOW = new Date('2026-09-05T15:40:00Z').getTime();
const iso = (minutesAgo: number): string => new Date(NOW - minutesAgo * 60_000).toISOString();
const isoAhead = (minutesAhead: number): string => new Date(NOW + minutesAhead * 60_000).toISOString();

/** Builds a plausible reefer temperature series (°C) that drifts inside a band, optionally breaching it. */
function buildTempHistory(
  points: number,
  band: [number, number],
  options: { breach?: boolean } = {},
): TempReading[] {
  const [low, high] = band;
  const mid = (low + high) / 2;
  const readings: TempReading[] = [];
  for (let i = points - 1; i >= 0; i -= 1) {
    const wave = Math.sin(i / 2.2) * ((high - low) / 2.4);
    let celsius = mid + wave + (i % 3 === 0 ? 0.4 : -0.3);
    if (options.breach && i <= 5) {
      // Last ~75 min: door seal fault, temperature climbs out of band.
      celsius = high + (6 - i) * 1.15;
    }
    readings.push({ atUtc: iso(i * 15), celsius: Math.round(celsius * 10) / 10 });
  }
  return readings;
}

function line(points: [number, number][], startMinutesAgo: number, stepMinutes: number): GpsPoint[] {
  return points.map(([lat, lng], index) => ({
    lat,
    lng,
    atUtc: iso(startMinutesAgo - index * stepMinutes),
  }));
}

export const MOCK_TRACKED_LOADS: readonly TrackedLoad[] = [
  {
    id: '5c0e2c9a-0001-4a1a-9f01-aa0000000001',
    reference: 'CPG-48219',
    serviceType: 'HeavyHaul',
    driver: 'R. Delgado',
    tractorUnit: 'T-1187 · Peterbilt 389',
    originLabel: 'Orlando, FL',
    destinationLabel: 'New Orleans, LA',
    progressPct: 58,
    distanceRemainingMiles: 276,
    speedMph: 61,
    headingDeg: 291,
    headingLabel: 'WNW',
    etaUtc: isoAhead(268),
    lastPingUtc: iso(1),
    currentPosition: { lat: 30.69, lng: -87.05, atUtc: iso(1) },
    coordinateHistory: line(
      [
        [28.54, -81.38],
        [28.9, -82.14],
        [29.65, -83.1],
        [30.19, -84.62],
        [30.42, -86.1],
        [30.55, -86.78],
        [30.69, -87.05],
      ],
      360,
      55,
    ),
    timeline: [
      { label: 'Dispatched', detail: 'Orlando yard · RGN multi-axle', atUtc: iso(430), kind: 'dispatched', complete: true },
      { label: 'Loaded & secured', detail: '51,200 lb · 6-point chain, DOT inspection passed', atUtc: iso(392), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Tallahassee, FL', detail: 'I-10 W · escort confirmed', atUtc: iso(150), kind: 'checkpoint', complete: true },
      { label: 'Checkpoint — Pensacola, FL', detail: 'Fuel + brake check · on schedule', atUtc: iso(24), kind: 'checkpoint', complete: true },
      { label: 'Arrival — New Orleans, LA', detail: 'Consignee dock 4', atUtc: isoAhead(268), kind: 'arrival', complete: false },
    ],
  },
  {
    id: '5c0e2c9a-0002-4a1a-9f01-aa0000000002',
    reference: 'CPG-48226',
    serviceType: 'ColdChain',
    driver: 'M. Osei',
    tractorUnit: 'T-1204 · Kenworth T680',
    originLabel: 'Orlando, FL',
    destinationLabel: 'Raleigh, NC',
    progressPct: 41,
    distanceRemainingMiles: 324,
    speedMph: 64,
    headingDeg: 18,
    headingLabel: 'NNE',
    etaUtc: isoAhead(315),
    lastPingUtc: iso(2),
    currentPosition: { lat: 32.08, lng: -81.09, atUtc: iso(2) },
    coordinateHistory: line(
      [
        [28.54, -81.38],
        [29.21, -81.05],
        [30.33, -81.66],
        [31.21, -81.49],
        [32.08, -81.09],
      ],
      300,
      60,
    ),
    temperature: {
      currentCelsius: -17.6,
      setpointCelsius: -18,
      minCelsius: -20,
      maxCelsius: -15,
      history: buildTempHistory(24, [-20, -15]),
    },
    timeline: [
      { label: 'Dispatched', detail: 'Orlando cross-dock · life-science reefer', atUtc: iso(360), kind: 'dispatched', complete: true },
      { label: 'Loaded & pre-cooled', detail: '18,700 lb · 2 h pull-down to −18 °C, seal #44192', atUtc: iso(320), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Jacksonville, FL', detail: 'Temp log verified · −17.9 °C', atUtc: iso(120), kind: 'checkpoint', complete: true },
      { label: 'Checkpoint — Savannah, GA', detail: 'Driver swap · reefer fuel 71%', atUtc: iso(10), kind: 'checkpoint', complete: true },
      { label: 'Arrival — Raleigh, NC', detail: 'BioCore receiving · chain-of-custody', atUtc: isoAhead(315), kind: 'arrival', complete: false },
    ],
  },
  {
    id: '5c0e2c9a-0003-4a1a-9f01-aa0000000003',
    reference: 'CPG-48231',
    serviceType: 'ColdChain',
    driver: 'K. Whitfield',
    tractorUnit: 'T-1152 · Volvo VNL 860',
    originLabel: 'Plant City, FL',
    destinationLabel: 'Atlanta, GA',
    progressPct: 73,
    distanceRemainingMiles: 118,
    speedMph: 58,
    headingDeg: 342,
    headingLabel: 'NNW',
    etaUtc: isoAhead(126),
    lastPingUtc: iso(1),
    currentPosition: { lat: 33.24, lng: -84.28, atUtc: iso(1) },
    coordinateHistory: line(
      [
        [28.01, -82.11],
        [29.19, -82.14],
        [30.44, -83.28],
        [31.58, -83.9],
        [32.62, -84.1],
        [33.24, -84.28],
      ],
      330,
      52,
    ),
    temperature: {
      currentCelsius: -9.2,
      setpointCelsius: -18,
      minCelsius: -20,
      maxCelsius: -15,
      history: buildTempHistory(24, [-20, -15], { breach: true }),
    },
    timeline: [
      { label: 'Dispatched', detail: 'Plant City grower dock · deep-freeze trailer', atUtc: iso(345), kind: 'dispatched', complete: true },
      { label: 'Loaded & pre-cooled', detail: '41,000 lb frozen produce · −20 °C', atUtc: iso(300), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Lake City, FL', detail: 'Temp log verified · −19.4 °C', atUtc: iso(150), kind: 'checkpoint', complete: true },
      { label: 'Temperature excursion', detail: 'Door-seal fault alarm · reefer at −9.2 °C, dispatch notified', atUtc: iso(38), kind: 'delay', complete: true },
      { label: 'Arrival — Atlanta, GA', detail: 'Cold-storage DC 7 · QA hold expected', atUtc: isoAhead(126), kind: 'arrival', complete: false },
    ],
  },
  {
    id: '5c0e2c9a-0004-4a1a-9f01-aa0000000004',
    reference: 'CPG-48214',
    serviceType: 'HeavyHaul',
    driver: 'J. Barnes',
    tractorUnit: 'T-1176 · Mack Anthem',
    originLabel: 'Tampa, FL',
    destinationLabel: 'Savannah, GA',
    progressPct: 22,
    distanceRemainingMiles: 301,
    speedMph: 55,
    headingDeg: 27,
    headingLabel: 'NNE',
    etaUtc: isoAhead(352),
    lastPingUtc: iso(3),
    currentPosition: { lat: 28.9, lng: -82.0, atUtc: iso(3) },
    coordinateHistory: line(
      [
        [27.95, -82.46],
        [28.35, -82.19],
        [28.9, -82.0],
      ],
      120,
      55,
    ),
    timeline: [
      { label: 'Dispatched', detail: 'Tampa port · superload permit + escort', atUtc: iso(140), kind: 'dispatched', complete: true },
      { label: 'Loaded & secured', detail: '96,500 lb · pole car front & rear', atUtc: iso(110), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Brooksville, FL', detail: 'Route survey clear · bridge analysis on file', atUtc: iso(20), kind: 'checkpoint', complete: true },
      { label: 'Checkpoint — Lake City, FL', detail: 'Scheduled scale stop', atUtc: isoAhead(95), kind: 'checkpoint', complete: false },
      { label: 'Arrival — Savannah, GA', detail: 'Gulf Coast Marine yard', atUtc: isoAhead(352), kind: 'arrival', complete: false },
    ],
  },
];

export function isTemperatureBreached(temp: ColdChainTelemetry): boolean {
  return temp.currentCelsius > temp.maxCelsius || temp.currentCelsius < temp.minCelsius;
}
