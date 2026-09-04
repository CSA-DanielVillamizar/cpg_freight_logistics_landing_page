import { Link } from 'react-router-dom';
import { Badge, Card } from '@/shared/ui';
import { TrustRibbon } from './components/TrustRibbon';
import { VERTICAL_CONTENT } from './verticalContent';

export function LandingPage(): JSX.Element {
  return (
    <div className="mx-auto flex max-w-container flex-col gap-12 px-4 py-10">
      <section className="flex flex-col gap-4 rounded-lg bg-primary-container p-8 text-white">
        <Badge tone="oversize">Live Dispatch Active • Tier 1 Fleet Ready</Badge>
        <h1 className="text-headline-xl text-white sm:text-display-lg">
          Heavy Haul &amp; Flatbed Transportation Across All 48 States
        </h1>
        <p className="max-w-2xl text-body-md text-white/80">
          Concrete precast, structural steel and specialized freight, delivered with 35+ years of
          certified heavy-haul engineering and safety-first field execution.
        </p>
        <div className="flex flex-wrap gap-3 pt-2">
          <Link
            to="/rates"
            className="inline-flex h-12 items-center rounded bg-hazard-orange px-5 font-heading text-label-md uppercase tracking-wide text-white"
          >
            Open the Rate Calculator
          </Link>
        </div>
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="text-headline-md">Proven Industrial Credibility</h2>
        <TrustRibbon />
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="text-headline-md">Specialized Freight Verticals</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          {VERTICAL_CONTENT.map((vertical) => (
            <Card key={vertical.slug} anchored className="flex flex-col gap-2 p-5">
              <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
                {vertical.serviceType}
              </span>
              <h3 className="text-headline-sm">{vertical.name}</h3>
              <p className="text-body-sm text-steel-gray">{vertical.subhead}</p>
              <Link
                to={`/verticals/${vertical.slug}`}
                className="mt-2 inline-flex items-center gap-1 font-mono text-label-md text-hazard-orange"
              >
                Request a quote →
              </Link>
            </Card>
          ))}
        </div>
      </section>
    </div>
  );
}
