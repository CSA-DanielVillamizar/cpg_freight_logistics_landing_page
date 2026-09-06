import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

export type ButtonVariant = 'primary' | 'fleet' | 'outline';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  children: ReactNode;
}

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  // Corporate action colour - fleet blue, never hazard orange.
  primary: 'bg-fleet-blue text-white hover:bg-fleet-blue-hover shadow-sm',
  fleet: 'bg-primary text-white hover:bg-secondary shadow-sm',
  outline:
    'bg-transparent border border-outline-strong text-primary hover:bg-surface-muted',
};

export function Button({
  variant = 'primary',
  className,
  children,
  type = 'button',
  ...rest
}: ButtonProps): JSX.Element {
  return (
    <button
      type={type === 'submit' ? 'submit' : type === 'reset' ? 'reset' : 'button'}
      className={cn(
        'inline-flex h-12 items-center justify-center gap-2 rounded px-4',
        'text-xs font-semibold uppercase tracking-wider',
        'transition-all active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-50',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-fleet-blue/40 focus-visible:ring-offset-1',
        VARIANT_CLASSES[variant],
        className,
      )}
      {...rest}
    >
      {children}
    </button>
  );
}
