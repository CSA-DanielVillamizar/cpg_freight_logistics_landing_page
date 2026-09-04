import type { ServiceType } from '@/shared/api/types';

export interface ServiceLine {
  value: ServiceType;
  label: string;
  blurb: string;
  requiresTemperature: boolean;
}

const COLD_CHAIN: ServiceLine = {
  value: 'ColdChain',
  label: 'Cold Chain / Reefer',
  blurb: 'Continuous temperature logging, dual reefer units',
  requiresTemperature: true,
};

export const SERVICE_LINES: readonly ServiceLine[] = [
  COLD_CHAIN,
  {
    value: 'HeavyHaul',
    label: 'Heavy Haul / Superload',
    blurb: 'RGN multi-axle, escorts, corridor permits',
    requiresTemperature: false,
  },
  {
    value: 'Flatbed',
    label: 'Flatbed / Step-Deck',
    blurb: 'Structural steel, precast, bulk fabrication',
    requiresTemperature: false,
  },
  {
    value: 'FdotConcrete',
    label: 'FDOT Concrete Barricades',
    blurb: 'Jersey barriers, crane staging, MOT-compliant',
    requiresTemperature: false,
  },
];

export function getServiceLine(value: ServiceType): ServiceLine {
  return SERVICE_LINES.find((line) => line.value === value) ?? COLD_CHAIN;
}
