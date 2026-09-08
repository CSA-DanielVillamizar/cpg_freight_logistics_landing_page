import { Link } from 'react-router-dom';
import { FLEET_PROOF_POINTS, FLEET_SPECS } from '../landingContent';

export function FleetShowcase(): JSX.Element {
  return (
    <section className="overflow-hidden rounded-xl bg-primary-container px-6 py-12 text-white sm:px-10 sm:py-14">
      <div className="grid gap-10 lg:grid-cols-[1.05fr_0.95fr] lg:items-center">
        <div className="flex flex-col gap-6">
          <div className="flex flex-col gap-2">
            <span className="text-xs font-semibold uppercase tracking-wider text-safety-amber">
              Rigid operational integrity
            </span>
            <h2 className="text-headline-lg text-white">Engineered rigging &amp; heavy precast mastery</h2>
            <p className="max-w-xl text-body-sm leading-relaxed text-white/70">
              Bridge girders, concrete barricades and multi-ton industrial machinery — moved with
              structural load-securing and highway permits across every federal transit artery.
            </p>
          </div>

          <ul className="flex flex-col gap-3">
            {FLEET_PROOF_POINTS.map((point) => (
              <li key={point.title} className="flex gap-3">
                <span
                  className="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-hazard-orange"
                  aria-hidden
                />
                <span className="text-body-sm text-white/80">
                  <strong className="font-semibold text-white">{point.title}:</strong> {point.body}
                </span>
              </li>
            ))}
          </ul>

          <Link
            to="/rates"
            className="group inline-flex h-11 w-fit items-center gap-2 rounded bg-hazard-orange px-5 text-xs font-semibold uppercase tracking-wider text-white shadow-sm transition-colors hover:bg-hazard-orange/90"
          >
            Discuss your load spec
            <span
              aria-hidden
              className="transition-transform duration-200 group-hover:translate-x-0.5 motion-reduce:transition-none"
            >
              &rarr;
            </span>
          </Link>
        </div>

        <div className="rounded-lg border border-white/10 bg-white/[0.04] p-5">
          <div className="flex items-center justify-between border-b border-white/10 pb-3">
            <span className="text-xs font-semibold uppercase tracking-wider text-white/70">
              Trailer spec sheet
            </span>
            <span className="rounded-full bg-safety-amber/15 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wider text-safety-amber">
              Fleet · 11 units
            </span>
          </div>
          <dl className="divide-y divide-white/10">
            {FLEET_SPECS.map((spec) => (
              <div key={spec.label} className="flex items-center justify-between gap-4 py-2.5">
                <dt className="text-[12px] text-white/60">{spec.label}</dt>
                <dd
                  className={`text-right font-mono text-[12px] ${
                    spec.emphasised ? 'text-safety-amber' : 'text-white'
                  }`}
                >
                  {spec.value}
                </dd>
              </div>
            ))}
          </dl>
        </div>
      </div>
    </section>
  );
}
