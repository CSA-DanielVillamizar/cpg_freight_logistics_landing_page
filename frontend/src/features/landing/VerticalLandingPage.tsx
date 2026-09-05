import { Navigate, useParams } from 'react-router-dom';
import { Card } from '@/shared/ui';
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
        className="bg-primary-container bg-cover bg-center text-white"
        style={{ backgroundImage: `linear-gradient(100deg, rgba(11,25,44,0.94) 35%, rgba(11,25,44,0.6) 100%), url(${heroPhoto})` }}
      >
        <div className="mx-auto flex max-w-container flex-col gap-5 px-4 py-14">
          <VerticalIcon slug={content.slug} className="h-10 w-10 text-hazard-orange" />
          <span className="font-mono text-label-sm uppercase tracking-wider text-hazard-orange">
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
                className="rounded border border-white/20 px-3 py-1 font-mono text-label-sm uppercase tracking-wide text-white/80"
              >
                {badge}
              </span>
            ))}
          </div>
          <a
            href="#request-quote"
            className="mt-2 inline-flex h-12 w-fit items-center rounded bg-hazard-orange px-6 font-heading text-label-md uppercase tracking-wide text-white"
          >
            Request a quote
          </a>
        </div>
      </section>

      {/* Metrics ribbon */}
      <section className="border-b border-outline bg-surface-card">
        <dl className="mx-auto grid max-w-container grid-cols-2 gap-px overflow-hidden px-4 py-6 sm:grid-cols-4">
          {content.metrics.map((metric) => (
            <div key={metric.label} className="flex flex-col px-2">
              <dt className="font-mono text-headline-sm text-fleet-blue">{metric.value}</dt>
              <dd className="font-mono text-label-sm uppercase text-steel-gray">{metric.label}</dd>
            </div>
          ))}
        </dl>
      </section>

      {/* Service catalog */}
      <section className="mx-auto flex w-full max-w-container flex-col gap-6 px-4 py-12">
        <h2 className="text-headline-md">Equipment &amp; service catalog</h2>
        <div className="grid gap-4 md:grid-cols-2">
          {content.serviceCards.map((card) => (
            <Card key={card.title} anchored className="flex flex-col gap-2 p-5">
              <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
                {card.tag}
              </span>
              <h3 className="text-headline-sm">{card.title}</h3>
              <p className="text-body-sm text-steel-gray">{card.detail}</p>
              <p className="mt-1 font-mono text-label-sm text-fleet-blue">{card.spec}</p>
            </Card>
          ))}
        </div>
      </section>

      {/* Proof points + quote form */}
      <section id="request-quote" className="bg-surface-muted">
        <div className="mx-auto grid max-w-container gap-8 px-4 py-14 md:grid-cols-[1fr_1fr]">
          <div className="flex flex-col gap-4">
            <h2 className="text-headline-md">Engineered for enterprise contractors</h2>
            {content.proofPoints.map((point) => (
              <div key={point.title} className="flex flex-col gap-1">
                <h3 className="font-heading text-label-md uppercase tracking-wide text-on-surface">
                  {point.title}
                </h3>
                <p className="text-body-sm text-steel-gray">{point.body}</p>
              </div>
            ))}
          </div>

          <Card anchored className="flex flex-col gap-4 p-6">
            <h2 className="text-headline-sm">{content.formHeading}</h2>
            <LeadCaptureForm
              verticalSlug={content.slug}
              serviceType={content.serviceType}
              cargoPlaceholder={content.defaultCargoPlaceholder}
            />
          </Card>
        </div>
      </section>

      {/* Testimonial */}
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
              <span className="font-mono text-label-md text-on-surface">
                {content.testimonial.author}
              </span>
              <span className="font-mono text-label-sm text-steel-gray">
                {content.testimonial.role}
              </span>
            </div>
          </div>
        </Card>
      </section>
    </div>
  );
}
