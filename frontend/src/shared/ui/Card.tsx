import type { HTMLAttributes, ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  /** Adds the dark navy top strip that anchors visual weight (design system). */
  anchored?: boolean;
  children: ReactNode;
}

export function Card({ anchored = false, className, children, ...rest }: CardProps): JSX.Element {
  return (
    <div
      className={cn(
        'rounded-lg border border-outline bg-surface-card',
        anchored && 'border-t-[3px] border-t-primary',
        className,
      )}
      {...rest}
    >
      {children}
    </div>
  );
}
