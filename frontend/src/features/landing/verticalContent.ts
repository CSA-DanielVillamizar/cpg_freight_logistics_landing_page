import type { ServiceType } from '@/shared/api/types';

export interface Metric {
  value: string;
  label: string;
}

export interface SpecRow {
  label: string;
  value: string;
}

export interface ServiceCard {
  tag: string;
  title: string;
  detail: string;
  specs: readonly SpecRow[];
}

export interface VerticalContent {
  slug: string;
  name: string;
  navLabel: string;
  serviceType: ServiceType;
  eyebrow: string;
  headline: string;
  subhead: string;
  badges: string[];
  metrics: Metric[];
  serviceCards: ServiceCard[];
  proofPoints: { title: string; body: string }[];
  testimonial: { quote: string; author: string; role: string };
  formHeading: string;
  defaultCargoPlaceholder: string;
}

export const VERTICAL_CONTENT: readonly VerticalContent[] = [
  {
    slug: 'fdot-concrete-barricades',
    name: 'FDOT Concrete Barricades & Crane Staging',
    navLabel: 'FDOT Concrete',
    serviceType: 'FdotConcrete',
    eyebrow: 'FDOT-approved source · MASH TL-3 compliant',
    headline: 'FDOT Concrete Jersey Barricades, Staging & Turnkey Crane Placement',
    subhead:
      'Full-service logistics delivery, certified manufacturing supply and precision hydraulic boom crane placement of FDOT-certified precast concrete barriers for road widening, highway infrastructure, airport civil works and perimeter security across Florida.',
    badges: ['FDOT Index 102-100 Certified', 'Hydraulic Boom Cranes', 'Emergency MOT Shift'],
    metrics: [
      { value: '5,000+ LF', label: 'Linear feet always ready' },
      { value: '12 f-4', label: 'K-rail turnpike vetted units' },
      { value: '± 0.5 in', label: 'FDOT placement tolerance' },
      { value: '90 min', label: 'Storm mobilization' },
    ],
    serviceCards: [
      {
        tag: 'FDOT Index 102-100',
        title: 'Type K-Rail & F-Shape Barriers',
        detail:
          '10-ft & 12-ft precast reinforced Jersey barriers with integrated heavy-gauge steel pin-and-loop stock connections.',
        specs: [
          { label: 'Unit weight', value: '4,000 lb (12 ft) · 4,800 lb (10 ft)' },
          { label: 'Connection', value: 'Steel pin-and-loop' },
          { label: 'Standard', value: 'FDOT Index 102-100' },
        ],
      },
      {
        tag: 'Self-offloading trucks',
        title: 'Hydraulic Boom Placement',
        detail:
          'Direct boom positioning over curbs, guardrails and trenches — eliminates the second mobile crane lease on civil jobs.',
        specs: [
          { label: 'Boom reach', value: 'Up to 40 ft' },
          { label: 'Offload', value: 'Self-erecting, no crane lease' },
          { label: 'Crew', value: 'Certified riggers on board' },
        ],
      },
      {
        tag: 'FAA & city compliance',
        title: 'Low-Profile Airport & Urban',
        detail:
          'Designed for tarmac perimeter work, airport taxiway realignments and pedestrian-density civil construction.',
        specs: [
          { label: 'Profiles', value: '10 in · 18 in F-shape' },
          { label: 'Use case', value: 'Tarmac & pedestrian zones' },
          { label: 'Compliance', value: 'FAA & municipal' },
        ],
      },
      {
        tag: 'Crash rated TL-3',
        title: 'Attenuators & End Cushions',
        detail:
          'Trailer-mounted attenuators (TMA) and stationary energy-absorbing end treatments with certified transition hardware.',
        specs: [
          { label: 'Crash rating', value: 'MASH TL-3' },
          { label: 'Validation', value: '8 mph impact' },
          { label: 'Types', value: 'Trailer-mounted & stationary' },
        ],
      },
    ],
    proofPoints: [
      {
        title: 'One contract, production to placement',
        body: 'Barrier production, freight transport and precision placement under one turnkey civil-highway contract.',
      },
      {
        title: 'DBE certified for state & federal bids',
        body: 'Disadvantaged Business Enterprise certified — barricade staging satisfies state and federal participation quotas on prime civil bids.',
      },
      {
        title: 'High-volume night shifts & MOT',
        body: 'Vetted for Florida Turnpike and I-4 Beyond the Ultimate; lane closures where speed is critical to meet the 5:00 AM reopen penalty.',
      },
    ],
    testimonial: {
      quote:
        'CPG moved and placed 12-ft K-rail barriers directly into traffic separation zones under live nighttime MOT escort. Not a minute of crane downtime on our jobsite.',
      author: 'Marcus Sterling',
      role: 'VP of Operations · Florida Infrastructure Corp',
    },
    formHeading: 'Request a barricade staging quote',
    defaultCargoPlaceholder: 'Linear footage, barrier profile, shift timing, job-site county',
  },
  {
    slug: 'refrigerated-cold-chain',
    name: 'Refrigerated & Cold Chain Freight',
    navLabel: 'Cold Chain',
    serviceType: 'ColdChain',
    eyebrow: 'Tier-1 temperature-controlled logistics · FSMA compliant',
    headline: 'Continuous Climate-Controlled & High-Value Cold Chain Freight',
    subhead:
      'Engineered reefer transportation maintaining precise thermal stability (-20°C to 21°C) for pharmaceuticals, fresh citrus, frozen proteins and perishable food products across Florida agricultural corridors and nationwide lanes.',
    badges: ['FSMA Compliant', 'Dual Redundant Reefer', 'Digital Temp Logging'],
    metrics: [
      { value: '±1.5°F', label: 'Thermal band held' },
      { value: '5 min', label: 'Telemetry broadcast interval' },
      { value: '2 units', label: 'Redundant reefer per trailer' },
      { value: '24/7', label: 'Rescue reefer standby' },
    ],
    serviceCards: [
      {
        tag: '41,000 lb max',
        title: "53' Dual-Temp High-Cube Reefer",
        detail:
          'Thermo King Precedent and Carrier Vector hybrid refrigeration units, moveable insulated bulkheads and dual evaporation systems for segregated frozen/fresh transit.',
        specs: [
          { label: 'Temp range', value: '-10°F to 65°F' },
          { label: 'Zones', value: 'Dual, movable bulkhead' },
          { label: 'Units', value: 'Thermo King / Carrier hybrid' },
        ],
      },
      {
        tag: 'Sub-zero specialty',
        title: 'Deep-Freeze Flash Trailers',
        detail:
          'High-output cryogenic evaporator fans designed to hold steady sub-zero temperatures for ice cream, biologics and frozen seafood.',
        specs: [
          { label: 'Set point', value: '-20°F steady pull-down' },
          { label: 'Airflow', value: 'High-output cryo evaporators' },
          { label: 'Cargo', value: 'Ice cream · biologics · seafood' },
        ],
      },
      {
        tag: 'Agricultural fresh',
        title: 'Chilled Citrus, Produce & Floral',
        detail:
          'Optimized for Florida growers with high-velocity airflow chutes, humidity management and micro-climate controls that prevent cellular freezing.',
        specs: [
          { label: 'Air circulation', value: '3,200 CFM continuous' },
          { label: 'Controls', value: 'Humidity & micro-climate' },
          { label: 'Corridors', value: 'Florida grower lanes' },
        ],
      },
      {
        tag: 'Security & bio-pharma',
        title: 'Life Science Transporters',
        detail:
          'Precision thermal containment with geofenced deadbolts, door-break-in sensors, remote satellite re-arming and automatic temperature-breach dispatch.',
        specs: [
          { label: 'Standards', value: 'GDP · 21 CFR Part 11' },
          { label: 'Security', value: 'Geofenced deadbolts, breach alerts' },
          { label: 'Dispatch', value: 'Auto temp-breach escalation' },
        ],
      },
    ],
    proofPoints: [
      {
        title: 'In-transit telematics',
        body: 'Live client-portal integration broadcasting temperature curves, fuel level and compressor cycle timestamps every 5 minutes.',
      },
      {
        title: 'Pre-cooled & sanitized',
        body: 'Certified washout facilities issuing formal food-grade sanitation slips; rigorous 2-hour continuous pre-cooling before loading.',
      },
      {
        title: 'Orlando crossdock hub',
        body: 'Central Florida re-icing, pallet temperature adjustment, load redistribution and reefer relay center at our terminal.',
      },
    ],
    testimonial: {
      quote:
        'Continuous digital temperature logs and guaranteed delivery windows across key agricultural lanes. Their reefer relay center saved a full citrus harvest for us.',
      author: 'Elena Rodriguez',
      role: 'Director of Distribution · Sunbelt Produce Cooperative',
    },
    formHeading: 'Request a reefer capacity quote',
    defaultCargoPlaceholder: 'Commodity, set-point temperature, weight, pickup & delivery windows',
  },
  {
    slug: 'flatbed-heavy-haul',
    name: 'Heavy Haul & Flatbed Freight',
    navLabel: 'Heavy Haul',
    serviceType: 'HeavyHaul',
    eyebrow: 'Rapid dispatch quote system · 100% safety rating',
    headline: 'Heavy Haul & Flatbed Transportation Across All 48 States',
    subhead:
      'Delivering concrete precast, structural steel and sophisticated industrial freight with over 35 years of certified heavy-haul engineering and flawless critical-transit records.',
    badges: ['#1 DBE Certified', '35+ Years Proven', '11 Dedicated Units', '48 States Covered'],
    metrics: [
      { value: '120K LBS', label: 'Max payload' },
      { value: '12 MIN', label: 'Avg dispatch response' },
      { value: 'ZERO', label: 'Escort-caused delays' },
      { value: 'TOP 5%', label: 'FMCSA safety rating' },
    ],
    serviceCards: [
      {
        tag: 'High capacity',
        title: 'Flatbed & Structural Steel',
        detail:
          'Precision tie-down arrays for bridge beams, heavy pipe, structural rebar cages and bulk industrial fabrication.',
        specs: [
          { label: 'Max payload', value: '48,000 lb' },
          { label: 'Securement', value: '1/2 in Grade 100 chains' },
          { label: 'Cargo', value: 'Beams · pipe · rebar cages' },
        ],
      },
      {
        tag: 'Taller clearances',
        title: 'Step-Deck / Drop-Deck',
        detail:
          'Lowered deck height for over-height machinery, tanks and pre-assembled modules that exceed standard flatbed clearance.',
        specs: [
          { label: 'Deck height', value: '42 in' },
          { label: 'Legal cargo height', value: '10 ft 2 in' },
          { label: 'Max payload', value: '45,000 lb' },
        ],
      },
      {
        tag: '120,000+ lbs',
        title: 'RGN Multi-Axle / Superload',
        detail:
          'Removable gooseneck multi-axle configurations for extreme-height industrial turbines, pre-stressed bridge segments and high-center-of-gravity civil assets.',
        specs: [
          { label: 'Max payload', value: '120,000+ lb' },
          { label: 'Loading', value: 'Ground-level drive-on' },
          { label: 'Support', value: 'Escort & pole cars, permits pre-cleared' },
        ],
      },
      {
        tag: 'Specialty haul',
        title: 'Commercial Fleet & Auto Transport',
        detail:
          'Enclosed and hydraulic multi-level transport for municipal trucks, utility rolling stock and commercial fleets.',
        specs: [
          { label: 'Loading', value: 'Soft-tie pneumatic ramps' },
          { label: 'Coverage', value: 'Insured to $1M' },
          { label: 'Cargo', value: 'Municipal trucks · utility rolling stock' },
        ],
      },
    ],
    proofPoints: [
      {
        title: 'Pre-cleared state route permits',
        body: 'CPG operations secures all superload permits, bridge analyses and highway-patrol escorts before you pull onto the asphalt.',
      },
      {
        title: 'Dedicated freight coordinators',
        body: 'Direct line to seasoned dispatchers who know axle weights, bridge formulas and diesel mechanics — not automated phone trees.',
      },
      {
        title: 'Multi-axle lowboys & step-decks',
        body: '11 dedicated specialized units: lowboys, step-decks, RGN and heavy multi-axle rigs on hand at our Orlando yard.',
      },
    ],
    testimonial: {
      quote:
        'CPG moved thirty-six 90-foot concrete girders across Interstate 4 with precision escort coordination. Not a minute of crane downtime on our jobsite.',
      author: 'David Vance',
      role: 'Senior Logistics Director · Gulf Coast Marine & Heavy Civil',
    },
    formHeading: 'Get your guaranteed heavy-haul quote',
    defaultCargoPlaceholder: 'Dimensions, weight, over-dimensional flags, origin & destination',
  },
  {
    slug: 'mobile-rate-calculator',
    name: 'Mobile Freight & Rate Calculator',
    navLabel: 'Rate Calculator',
    serviceType: 'Flatbed',
    eyebrow: 'Instant estimator · under 500 ms',
    headline: 'Mobile Freight Quoting & Interactive Rate Calculator',
    subhead:
      'Select equipment class and lane dimensions for rapid toll, fuel and escort calculations — then hand off directly to a dispatcher for guaranteed capacity.',
    badges: ['Instant Estimate', 'Zero Obligation', 'DOT Verified'],
    metrics: [
      { value: '< 500 ms', label: 'Quote engine' },
      { value: '12 MIN', label: 'Avg dispatch' },
      { value: '284 mi', label: 'Sample lane' },
      { value: '3 steps', label: 'Cargo · route · dispatch' },
    ],
    serviceCards: [
      {
        tag: 'Step 1',
        title: 'Cargo & Class',
        detail: 'Pick the trailer / bed specification and enter estimated gross freight weight.',
        specs: [
          { label: 'Equipment', value: 'Flatbed · step-deck · RGN' },
          { label: 'Input', value: 'Estimated gross weight' },
        ],
      },
      {
        tag: 'Step 2',
        title: 'Route Specs',
        detail: 'Origin and destination ZIP or city, plus any specialized haul adders (tarping, permits, crane offload).',
        specs: [
          { label: 'Lane', value: 'Origin & destination ZIP' },
          { label: 'Adders', value: 'Tarping · permit · crane offload' },
        ],
      },
      {
        tag: 'Step 3',
        title: 'Instant Dispatch',
        detail: 'Receive an all-inclusive rate band with fuel surcharge and over-dimension escort baked in.',
        specs: [
          { label: 'Output', value: 'All-in rate band' },
          { label: 'Includes', value: 'Fuel surcharge & escort' },
        ],
      },
      {
        tag: 'Live desk',
        title: 'Direct Dispatch Routing',
        detail: 'Every quote routes to a named Central Florida dispatcher for same-day heavy-haul or hotshot.',
        specs: [
          { label: 'Desk', value: 'Named Central Florida dispatcher' },
          { label: 'Yard', value: '2824 S. Orange Ave, Orlando' },
        ],
      },
    ],
    proofPoints: [
      {
        title: '100% free spec',
        body: 'The estimate is free and zero-obligation — built to give procurement a defensible budget number fast.',
      },
      {
        title: 'Precision surcharge model',
        body: 'Base rate, cold-chain surcharge and fuel surcharge broken out line by line, exactly as dispatch prices it.',
      },
      {
        title: 'DOT verified carrier',
        body: 'FMCSA satisfactory rating, DOT FL-ORL-982 · MC-749211, $1M cargo all-risk coverage.',
      },
    ],
    testimonial: {
      quote:
        'The rate calculator gave us a number in seconds and dispatch honored it exactly. First and only call for Florida transport.',
      author: 'Sarah Whitfield',
      role: 'Procurement Lead · Apex Construction',
    },
    formHeading: 'Send this lane to a dispatcher',
    defaultCargoPlaceholder: 'Equipment class, weight, origin & destination, timing',
  },
] as const;

export function getVerticalContent(slug: string | undefined): VerticalContent | null {
  return VERTICAL_CONTENT.find((entry) => entry.slug === slug) ?? null;
}

export function verticalNavLinks(): { slug: string; navLabel: string }[] {
  return VERTICAL_CONTENT.map(({ slug, navLabel }) => ({ slug, navLabel }));
}
