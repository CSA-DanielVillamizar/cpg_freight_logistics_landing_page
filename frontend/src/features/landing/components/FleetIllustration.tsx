interface FleetIllustrationProps {
  name: string;
  className?: string;
}

/**
 * Flat geometric scene illustrations for the heavy-haul equipment cards and the proof band.
 * Brand palette only (navy / fleet-blue / hazard-orange / safety-amber / steel), no external
 * assets. Card scenes share a 400x150 frame; the proof band is 800x200.
 */
export function FleetIllustration({ name, className }: FleetIllustrationProps): JSX.Element {
  if (name === 'rigging') {
    return (
      <svg
        viewBox="0 0 800 200"
        className={className}
        preserveAspectRatio="xMidYMid slice"
        role="img"
        aria-label="Grade 100 transport chains securing steel cargo to a flatbed deck"
      >
        <defs>
          <linearGradient id="rig-sky" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0" stopColor="#0E1C2F" />
            <stop offset="1" stopColor="#0b1728" />
          </linearGradient>
        </defs>
        <rect width="800" height="200" fill="url(#rig-sky)" />
        <g opacity="0.12" fill="#d6e3fe">
          {Array.from({ length: 22 }).map((_, col) =>
            Array.from({ length: 6 }).map((__, row) => (
              <circle key={`${col}-${row}`} cx={16 + col * 36} cy={14 + row * 30} r="1.4" />
            )),
          )}
        </g>
        {/* deck edge */}
        <rect x="0" y="150" width="800" height="50" fill="#16283f" />
        <rect x="0" y="150" width="800" height="4" fill="#24467F" />
        {/* secured steel block (right side, out of the way of the stats) */}
        <g transform="translate(590 52)">
          <rect x="0" y="0" width="150" height="86" rx="3" fill="#2a3a4f" />
          <rect x="0" y="0" width="150" height="12" fill="#3b4d63" />
          <rect x="0" y="74" width="150" height="12" fill="#1f2937" />
          <rect x="66" y="12" width="18" height="62" fill="#3b4d63" />
        </g>
        {/* chain — low-contrast texture, kept below the stat row */}
        <g
          fill="none"
          stroke="#3d5170"
          strokeWidth="10"
          transform="translate(0 150) rotate(-3 400 0)"
        >
          {Array.from({ length: 12 }).map((_, i) => (
            <ellipse key={i} cx={40 + i * 68} cy={0} rx="26" ry="16" />
          ))}
        </g>
        {/* orange ratchet binder on the chain */}
        <g transform="translate(150 128) rotate(-3)">
          <rect x="-16" y="-10" width="52" height="20" rx="4" fill="#EA580C" />
          <rect x="30" y="-15" width="9" height="30" rx="3" fill="#c2410c" />
          <circle cx="8" cy="0" r="4" fill="#0E1C2F" />
        </g>
      </svg>
    );
  }

  return (
    <svg
      viewBox="0 0 400 150"
      className={className}
      preserveAspectRatio="xMidYMid slice"
      role="img"
      aria-label={SCENE_LABEL[name] ?? 'CPG specialized freight equipment'}
    >
      <defs>
        <linearGradient id={`fleet-sky-${name}`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#1C3766" />
          <stop offset="1" stopColor="#0E1C2F" />
        </linearGradient>
      </defs>
      <rect width="400" height="150" fill={`url(#fleet-sky-${name})`} />
      <g opacity="0.14" fill="#d6e3fe">
        {Array.from({ length: 13 }).map((_, col) =>
          Array.from({ length: 4 }).map((__, row) => (
            <circle key={`${col}-${row}`} cx={14 + col * 31} cy={12 + row * 26} r="1.3" />
          )),
        )}
      </g>
      {/* ground */}
      <rect x="0" y="118" width="400" height="32" fill="#0b1728" />
      <rect x="0" y="118" width="400" height="3" fill="#EA580C" opacity="0.85" />
      <g transform="translate(0 4)">{SCENES[name] ?? SCENES['flatbed-steel']}</g>
    </svg>
  );
}

const SCENE_LABEL: Record<string, string> = {
  'flatbed-steel': 'Flatbed trailer loaded with structural steel I-beams',
  'step-deck': 'Step-deck trailer carrying over-height industrial crates',
  'rgn-lowboy': 'RGN lowboy trailer with a tracked excavator on the low well',
  'auto-transport': 'Multi-level auto transport hauler loaded with fleet vehicles',
};

/** Shared cab + wheel primitives keep the four scenes visually consistent. */
function Cab(): JSX.Element {
  return (
    <g>
      <path d="M8 114V78c0-3 2-5 5-5h30l14 20v21z" fill="#24467F" />
      <path d="M45 76h9l10 16v6H45z" fill="#0E1C2F" />
      <rect x="14" y="80" width="16" height="12" rx="2" fill="#9db6dd" />
      <rect x="8" y="104" width="52" height="4" fill="#EA580C" />
    </g>
  );
}

function Wheel({ cx }: { cx: number }): JSX.Element {
  return (
    <g>
      <circle cx={cx} cy="116" r="9" fill="#0b1220" />
      <circle cx={cx} cy="116" r="4" fill="#F59E0B" />
    </g>
  );
}

const SCENES: Record<string, JSX.Element> = {
  'flatbed-steel': (
    <g>
      <rect x="60" y="104" width="300" height="8" fill="#334155" />
      {/* stacked I-beams */}
      <g transform="translate(120 78)">
        <rect x="0" y="0" width="150" height="26" fill="#64748b" />
        <rect x="0" y="0" width="150" height="6" fill="#94a3b8" />
        <rect x="0" y="20" width="150" height="6" fill="#475569" />
        <rect x="0" y="-14" width="150" height="12" fill="#475569" />
        <rect x="68" y="-14" width="14" height="12" fill="#64748b" />
      </g>
      {/* orange straps */}
      <g stroke="#EA580C" strokeWidth="3">
        <line x1="150" y1="60" x2="150" y2="106" />
        <line x1="235" y1="60" x2="235" y2="106" />
      </g>
      <Cab />
      <Wheel cx={46} />
      <Wheel cx={250} />
      <Wheel cx={286} />
    </g>
  ),
  'step-deck': (
    <g>
      {/* upper deck then drop to lower main deck */}
      <path d="M60 96h70v-8h4v8h222v10H60z" fill="#334155" />
      {/* over-height crates on the low deck */}
      <g transform="translate(150 44)">
        <rect x="0" y="0" width="80" height="52" fill="#b98a4b" />
        <rect x="0" y="0" width="80" height="52" fill="none" stroke="#7c5a2e" strokeWidth="3" />
        <path d="M0 0l80 52M80 0L0 52" stroke="#7c5a2e" strokeWidth="2" />
      </g>
      <g transform="translate(240 58)">
        <rect x="0" y="0" width="60" height="38" fill="#a8b0bd" />
        <rect x="0" y="0" width="60" height="38" fill="none" stroke="#5b6472" strokeWidth="3" />
      </g>
      <g stroke="#EA580C" strokeWidth="3">
        <line x1="190" y1="30" x2="190" y2="98" />
        <line x1="270" y1="50" x2="270" y2="98" />
      </g>
      <Cab />
      <Wheel cx={46} />
      <Wheel cx={250} />
      <Wheel cx={286} />
      <Wheel cx={322} />
    </g>
  ),
  'rgn-lowboy': (
    <g>
      {/* gooseneck + low well + rear axle deck */}
      <path d="M60 96l14-22h10v22h190v-14h60v14h6v10H60z" fill="#334155" />
      {/* excavator */}
      <g transform="translate(120 46)">
        <rect x="6" y="44" width="96" height="12" rx="6" fill="#1f2937" />
        <circle cx="14" cy="50" r="5" fill="#334155" />
        <circle cx="94" cy="50" r="5" fill="#334155" />
        <rect x="30" y="18" width="42" height="28" rx="3" fill="#F59E0B" />
        <rect x="36" y="23" width="18" height="12" fill="#0E1C2F" />
        <path d="M60 30l34-16 8 6-30 18z" fill="#F59E0B" stroke="#b45309" strokeWidth="2" />
        <path d="M95 16l12 4-2 12-12-4z" fill="#b45309" />
        <path d="M104 12l14-6 3 5-13 7z" fill="#EA580C" />
      </g>
      <Cab />
      <Wheel cx={46} />
      <Wheel cx={250} />
      <Wheel cx={278} />
      <Wheel cx={322} />
      <Wheel cx={350} />
    </g>
  ),
  'auto-transport': (
    <g>
      {/* two-level open frame */}
      <g stroke="#475569" strokeWidth="6" fill="none">
        <path d="M64 108h296" />
        <path d="M70 74h290" />
        <path d="M74 108V74M356 108V74M215 108V74" />
        <path d="M74 108l40-34M215 108l40-34" />
      </g>
      {/* vehicles */}
      <g transform="translate(96 78)">
        <path d="M0 22V12l10-10h34l14 10v10z" fill="#9db6dd" />
        <rect x="8" y="5" width="18" height="9" rx="2" fill="#0E1C2F" />
        <circle cx="12" cy="24" r="5" fill="#0b1220" />
        <circle cx="46" cy="24" r="5" fill="#0b1220" />
      </g>
      <g transform="translate(238 44)">
        <path d="M0 22V10l8-8h30l16 8v12z" fill="#cbd5e1" />
        <rect x="6" y="5" width="16" height="8" rx="2" fill="#0E1C2F" />
        <circle cx="12" cy="24" r="5" fill="#0b1220" />
        <circle cx="44" cy="24" r="5" fill="#0b1220" />
      </g>
      <rect x="64" y="104" width="296" height="6" fill="#EA580C" opacity="0.7" />
      <Cab />
      <Wheel cx={46} />
      <Wheel cx={250} />
      <Wheel cx={286} />
    </g>
  ),
};
