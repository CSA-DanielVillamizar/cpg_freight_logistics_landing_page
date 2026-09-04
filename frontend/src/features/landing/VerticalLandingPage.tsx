import { useParams, Navigate } from 'react-router-dom';
import { Card } from '@/shared/ui';
import { LeadCaptureForm } from '@/features/leads/LeadCaptureForm';
import { TrustRibbon } from './components/TrustRibbon';
import { VERTICALS } from './verticals';

export function VerticalLandingPage(): JSX.Element {
  const { slug } = useParams<{ slug: string }>();
  const vertical = VERTICALS.find((entry) => entry.slug === slug);

  if (!vertical) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="mx-auto flex max-w-container flex-col gap-10 px-4 py-10">
      <section className="flex flex-col gap-3 rounded-lg bg-primary-container p-8 text-white">
        <span className="font-mono text-label-sm uppercase tracking-wider text-hazard-orange">
          {vertical.serviceType}
        </span>
        <h1 className="text-headline-xl text-white">{vertical.name}</h1>
        <p className="max-w-2xl text-body-md text-white/80">{vertical.headline}</p>
      </section>

      <TrustRibbon />

      <section className="grid gap-8 md:grid-cols-[1.2fr_1fr]">
        <div className="flex flex-col gap-3 text-body-md text-steel-gray">
          <h2 className="text-headline-md">Why contractors choose CPG</h2>
          <p>
            Pre-cleared state route permits, in-house escort coordination and dedicated multi-axle
            equipment — one contract from production to placement.
          </p>
          <p>
            DBE-certified for state and federal participation goals, FMCSA satisfactory safety rating,
            and full chain-of-custody documentation on every move.
          </p>
        </div>
        <Card anchored className="flex flex-col gap-4 p-6">
          <h2 className="text-headline-sm">Request an enterprise quote</h2>
          <LeadCaptureForm verticalSlug={vertical.slug} serviceType={vertical.serviceType} />
        </Card>
      </section>
    </div>
  );
}
