import type { ServiceType } from '@/shared/api/types';

export interface VerticalDefinition {
  slug: string;
  name: string;
  headline: string;
  serviceType: ServiceType;
}

/** Niche landing-page verticals for lead capture (SPEC.md US-04). */
export const VERTICALS: readonly VerticalDefinition[] = [
  {
    slug: 'fdot-concrete-barricades',
    name: 'FDOT Concrete Barricades & Crane Staging',
    headline: 'FDOT-certified concrete Jersey barricades, staged and crane-placed across Florida.',
    serviceType: 'FdotConcrete',
  },
  {
    slug: 'refrigerated-cold-chain',
    name: 'Refrigerated & Cold Chain Freight',
    headline: 'Continuous climate-controlled freight from -20°C pharma lanes to fresh produce corridors.',
    serviceType: 'ColdChain',
  },
  {
    slug: 'flatbed-heavy-haul',
    name: 'Flatbed & Heavy Haul Services',
    headline: 'Structural steel, precast beams and superload multi-axle transport across 48 states.',
    serviceType: 'HeavyHaul',
  },
  {
    slug: 'mobile-rate-calculator',
    name: 'Mobile Freight & Rate Calculator',
    headline: 'Instant toll, fuel and escort estimates for specialized freight lanes.',
    serviceType: 'Flatbed',
  },
] as const;
