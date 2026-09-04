import type { ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

export type BadgeTone = 'dispatched' | 'delivered' | 'oversize' | 'neutral';

interface BadgeProps {
  tone?: BadgeTone;
  children: ReactNode;
}

const TONE_CLASSES: Record<BadgeTone, string> = {
  dispatched: 'bg-warning-container text-warning border border-[#FCD34D]',
  delivered: 'bg-success-container text-success border border-[#A7F3D0]',
  oversize: 'bg-primary text-hazard-orange border border-hazard-orange',
  neutral: 'bg-surface-muted text-steel-gray border border-outline',
};

export function Badge({ tone = 'neutral', children }: BadgeProps): JSX.Element {
  return (
    <span
      className={cn(
        'inline-flex h-6 items-center rounded px-2',
        'font-mono text-label-sm uppercase',
        TONE_CLASSES[tone],
      )}
    >
      {children}
    </span>
  );
}
