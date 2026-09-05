import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/features/auth/useAuth';
import cpgLogo from '@/assets/cpg-logo.png';

const DISPATCH_PHONE = '(407) 555-0194';

const PRIMARY_NAV_LINKS = [
  { label: 'Cold Chain', to: '/verticals/refrigerated-cold-chain' },
  { label: 'FDOT Permits', to: '/verticals/fdot-concrete-barricades' },
  { label: 'Heavy Haul', to: '/verticals/flatbed-heavy-haul' },
  { label: 'Rate Calculator', to: '/rates' },
] as const;

export function SiteHeader(): JSX.Element {
  const { user, isAuthenticated, hasRole, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout(): void {
    logout();
    navigate('/');
  }

  return (
    <header className="sticky top-0 z-50 border-b border-outline bg-surface/90 backdrop-blur-xl">
      <div className="mx-auto flex h-16 max-w-container items-center justify-between gap-3 px-4">
        <Link to="/" className="flex min-w-0 items-center gap-3">
          <img src={cpgLogo} alt="CPG Enterprises" className="h-9 w-9 shrink-0 object-contain" />
          <span className="flex min-w-0 flex-col">
            <span className="font-heading text-headline-sm tracking-tight">CPG Enterprises</span>
            <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
              Heavy Haul &amp; Specialized Logistics
            </span>
          </span>
        </Link>

        <nav className="hidden items-center gap-6 md:flex">
          {PRIMARY_NAV_LINKS.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              className="font-mono text-label-sm uppercase tracking-wide text-steel-gray transition-colors hover:text-hazard-orange"
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <nav className="flex items-center gap-2">
          {isAuthenticated ? (
            <Link
              to="/load-board"
              className="hidden font-mono text-label-sm uppercase tracking-wide text-steel-gray hover:text-on-surface sm:inline"
            >
              Load board
            </Link>
          ) : null}
          {hasRole('Admin') ? (
            <Link
              to="/admin/audit-logs"
              className="hidden font-mono text-label-sm uppercase tracking-wide text-steel-gray hover:text-on-surface sm:inline"
            >
              Audit log
            </Link>
          ) : null}
          {hasRole('Carrier') ? (
            <Link
              to="/carrier"
              className="hidden font-mono text-label-sm uppercase tracking-wide text-steel-gray hover:text-on-surface sm:inline"
            >
              Carrier portal
            </Link>
          ) : null}

          <a
            href={`tel:${DISPATCH_PHONE.replace(/[^\d]/g, '')}`}
            className="flex h-11 items-center gap-2 rounded bg-surface-muted px-3 font-mono text-label-sm text-on-surface hover:bg-secondary-container"
          >
            <span className="text-hazard-orange">☎</span>
            <span className="hidden sm:inline">{DISPATCH_PHONE}</span>
          </a>

          {isAuthenticated ? (
            <button
              type="button"
              onClick={handleLogout}
              className="flex h-11 items-center gap-2 rounded border border-steel-gray px-3 font-mono text-label-sm uppercase tracking-wide text-primary hover:bg-surface-muted"
            >
              <span className="hidden sm:inline">{user?.email}</span>
              <span>Sign out</span>
            </button>
          ) : (
            <Link
              to="/login"
              className="flex h-11 items-center rounded bg-primary px-4 font-mono text-label-sm uppercase tracking-wide text-white hover:bg-secondary"
            >
              Sign in
            </Link>
          )}
        </nav>
      </div>
    </header>
  );
}
