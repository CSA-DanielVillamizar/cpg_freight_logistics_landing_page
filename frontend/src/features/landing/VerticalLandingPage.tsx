import { Navigate, useParams } from 'react-router-dom';
import { Card, Reveal } from '@/shared/ui';
import { LeadCaptureForm } from '@/features/leads/LeadCaptureForm';
import { VerticalIcon } from './components/VerticalIcon';
import { getVerticalContent } from './verticalContent';
import heroPhoto from '@/assets/hero-heavy-haul.jpg';

export function VerticalLandingPage(): JSX.Element {
  const { slug } = useParams<{ slug: string }>();
  const content = getVerticalContent(slug);

  if (!content) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="flex flex-col">
      {/* Hero */}
      <section
        className="relative isolate overflow-hidden bg-primary-container bg-cover bg-center text-white"
        style={{ backgroundImage: `linear-gradient(100deg, rgba(11,25,44,0.94) 35%, rgba(11,25,44,0.6) 100%), url(${heroPhoto})` }}
      >
        <div
          aria-hidden
          className="pointer-events-none absolute inset-0 -z-10 opacity-[0.15] [background-image:radial-gradient(rgba(255,255,255,0.9)_1px,transparent_1px)] [background-size:22px_22px]"
        />
        <div
          aria-hidden
          className="pointer-events-none absolute -right-24 -top-28 -z-10 h-80 w-80 rounded-full bg-hazard-orange/15 blur-3xl"
        />
        <div className="mx-auto flex max-w-container flex-col gap-5 px-4 py-14">
          <VerticalIcon slug={content.slug} className="h-10 w-10 text-fleet-blue" />
          <span className="text-xs font-semibold uppercase tracking-wider text-white/70">
            {content.eyebrow}
          </span>
          <h1 className="max-w-3xl text-headline-xl text-white sm:text-display-lg">
            {content.headline}
          </h1>
          <p className="max-w-2xl text-body-md text-white/80">{content.subhead}</p>
          <div className="flex flex-wrap gap-2 pt-1">
            {content.badges.map((badge) => (
              <span
                key={badge}
                className="rounded-full border border-white/20 px-3 py-1 text-[11px] font-semibold uppercase tracking-wider text-white/80"
              >
                {badge}
              </span>
            ))}
          </div>
          <a
            href="#request-quote"
            className="group mt-2 inline-flex h-12 w-fit items-center gap-2 rounded bg-fleet-blue px-6 text-xs font-semibold uppercase tracking-wider text-white shadow-sm transition-all hover:-translate-y-0.5 hover:bg-fleet-blue-hover motion-reduce:transition-none motion-reduce:hover:translate-y-0"
          >
            Request a quote
            <span
              aria-hidden
              className="transition-transform duration-200 group-hover:translate-x-0.5 motion-reduce:transition-none"
            >
              &rarr;
            </span>
          </a>
        </div>
      </section>

      {/* Metrics ribbon */}
      <section className="border-b border-slate-200 bg-surface-card">
        <dl className="mx-auto grid max-w-container grid-cols-2 gap-px overflow-hidden px-4 py-6 sm:grid-cols-4">
          {content.metrics.map((metric) => (
            <div key={metric.label} className="flex flex-col-reverse px-2">
              <dt className="text-[11px] font-semibold uppercase tracking-wider text-steel-gray">
                {metric.label}
              </dt>
              <dd className="font-mono text-headline-sm tabular-nums text-fleet-blue">{metric.value}</dd>
            </div>
          ))}
        </dl>
      </section>

      {/* Service catalog */}
      <Reveal>
        <section className="mx-auto flex w-full max-w-container flex-col gap-6 px-4 py-12">
          <h2 className="text-headline-md">Equipment &amp; service catalog</h2>
          <div className="grid gap-4 md:grid-cols-2">
            {content.serviceCards.map((card, index) => (
              <Reveal key={card.title} delayMs={index * 70}>
                <Card interactive className="flex h-full flex-col gap-2 p-5">
                  <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    {card.tag}
                  </span>
                  <h3 className="text-headline-sm">{card.title}</h3>
                  <p className="flex-1 text-body-sm text-steel-gray">{card.detail}</p>
                  <dl className="mt-2 flex flex-col divide-y divide-slate-100 border-t border-slate-200 text-body-sm">
                    {card.specs.map((row) => (
                      <div key={row.label} className="flex items-baseline justify-between gap-4 py-1.5">
                        <dt className="shrink-0 text-xs font-semibold uppercase tracking-wider text-steel-gray">
                          {row.label}
                        </dt>
                        <dd className="text-right font-medium text-on-surface">{row.value}</dd>
                      </div>
                    ))}
                  </dl>
                </Card>
              </Reveal>
            ))}
          </div>
        </section>
      </Reveal>

      {/* Proof points + quote form */}
      <section id="request-quote" className="bg-surface-muted">
        <div className="mx-auto grid max-w-container gap-8 px-4 py-14 md:grid-cols-[1fr_1fr]">
          <div className="flex flex-col gap-4">
            <h2 className="text-headline-md">Engineered for enterprise contractors</h2>
            <dl className="grid grid-cols-3 gap-px overflow-hidden rounded-lg bg-fleet-blue/20">
              {content.metrics.slice(0, 3).map((metric) => (
                <div
                  key={metric.label}
                  className="flex flex-col-reverse gap-1 bg-primary-container p-4"
                >
                  <dt className="text-[11px] font-semibold uppercase tracking-wider text-white/70">
                    {metric.label}
                  </dt>
                  <dd className="font-mono text-headline-sm tabular-nums text-safety-amber">
                    {metric.value}
                  </dd>
                </div>
              ))}
            </dl>
            {content.proofPoints.map((point) => (
              <div key={point.title} className="flex flex-col gap-1">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-on-surface">
                  {point.title}
                </h3>
                <p className="text-body-sm text-steel-gray">{point.body}</p>
              </div>
            ))}
          </div>

          <Card raised className="flex flex-col gap-4 p-6">
            <h2 className="text-headline-sm">{content.formHeading}</h2>
            <ol className="flex flex-col gap-2 border-b border-slate-200 pb-4 sm:flex-row sm:gap-4">
              {['Cargo & equipment', 'Lane & dimensions', 'Guaranteed rate'].map((step, index) => (
                <li key={step} className="flex items-center gap-2">
                  <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-fleet-blue font-mono text-[11px] font-semibold tabular-nums text-white">
                    {index + 1}
                  </span>
                  <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    {step}
                  </span>
                </li>
              ))}
            </ol>
            <LeadCaptureForm
              verticalSlug={content.slug}
              serviceType={content.serviceType}
              cargoPlaceholder={content.defaultCargoPlaceholder}
            />
          </Card>
        </div>
      </section>

      {/* Testimonial */}
      <Reveal>
      <section className="mx-auto w-full max-w-container px-4 py-12">
        <Card className="flex flex-col gap-3 p-8">
          <div className="flex gap-1 text-safety-amber" aria-hidden>
            {'★★★★★'}
          </div>
          <blockquote className="text-body-lg text-on-surface-variant">
            &ldquo;{content.testimonial.quote}&rdquo;
          </blockquote>
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-fleet-blue font-heading text-label-md text-white">
              {content.testimonial.author
                .split(' ')
                .map((part) => part.charAt(0))
                .join('')}
            </div>
            <div className="flex flex-col">
              <span className="text-sm font-semibold text-on-surface">
                {content.testimonial.author}
              </span>
              <span className="text-body-sm text-steel-gray">{content.testimonial.role}</span>
            </div>
          </div>
        </Card>
      </section>
      </Reveal>
    </div>
  );
}
