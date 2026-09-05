import type { ColdChainTelemetry } from '../mockTelemetry';

interface SparklineProps {
  telemetry: ColdChainTelemetry;
}

const WIDTH = 320;
const HEIGHT = 96;
const PAD = 6;

/** Pure-SVG temperature trend for the last few hours. No charting library. */
export function Sparkline({ telemetry }: SparklineProps): JSX.Element {
  const values = telemetry.history.map((reading) => reading.celsius);
  const domainMin = Math.min(telemetry.minCelsius, ...values) - 1;
  const domainMax = Math.max(telemetry.maxCelsius, ...values) + 1;

  const x = (index: number): number =>
    PAD + (index / (telemetry.history.length - 1)) * (WIDTH - PAD * 2);
  const y = (celsius: number): number =>
    PAD + (1 - (celsius - domainMin) / (domainMax - domainMin)) * (HEIGHT - PAD * 2);

  const linePath = telemetry.history
    .map((reading, index) => `${index === 0 ? 'M' : 'L'} ${x(index).toFixed(1)} ${y(reading.celsius).toFixed(1)}`)
    .join(' ');

  const areaPath = `${linePath} L ${x(telemetry.history.length - 1).toFixed(1)} ${HEIGHT - PAD} L ${PAD} ${HEIGHT - PAD} Z`;

  const bandTop = y(telemetry.maxCelsius);
  const bandBottom = y(telemetry.minCelsius);

  return (
    <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="h-24 w-full" role="img" aria-label="Temperature trend">
      {/* Allowed band */}
      <rect
        x={PAD}
        y={bandTop}
        width={WIDTH - PAD * 2}
        height={Math.max(0, bandBottom - bandTop)}
        fill="rgba(28,55,102,0.12)"
      />
      <line x1={PAD} y1={bandTop} x2={WIDTH - PAD} y2={bandTop} stroke="#94A3B8" strokeWidth="1" strokeDasharray="3 3" />
      <line x1={PAD} y1={bandBottom} x2={WIDTH - PAD} y2={bandBottom} stroke="#94A3B8" strokeWidth="1" strokeDasharray="3 3" />

      <path d={areaPath} fill="rgba(234,88,12,0.10)" />
      <path d={linePath} fill="none" stroke="#EA580C" strokeWidth="2" strokeLinejoin="round" strokeLinecap="round" />

      {telemetry.history.map((reading, index) => {
        const breached = reading.celsius > telemetry.maxCelsius || reading.celsius < telemetry.minCelsius;
        if (!breached) {
          return null;
        }
        return <circle key={reading.atUtc} cx={x(index)} cy={y(reading.celsius)} r="2.5" fill="#DD1A1A" />;
      })}

      <circle
        cx={x(telemetry.history.length - 1)}
        cy={y(values[values.length - 1] ?? telemetry.currentCelsius)}
        r="3.5"
        fill="#0B192C"
        stroke="#EA580C"
        strokeWidth="2"
      />
    </svg>
  );
}
