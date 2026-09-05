interface VerticalIconProps {
  slug: string;
  className?: string;
}

export function VerticalIcon({ slug, className }: VerticalIconProps): JSX.Element {
  const common = {
    className,
    viewBox: '0 0 24 24',
    fill: 'none',
    stroke: 'currentColor',
    strokeWidth: 1.75,
    strokeLinecap: 'round' as const,
    strokeLinejoin: 'round' as const,
    'aria-hidden': true,
  };

  switch (slug) {
    case 'fdot-concrete-barricades':
      return (
        <svg {...common}>
          <path d="M3 20V9l4-3 4 3v11" />
          <path d="M13 20V9l4-3 4 3v11" />
          <path d="M3 14h6" />
          <path d="M13 14h6" />
          <path d="M2 20h20" />
        </svg>
      );
    case 'refrigerated-cold-chain':
      return (
        <svg {...common}>
          <path d="M12 2v20" />
          <path d="M5 5.5 19 18.5" />
          <path d="M19 5.5 5 18.5" />
          <path d="m9 4 3 2 3-2" />
          <path d="m9 20 3-2 3 2" />
          <path d="m4 8.5 1 3.5-3.5 1" />
          <path d="m20 15.5-1-3.5 3.5-1" />
          <path d="m4 15.5 1-3.5-3.5-1" />
          <path d="m20 8.5-1 3.5 3.5 1" />
        </svg>
      );
    case 'flatbed-heavy-haul':
      return (
        <svg {...common}>
          <path d="M2 16V8a1 1 0 0 1 1-1h9v9" />
          <path d="M12 11h6.5l2.5 3v2h-2" />
          <path d="M2 16h1" />
          <rect x="17" y="13" width="1" height="0" />
          <circle cx="7" cy="17.5" r="1.75" />
          <circle cx="17" cy="17.5" r="1.75" />
          <path d="M8.75 17.5h6.5" />
        </svg>
      );
    case 'standard-dry-van':
      return (
        <svg {...common}>
          <rect x="2" y="7" width="16" height="9" rx="1" />
          <path d="M18 10h2.5l1.5 2.5V16h-1" />
          <circle cx="6.5" cy="17.5" r="1.75" />
          <circle cx="17.5" cy="17.5" r="1.75" />
          <path d="M8.25 17.5h7.5" />
        </svg>
      );
    case 'mobile-rate-calculator':
      return (
        <svg {...common}>
          <rect x="5" y="2" width="14" height="20" rx="2" />
          <path d="M9 6h6" />
          <path d="M8 11h1" />
          <path d="M11.5 11h1" />
          <path d="M15 11h1" />
          <path d="M8 14.5h1" />
          <path d="M11.5 14.5h1" />
          <path d="M15 14.5h1" />
          <path d="M8 18h8" />
        </svg>
      );
    default:
      return (
        <svg {...common}>
          <circle cx="12" cy="12" r="9" />
        </svg>
      );
  }
}
