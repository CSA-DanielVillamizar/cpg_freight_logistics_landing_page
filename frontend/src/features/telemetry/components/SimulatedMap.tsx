import type { TrackedLoad } from '../types';

interface SimulatedMapProps {
  load: TrackedLoad;
}

const W = 800;
const H = 520;
const MARGIN = 56;

/** Stylised route canvas — not a real map. Projects the GPS trail into the viewbox. */
export function SimulatedMap({ load }: SimulatedMapProps): JSX.Element {
  const points = load.coordinateHistory;
  const origin = points[0];
  const head = points[points.length - 1];

  if (!origin || !head) {
    return <svg viewBox={`0 0 ${W} ${H}`} className="h-full w-full" aria-hidden />;
  }

  const lats = points.map((p) => p.lat);
  const lngs = points.map((p) => p.lng);
  const minLat = Math.min(...lats);
  const maxLat = Math.max(...lats);
  const minLng = Math.min(...lngs);
  const maxLng = Math.max(...lngs);

  const spanLat = maxLat - minLat || 1;
  const spanLng = maxLng - minLng || 1;

  const projX = (lng: number): number => MARGIN + ((lng - minLng) / spanLng) * (W - MARGIN * 2);
  const projY = (lat: number): number => MARGIN + (1 - (lat - minLat) / spanLat) * (H - MARGIN * 2);

  const trail = points
    .map((p, i) => `${i === 0 ? 'M' : 'L'} ${projX(p.lng).toFixed(1)} ${projY(p.lat).toFixed(1)}`)
    .join(' ');

  const headX = projX(head.lng);
  const headY = projY(head.lat);

  // Projected remaining leg: continue along the reported heading toward the viewport edge.
  const rad = (load.headingDeg - 90) * (Math.PI / 180);
  const projEndX = headX + Math.cos(rad) * 220;
  const projEndY = headY + Math.sin(rad) * 220;

  return (
    <svg viewBox={`0 0 ${W} ${H}`} className="h-full w-full" role="img" aria-label={`Route for load ${load.reference}`}>
      <defs>
        <pattern id="tracking-grid" width="40" height="40" patternUnits="userSpaceOnUse">
          <path d="M 40 0 L 0 0 0 40" fill="none" stroke="rgba(255,255,255,0.05)" strokeWidth="1" />
        </pattern>
        <radialGradient id="tracking-glow" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="rgba(234,88,12,0.45)" />
          <stop offset="100%" stopColor="rgba(234,88,12,0)" />
        </radialGradient>
      </defs>

      <rect width={W} height={H} fill="#0B192C" />
      <rect width={W} height={H} fill="url(#tracking-grid)" />

      {/* projected remaining leg */}
      <path
        d={`M ${headX} ${headY} L ${projEndX.toFixed(1)} ${projEndY.toFixed(1)}`}
        stroke="rgba(148,163,184,0.55)"
        strokeWidth="2"
        strokeDasharray="6 7"
        fill="none"
      />

      {/* travelled trail */}
      <path d={trail} fill="none" stroke="#EA580C" strokeWidth="3" strokeLinejoin="round" strokeLinecap="round" />

      {/* origin */}
      <circle cx={projX(origin.lng)} cy={projY(origin.lat)} r="6" fill="#1C3766" stroke="white" strokeWidth="2" />
      <text
        x={projX(origin.lng)}
        y={projY(origin.lat) + 22}
        textAnchor="middle"
        fontSize="12"
        fill="#CBD5E1"
        fontFamily="'JetBrains Mono', monospace"
      >
        {load.originLabel}
      </text>

      {/* live position */}
      <circle cx={headX} cy={headY} r="26" fill="url(#tracking-glow)" />
      <circle cx={headX} cy={headY} r="7" fill="#EA580C" stroke="white" strokeWidth="2" />
      <text
        x={headX}
        y={headY - 18}
        textAnchor="middle"
        fontSize="12"
        fill="#F8FAFC"
        fontFamily="'JetBrains Mono', monospace"
      >
        {load.reference}
      </text>

      {/* readout */}
      <text x={MARGIN} y={H - 24} fontSize="12" fill="#94A3B8" fontFamily="'JetBrains Mono', monospace">
        {head.lat.toFixed(4)}°, {head.lng.toFixed(4)}° · {load.speedMph} mph · {load.headingLabel} ({load.headingDeg}°)
      </text>
      <text x={W - MARGIN} y={H - 24} textAnchor="end" fontSize="12" fill="#94A3B8" fontFamily="'JetBrains Mono', monospace">
        {load.distanceRemainingMiles} mi to {load.destinationLabel}
      </text>
    </svg>
  );
}
