/**
 * Static "profile" context for tracked loads — the parts a GPS/telemetry feed does not carry
 * (driver, tractor unit, milestone timeline, reefer band + seed history, initial trail). The
 * live position/temperature/speed come from the SignalR hub; everything here is the scaffold
 * they animate on top of. Keyed by load reference.
 */

import type { Load } from '@/features/load-board/types';
import type {
  ColdChainTelemetry,
  GpsPoint,
  TempReading,
  TimelineEvent,
  TrackedLoad,
  TrackedServiceType,
} from './types';

interface TelemetryProfile {
  driver: string;
  tractorUnit: string;
  headingDeg: number;
  headingLabel: string;
  speedMph: number;
  progressPct: number;
  distanceRemainingMiles: number;
  seedTrail: [number, number][];
  reeferBand?: [number, number] | undefined;
  timeline: TimelineEvent[];
}

const NOW = Date.now();
const agoIso = (minutes: number): string => new Date(NOW - minutes * 60_000).toISOString();
const aheadIso = (minutes: number): string => new Date(NOW + minutes * 60_000).toISOString();

function trail(points: [number, number][], oldestMinutesAgo: number, stepMinutes: number): GpsPoint[] {
  return points.map(([lat, lng], index) => ({
    lat,
    lng,
    atUtc: agoIso(oldestMinutesAgo - index * stepMinutes),
  }));
}

/** Plausible reefer series drifting inside a band (°C). */
function seedTempHistory(points: number, band: [number, number]): TempReading[] {
  const [low, high] = band;
  const mid = (low + high) / 2;
  const readings: TempReading[] = [];
  for (let i = points - 1; i >= 0; i -= 1) {
    const wave = Math.sin(i / 2.2) * ((high - low) / 2.4);
    const celsius = mid + wave + (i % 3 === 0 ? 0.4 : -0.3);
    readings.push({ atUtc: agoIso(i * 15), celsius: Math.round(celsius * 10) / 10 });
  }
  return readings;
}

