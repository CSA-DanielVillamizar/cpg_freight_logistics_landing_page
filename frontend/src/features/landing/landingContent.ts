import { VERTICAL_CONTENT } from './verticalContent';

/** Homepage social proof — the strongest quote from each of the first three verticals. */
export const HOME_TESTIMONIALS = VERTICAL_CONTENT.slice(0, 3).map((vertical) => vertical.testimonial);

/** Fleet & equipment spec sheet (mirrors the approved landing prototype, section 5). */
export interface FleetSpec {
  label: string;
  value: string;
  emphasised?: boolean;
}

export const FLEET_SPECS: readonly FleetSpec[] = [
  { label: 'Max payload capacity', value: '85,000 lbs — standard flatbed' },
  { label: 'Deck length', value: "48'0\" & 53'0\" aluminum / steel" },
  { label: 'Lowboy well clearance', value: '18" to 24" road-to-deck' },
  { label: 'Safety winch rating', value: 'Grade 70 / 100 transport chains' },
  { label: 'Escort & pole cars', value: 'FL · GA · AL · SC · NC · TX', emphasised: true },
  { label: 'FMCSA DOT carrier', value: 'FL-9820-ORL · Satisfactory tier', emphasised: true },
];

export const FLEET_PROOF_POINTS: readonly { title: string; body: string }[] = [
  {
    title: 'Multi-axle lowboys & step-decks',
    body: 'Engineered configurations absorbing gross vehicle weights beyond 120,000 lbs.',
  },
  {
    title: 'Certified escort coordination',
    body: 'In-house route survey teams navigating multi-state over-width and height regulations.',
  },
  {
    title: 'Real-time satellite telemetry',
    body: 'Geo-tracking with load-weight monitoring and direct driver communication.',
  },
];
