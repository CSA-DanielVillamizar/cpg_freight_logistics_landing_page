import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { cn } from '@/shared/lib/cn';
import { formatEnum } from '@/shared/lib/formatEnum';
import { Card, Reveal } from '@/shared/ui';
import { ArrowLink } from './components/ArrowLink';
import { ClosingCta } from './components/ClosingCta';
import { FleetShowcase } from './components/FleetShowcase';
import { Testimonials } from './components/Testimonials';
import { TrustRibbon } from './components/TrustRibbon';
import { VerticalIcon } from './components/VerticalIcon';
import { VERTICAL_CONTENT } from './verticalContent';
import heroPhoto from '@/assets/hero-heavy-haul.jpg';

export function LandingPage(): JSX.Element {
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);

  return (
    <div className="mx-auto flex max-w-container flex-col gap-16 px-4 py-10">
      {/* Hero */}
      <section className="relative isolate flex flex-col gap-4 overflow-hidden rounded-xl bg-primary-container px-6 py-14 text-white sm:px-10 sm:py-20">
        <img
          src={heroPhoto}
          alt=""
          aria-hidden
          className={cn(
            'absolute inset-0 -z-10 h-full w-full object-cover object-center',
            'transition-transform duration-[1400ms] ease-out motion-reduce:transition-none',
            mounted ? 'scale-100' : 'scale-105',
          )}
        />
        <div className="absolute inset-0 -z-10 bg-gradient-to-r from-primary/95 via-primary/85 to-primary/45" />
        <div className="absolute inset-0 -z-10 bg-gradient-to-t from-primary via-primary/10 to-transparent" />
        <div
          className="pointer-events-none absolute -right-24 -top-28 -z-10 h-72 w-72 rounded-full bg-hazard-orange/10 blur-3xl"
          aria-hidden
        />

        <span className="inline-flex w-fit items-center gap-2 rounded-full border border-white/25 bg-white/10 px-3 py-1 text-[11px] font-semibold uppercase tracking-wider text-white/90 backdrop-blur-sm">
          <span className="h-1.5 w-1.5 rounded-full bg-safety-amber motion-safe:animate-pulse" aria-hidden />
          Live dispatch active • Tier 1 fleet ready
        </span>
        <h1 className="max-w-3xl text-headline-xl text-white sm:text-display-lg">
          Heavy Haul &amp; Flatbed Transportation Across All 48 States
        </h1>
        <p className="max-w-2xl text-body-md text-white/80">
          Concrete precast, structural steel and specialized freight, delivered with 35+ years of
          certified heavy-haul engineering and safety-first field execution.
        </p>
        <div className="flex flex-wrap items-center gap-3 pt-2">
          <Link
            to="/rates"
            className="group inline-flex h-12 items-center gap-2 rounded bg-hazard-orange px-5 text-xs font-semibold uppercase tracking-wider text-white shadow-sm transition-all hover:-translate-y-0.5 hover:bg-hazard-orange/90 motion-reduce:transition-none motion-reduce:hover:translate-y-0"
          >
            Request a freight quote
            <span
              aria-hidden
              className="transition-transform duration-200 group-hover:translate-x-0.5 motion-reduce:transition-none"
            >
              &rarr;
            </span>
          </Link>
          <a
            href="tel:4075550194"
            className="inline-flex h-12 items-center gap-2 rounded border border-white/25 bg-white/10 px-4 text-sm font-medium tabular-nums text-white transition-colors hover:bg-white/20"
          >
            <span className="material-symbols-outlined text-[18px] text-safety-amber" aria-hidden>
              call
            </span>
            (407) 555-0194
          </a>
        </div>
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 pt-2 text-[11px] font-semibold uppercase tracking-wider text-white/60">
          <span>Fast dispatch</span>
          <span aria-hidden>•</span>
          <span>Guaranteed capacity</span>
          <span aria-hidden>•</span>
          <span>DOT compliant · FL-ORL-982</span>
        </div>
      </section>

      {/* Proven Industrial Credibility */}
      <Reveal>
        <section className="flex flex-col gap-4">
          <h2 className="text-headline-md">Proven Industrial Credibility</h2>
          <TrustRibbon />
        </section>
      </Reveal>

      {/* Specialized Freight Verticals */}
      <Reveal>
        <section className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <span className="text-xs font-semibold uppercase tracking-wider text-hazard-orange">
              Engineered haulage capabilities
            </span>
            <h2 className="text-headline-md">Specialized Freight Solutions</h2>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            {VERTICAL_CONTENT.map((vertical, index) => (
              <Reveal key={vertical.slug} delayMs={index * 70}>
                <Card interactive className="flex h-full flex-col gap-2 p-5">
                  <VerticalIcon slug={vertical.slug} className="h-8 w-8 text-fleet-blue" />
                  <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    {formatEnum(vertical.serviceType)}
                  </span>
                  <h3 className="text-headline-sm">{vertical.name}</h3>
                  <p className="flex-1 text-body-sm text-steel-gray">{vertical.subhead}</p>
                  <ArrowLink to={`/verticals/${vertical.slug}`} className="mt-2">
                    Request a quote
                  </ArrowLink>
                </Card>
              </Reveal>
            ))}
          </div>
        </section>
      </Reveal>

      {/* Fleet & equipment */}
      <Reveal>
        <FleetShowcase />
      </Reveal>

      {/* Testimonials */}
      <Reveal>
        <Testimonials />
      </Reveal>

      {/* Closing CTA */}
      <Reveal>
        <ClosingCta />
      </Reveal>
    </div>
  );
}