const TELEMETRY_PROFILES: Record<string, TelemetryProfile> = {
  'CPG-48219': {
    driver: 'R. Delgado',
    tractorUnit: 'T-1187 · Peterbilt 389',
    headingDeg: 291,
    headingLabel: 'WNW',
    speedMph: 61,
    progressPct: 58,
    distanceRemainingMiles: 276,
    seedTrail: [
      [28.54, -81.38],
      [29.65, -83.1],
      [30.19, -84.62],
      [30.55, -86.78],
      [30.69, -87.05],
    ],
    timeline: [
      { label: 'Dispatched', detail: 'Orlando yard · RGN multi-axle', atUtc: agoIso(430), kind: 'dispatched', complete: true },
      { label: 'Loaded & secured', detail: '51,200 lb · 6-point chain, DOT inspection passed', atUtc: agoIso(392), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Tallahassee, FL', detail: 'I-10 W · escort confirmed', atUtc: agoIso(150), kind: 'checkpoint', complete: true },
      { label: 'Checkpoint — Pensacola, FL', detail: 'Fuel + brake check · on schedule', atUtc: agoIso(24), kind: 'checkpoint', complete: true },
      { label: 'Arrival — New Orleans, LA', detail: 'Consignee dock 4', atUtc: aheadIso(268), kind: 'arrival', complete: false },
    ],
  },
  'CPG-48226': {
    driver: 'M. Osei',
    tractorUnit: 'T-1204 · Kenworth T680',
    headingDeg: 18,
    headingLabel: 'NNE',
    speedMph: 64,
    progressPct: 41,
    distanceRemainingMiles: 324,
    reeferBand: [-5, 5],
    seedTrail: [
      [28.54, -81.38],
      [30.33, -81.66],
      [31.21, -81.49],
      [32.08, -81.09],
    ],
    timeline: [
      { label: 'Dispatched', detail: 'Orlando cross-dock · life-science reefer', atUtc: agoIso(360), kind: 'dispatched', complete: true },
      { label: 'Loaded & pre-cooled', detail: '18,700 lb · seal #44192', atUtc: agoIso(320), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Jacksonville, FL', detail: 'Temp log verified', atUtc: agoIso(120), kind: 'checkpoint', complete: true },
      { label: 'Checkpoint — Savannah, GA', detail: 'Driver swap · reefer fuel 71%', atUtc: agoIso(10), kind: 'checkpoint', complete: true },
      { label: 'Arrival — Raleigh, NC', detail: 'BioCore receiving · chain-of-custody', atUtc: aheadIso(315), kind: 'arrival', complete: false },
    ],
  },
  'CPG-48231': {
    driver: 'K. Whitfield',
    tractorUnit: 'T-1152 · Volvo VNL 860',
    headingDeg: 342,
    headingLabel: 'NNW',
    speedMph: 58,
    progressPct: 66,
    distanceRemainingMiles: 156,
    reeferBand: [-22, -16],
    seedTrail: [
      [28.01, -82.11],
      [30.44, -83.28],
      [31.58, -83.9],
      [32.62, -84.1],
      [33.24, -84.28],
    ],
    timeline: [
      { label: 'Dispatched', detail: 'Plant City grower dock · deep-freeze trailer', atUtc: agoIso(345), kind: 'dispatched', complete: true },
      { label: 'Loaded & pre-cooled', detail: '41,000 lb frozen produce · −20 °C', atUtc: agoIso(300), kind: 'loaded', complete: true },
      { label: 'Checkpoint — Lake City, FL', detail: 'Door-seal integrity check passed', atUtc: agoIso(150), kind: 'checkpoint', complete: true },
      { label: 'Checkpoint — Valdosta, GA', detail: 'Reefer fuel 64%', atUtc: agoIso(28), kind: 'checkpoint', complete: true },
      { label: 'Arrival — Atlanta, GA', detail: 'Cold-storage DC 7', atUtc: aheadIso(126), kind: 'arrival', complete: false },
    ],
  },
};

function defaultProfile(load: Load): TelemetryProfile {
  return {
    driver: 'Unassigned',
    tractorUnit: load.equipmentType,
    headingDeg: 20,
    headingLabel: 'NNE',
    speedMph: 60,
    progressPct: 45,
    distanceRemainingMiles: Math.round(load.distanceMiles * 0.55),
    reeferBand: load.serviceType === 'ColdChain' ? [-20, -15] : undefined,
    seedTrail: [
      [28.54, -81.38],
      [29.4, -81.5],
      [30.2, -81.7],
    ],
    timeline: [
      { label: 'Dispatched', detail: `${load.originCity} · ${load.equipmentType}`, atUtc: agoIso(300), kind: 'dispatched', complete: true },
      { label: 'Loaded & secured', detail: `${load.weightLbs.toLocaleString()} lb`, atUtc: agoIso(260), kind: 'loaded', complete: true },
      { label: 'In transit', detail: 'Position telemetry live', atUtc: agoIso(30), kind: 'checkpoint', complete: true },
      { label: `Arrival — ${load.destinationCity}, ${load.destinationState}`, detail: load.shipperName, atUtc: load.deliveryAtUtc, kind: 'arrival', complete: false },
    ],
  };
}

function buildTemperature(load: Load, profile: TelemetryProfile): ColdChainTelemetry | undefined {
  if (load.serviceType !== 'ColdChain') {
    return undefined;
  }
  const band: [number, number] =
    profile.reeferBand ??
    (load.targetTemperatureF !== null
      ? [((load.targetTemperatureF - 32) * 5) / 9 - 2.5, ((load.targetTemperatureF - 32) * 5) / 9 + 2.5]
      : [-20, -15]);
  const history = seedTempHistory(24, band);
  return {
    currentCelsius: history[history.length - 1]?.celsius ?? (band[0] + band[1]) / 2,
    setpointCelsius: Math.round(((band[0] + band[1]) / 2) * 10) / 10,
    minCelsius: band[0],
    maxCelsius: band[1],
    history,
  };
}

export function buildTrackedLoad(load: Load): TrackedLoad {
  const profile = TELEMETRY_PROFILES[load.reference] ?? defaultProfile(load);
  const coordinateHistory = trail(profile.seedTrail, 300, 55);
  const currentPosition =
    coordinateHistory[coordinateHistory.length - 1] ?? { lat: 28.54, lng: -81.38, atUtc: agoIso(0) };

  return {
    id: load.id,
    reference: load.reference,
    serviceType: load.serviceType as TrackedServiceType,
    driver: profile.driver,
    tractorUnit: profile.tractorUnit,
    originLabel: `${load.originCity}, ${load.originState}`,
    destinationLabel: `${load.destinationCity}, ${load.destinationState}`,
    distanceRemainingMiles: profile.distanceRemainingMiles,
    speedMph: profile.speedMph,
    headingDeg: profile.headingDeg,
    headingLabel: profile.headingLabel,
    etaUtc: load.deliveryAtUtc,
    lastPingUtc: agoIso(0),
    progressPct: profile.progressPct,
    currentPosition,
    coordinateHistory,
    temperature: buildTemperature(load, profile),
    timeline: profile.timeline,
  };
}
