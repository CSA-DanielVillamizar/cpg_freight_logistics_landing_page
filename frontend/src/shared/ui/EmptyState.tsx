import type { ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

interface EmptyStateProps {
  /** Material Symbols icon name. */
  icon?: string;
  title: string;
  hint?: string;
  action?: ReactNode;
  className?: string;
}

/**
 * Compact, left-aligned empty state. Replaces the tall desolate cards -
 * a subtle icon, a one-line message and an optional call to action.
 */
export function EmptyState({
  icon = 'inbox',
  title,
  hint,
  action,
  className,
}: EmptyStateProps): JSX.Element {
  return (
    <div
      className={cn(
        'flex flex-col items-start gap-2 rounded-lg border border-slate-200 bg-surface-card p-6 shadow-sm',
        className,
      )}
    >
      <span
        className="material-symbols-outlined text-2xl text-outline-strong"
        aria-hidden
      >
        {icon}
      </span>
      <p className="font-semibold text-on-surface">{title}</p>
      {hint ? <p className="max-w-prose text-body-sm text-steel-gray">{hint}</p> : null}
      {action ? <div className="pt-1">{action}</div> : null}
    </div>
  );
}
