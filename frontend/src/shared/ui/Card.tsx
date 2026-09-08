import type { HTMLAttributes, ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  /**
   * Lifts the card with a stronger shadow. Use only for the single primary
   * panel of a screen, never as a default.
   */
  raised?: boolean;
  /**
   * Adds a hover lift + shadow. Use for cards that are (or contain) a link —
   * signals the whole card is a target.
   */
  interactive?: boolean;
  children: ReactNode;
}

export function Card({
  raised = false,
  interactive = false,
  className,
  children,
  ...rest
}: CardProps): JSX.Element {
  return (
    <div
      className={cn(
        'rounded-lg border border-slate-200 bg-surface-card',
        raised ? 'shadow-md' : 'shadow-sm',
        interactive &&
          'transition-all duration-200 hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-md motion-reduce:transition-none motion-reduce:hover:translate-y-0',
        className,
      )}
      {...rest}
    >
      {children}
    </div>
  );
}
