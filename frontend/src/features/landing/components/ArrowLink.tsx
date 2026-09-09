import { Link } from 'react-router-dom';
import { cn } from '@/shared/lib/cn';

interface ArrowLinkProps {
  to: string;
  children: string;
  className?: string;
}

/**
 * Text link whose arrow nudges right when the link (its own `group/arrow`) is hovered or
 * receives keyboard focus (`focus-visible`).
 */
export function ArrowLink({ to, children, className }: ArrowLinkProps): JSX.Element {
  return (
    <Link
      to={to}
      className={cn(
        'group/arrow inline-flex items-center gap-1.5 text-sm font-semibold text-fleet-blue',
        'transition-colors hover:text-fleet-blue-hover',
        className,
      )}
    >
      {children}
      <span
        aria-hidden
        className="transition-transform duration-200 group-hover/arrow:translate-x-0.5 group-focus-visible/arrow:translate-x-0.5 motion-reduce:transition-none"
      >
        &rarr;
      </span>
    </Link>
  );
}
