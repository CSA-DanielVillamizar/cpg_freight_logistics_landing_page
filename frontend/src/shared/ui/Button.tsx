import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '@/shared/lib/cn';

export type ButtonVariant = 'primary' | 'fleet' | 'outline';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  children: ReactNode;
}

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary:
    'bg-hazard-orange text-white hover:bg-[#C2410C] border-b border-[#9A3412]',
  fleet: 'bg-primary text-white border border-secondary hover:bg-secondary',
  outline:
    'bg-transparent border-[1.5px] border-steel-gray text-primary hover:bg-surface-muted',
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
        'font-heading text-label-md uppercase tracking-wide',
        'transition-all active:scale-95 disabled:cursor-not-allowed disabled:opacity-50',
        VARIANT_CLASSES[variant],
        className,
      )}
      {...rest}
    >
      {children}
    </button>
  );
}
