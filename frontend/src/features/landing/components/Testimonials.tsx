import { Card, Reveal } from '@/shared/ui';
import { HOME_TESTIMONIALS } from '../landingContent';

function initials(name: string): string {
  return name
    .split(' ')
    .map((part) => part.charAt(0))
    .join('')
    .slice(0, 2);
}

export function Testimonials(): JSX.Element {
  return (
    <section className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <span className="text-xs font-semibold uppercase tracking-wider text-hazard-orange">
          Project verification
        </span>
        <h2 className="text-headline-md">Trusted by contractors &amp; precast manufacturers</h2>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        {HOME_TESTIMONIALS.map((testimonial, index) => (
          <Reveal key={testimonial.author} delayMs={index * 80}>
            <Card className="flex h-full flex-col gap-4 p-6">
              <div className="flex gap-0.5 text-safety-amber" aria-label="Five out of five">
                {'★★★★★'}
              </div>
              <blockquote className="flex-1 text-body-sm italic leading-relaxed text-on-surface-variant">
                &ldquo;{testimonial.quote}&rdquo;
              </blockquote>
              <div className="flex items-center gap-3 border-t border-slate-200 pt-4">
                <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-fleet-blue font-heading text-[11px] font-semibold text-white">
                  {initials(testimonial.author)}
                </span>
                <span className="flex min-w-0 flex-col">
                  <span className="truncate text-sm font-semibold text-on-surface">
                    {testimonial.author}
                  </span>
                  <span className="truncate text-[12px] text-steel-gray">{testimonial.role}</span>
                </span>
              </div>
            </Card>
          </Reveal>
        ))}
      </div>
    </section>
  );
}
