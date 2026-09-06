import { useId } from 'react';
import type { InputHTMLAttributes } from 'react';
import { cn } from '@/shared/lib/cn';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  hint?: string;
  error?: string;
}

export function Input({ label, hint, error, className, id, ...rest }: InputProps): JSX.Element {
  const generatedId = useId();
  const inputId = id ?? generatedId;

  return (
    <div className="flex flex-col gap-1">
      <label
        htmlFor={inputId}
        className="text-xs font-semibold uppercase tracking-wider text-steel-gray"
      >
        {label}
      </label>
      <input
        id={inputId}
        className={cn(
          'h-12 rounded border border-outline-strong bg-surface-card px-3 text-[16px] text-on-surface',
          'outline-none transition-colors focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25',
          error && 'border-error focus:border-error focus:ring-error/30',
          className,
        )}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined}
        {...rest}
      />
      {hint && !error ? (
        <p id={`${inputId}-hint`} className="text-body-sm text-on-surface-variant">
          {hint}
        </p>
      ) : null}
      {error ? (
        <p id={`${inputId}-error`} className="text-body-sm text-error">
          {error}
        </p>
      ) : null}
    </div>
  );
}
