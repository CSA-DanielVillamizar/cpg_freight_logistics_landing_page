import { Link } from 'react-router-dom';

const DISPATCH_TEL = 'tel:4075550194';

export function ClosingCta(): JSX.Element {
  return (
    <section className="flex flex-col gap-6 rounded-xl bg-hazard-orange px-6 py-10 text-white sm:px-10 md:flex-row md:items-center md:justify-between">
      <div className="flex flex-col gap-1.5">
        <span className="text-xs font-semibold uppercase tracking-widest text-white/80">
          Ready for dispatch
        </span>
        <h2 className="max-w-xl text-headline-md text-white">
          Lock in dedicated flatbed &amp; heavy-haul capacity.
        </h2>
        <p className="max-w-xl text-body-sm text-white/85">
          Our Orlando team routes, permits and dispatches your freight across all 48 continental
          states.
        </p>
      </div>
      <div className="flex shrink-0 flex-wrap gap-3">
        <Link
          to="/rates"
          className="inline-flex h-11 items-center rounded bg-primary-container px-5 text-xs font-semibold uppercase tracking-wider text-white shadow-sm transition-colors hover:bg-primary"
        >
          Get a fast rate
        </Link>
        <a
          href={DISPATCH_TEL}
          className="inline-flex h-11 items-center rounded border border-white/40 bg-white/10 px-5 text-xs font-semibold uppercase tracking-wider text-white transition-colors hover:bg-white/20"
        >
          Call dispatch
        </a>
      </div>
    </section>
  );
}
