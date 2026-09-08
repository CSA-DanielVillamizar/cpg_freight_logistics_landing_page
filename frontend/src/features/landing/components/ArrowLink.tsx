import { Link } from 'react-router-dom';
import { cn } from '@/shared/lib/cn';

interface ArrowLinkProps {
  to: string;
  children: string;
  className?: string;
}

/** Text link with an arrow that nudges right on hover / group-hover. */
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
        className="transition-transform duration-200 group-hover/arrow:translate-x-0.5 group-hover:translate-x-0.5 motion-reduce:transition-none"
      >
        &rarr;
      </span>
    </Link>
  );
}
