import type { HTMLAttributes, ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  /**
   * Lifts the card with a stronger shadow. Use only for the single primary
   * panel of a screen, never as a default.
   */
  raised?: boolean;
  children: ReactNode;
}

export function Card({ raised = false, className, children, ...rest }: CardProps): JSX.Element {
  return (
    <div
      className={cn(
        'rounded-lg border border-slate-200 bg-surface-card',
        raised ? 'shadow-md' : 'shadow-sm',
        className,
      )}
      {...rest}
    >
      {children}
    </div>
  );
}
