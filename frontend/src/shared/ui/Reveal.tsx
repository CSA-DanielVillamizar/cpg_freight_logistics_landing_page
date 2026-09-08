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
 * reveals it if the observer never fires *for content already in view*, and
 * `motion-reduce` users get no animation at all. Content still below the fold
 * keeps waiting on the observer so its scroll-in is preserved.
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

    // Fallback for a stuck observer, but only for content that is already on
    // screen — a below-the-fold section keeps waiting so its scroll-in survives.
    const safety = window.setTimeout(() => {
      const rect = el.getBoundingClientRect();
      const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
      if (rect.top < viewportHeight && rect.bottom > 0) {
        setShown(true);
      }
    }, 600);
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
