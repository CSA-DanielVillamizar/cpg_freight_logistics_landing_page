import { useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

interface RevealProps {
  children: ReactNode;
  className?: string;
  /** Delay (ms) applied once the element enters view — use to stagger siblings. */
  delayMs?: number;
}

/**
 * Fades and lifts its children in as they scroll into view. The resting state is
 * fully visible: it only starts hidden for `motion-safe` users, a 600 ms timer
 * guarantees it appears even if the observer never fires, and `motion-reduce`
 * users get no animation at all.
 */
export function Reveal({ children, className, delayMs = 0 }: RevealProps): JSX.Element {
  const ref = useRef<HTMLDivElement>(null);
  const [shown, setShown] = useState(false);

  useEffect(() => {
    if (shown) {
      return;
    }

    const el = ref.current;
    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (prefersReduced || el === null || typeof IntersectionObserver === 'undefined') {
      setShown(true);
      return;
    }

    const safety = window.setTimeout(() => setShown(true), 600);
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0]?.isIntersecting) {
          window.clearTimeout(safety);
          setShown(true);
          observer.disconnect();
        }
      },
      { threshold: 0.08, rootMargin: '0px 0px -8% 0px' },
    );
    observer.observe(el);

    return () => {
      window.clearTimeout(safety);
      observer.disconnect();
    };
  }, [shown]);

  return (
    <div
      ref={ref}
      style={{ transitionDelay: shown ? `${delayMs}ms` : '0ms' }}
      className={cn(
        'transition-all duration-500 ease-out motion-reduce:transition-none',
        shown ? 'translate-y-0 opacity-100' : 'motion-safe:translate-y-3 motion-safe:opacity-0',
        className,
      )}
    >
      {children}
    </div>
  );
}
