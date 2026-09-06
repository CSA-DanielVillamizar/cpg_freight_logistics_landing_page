import { Link } from 'react-router-dom';
import { formatEnum } from '@/shared/lib/formatEnum';
import { Card } from '@/shared/ui';
import { TrustRibbon } from './components/TrustRibbon';
import { VerticalIcon } from './components/VerticalIcon';
import { VERTICAL_CONTENT } from './verticalContent';
import heroPhoto from '@/assets/hero-heavy-haul.jpg';

export function LandingPage(): JSX.Element {
  return (
    <div className="mx-auto flex max-w-container flex-col gap-12 px-4 py-10">
      <section
        className="relative flex flex-col gap-4 overflow-hidden rounded-xl bg-primary-container bg-cover bg-center p-8 text-white"
        style={{ backgroundImage: `linear-gradient(100deg, rgba(11,25,44,0.94) 30%, rgba(11,25,44,0.55) 100%), url(${heroPhoto})` }}
      >
        <span className="inline-flex w-fit items-center gap-2 rounded-full border border-white/25 bg-white/10 px-3 py-1 text-[11px] font-semibold uppercase tracking-wider text-white/90">
          Live Dispatch Active • Tier 1 Fleet Ready
        </span>
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
            className="inline-flex h-12 items-center rounded bg-fleet-blue px-5 text-xs font-semibold uppercase tracking-wider text-white shadow-sm transition-colors hover:bg-fleet-blue-hover"
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
            <Card key={vertical.slug} className="flex flex-col gap-2 p-5">
              <VerticalIcon slug={vertical.slug} className="h-8 w-8 text-fleet-blue" />
              <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                {formatEnum(vertical.serviceType)}
              </span>
              <h3 className="text-headline-sm">{vertical.name}</h3>
              <p className="text-body-sm text-steel-gray">{vertical.subhead}</p>
              <Link
                to={`/verticals/${vertical.slug}`}
                className="mt-2 inline-flex items-center gap-1 text-sm font-semibold text-fleet-blue hover:underline"
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
