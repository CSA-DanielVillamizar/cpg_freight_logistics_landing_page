import { useEffect, useId, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/features/auth/useAuth';
import { cn } from '@/shared/lib/cn';
import cpgLogo from '@/assets/cpg-logo.png';

const DISPATCH_PHONE = '(407) 555-0194';

const PRIMARY_NAV_LINKS = [
  { label: 'Cold Chain', to: '/verticals/refrigerated-cold-chain' },
  { label: 'FDOT Permits', to: '/verticals/fdot-concrete-barricades' },
  { label: 'Heavy Haul', to: '/verticals/flatbed-heavy-haul' },
  { label: 'Rate Calculator', to: '/rates' },
] as const;

interface NavLink {
  to: string;
  label: string;
}

export function SiteHeader(): JSX.Element {
  const { user, isAuthenticated, hasRole, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuId = useId();

  // Close the mobile menu whenever the route changes.
  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  // Close on Escape for keyboard users.
  useEffect(() => {
    if (!menuOpen) {
      return;
    }
    function onKey(event: KeyboardEvent): void {
      if (event.key === 'Escape') {
        setMenuOpen(false);
      }
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [menuOpen]);

  const appLinks: NavLink[] = [
    isAuthenticated ? { to: '/load-board', label: 'Load board' } : null,
    isAuthenticated ? { to: '/tracking', label: 'Live tracking' } : null,
    hasRole('Admin') ? { to: '/admin/carriers', label: 'Carrier review' } : null,
    hasRole('Admin') ? { to: '/admin/audit-logs', label: 'Audit log' } : null,
    hasRole('Shipper') ? { to: '/shipper/dashboard', label: 'My shipments' } : null,
    hasRole('Carrier') ? { to: '/carrier', label: 'Carrier portal' } : null,
  ].filter((link): link is NavLink => link !== null);

  function handleLogout(): void {
    setMenuOpen(false);
    logout();
    navigate('/');
  }

  return (
    <header className="sticky top-0 z-50 border-b border-slate-200 bg-surface/90 backdrop-blur-xl">
      <div className="mx-auto flex h-16 max-w-container items-center justify-between gap-3 px-4">
        <Link to="/" className="flex min-w-0 items-center gap-3">
          <img src={cpgLogo} alt="CPG Enterprises" className="h-9 w-9 shrink-0 object-contain" />
          <span className="flex min-w-0 flex-col">
            <span className="whitespace-nowrap font-heading text-lg tracking-tight sm:text-headline-sm">
              CPG Enterprises
            </span>
            <span className="hidden text-[11px] font-semibold uppercase tracking-wider text-steel-gray sm:block">
              Heavy Haul &amp; Specialized Logistics
            </span>
          </span>
        </Link>

        <nav className="hidden items-center gap-6 md:flex">
          {PRIMARY_NAV_LINKS.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              className="text-xs font-semibold uppercase tracking-wider text-steel-gray transition-colors hover:text-fleet-blue"
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <nav className="flex items-center gap-2">
          {appLinks.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              className="hidden text-xs font-semibold uppercase tracking-wider text-steel-gray hover:text-on-surface md:inline"
            >
              {link.label}
            </Link>
          ))}

          <a
            href={`tel:${DISPATCH_PHONE.replace(/[^\d]/g, '')}`}
            className="flex h-11 items-center gap-1.5 rounded bg-surface-muted px-3 text-sm font-medium tabular-nums text-on-surface transition-colors hover:bg-secondary-container"
          >
            <span className="material-symbols-outlined text-[18px] text-fleet-blue" aria-hidden>
              call
            </span>
            <span className="hidden sm:inline">{DISPATCH_PHONE}</span>
          </a>

          {isAuthenticated ? (
            <button
              type="button"
              onClick={handleLogout}
              className="hidden h-11 items-center gap-2 rounded border border-outline-strong px-3 text-xs font-semibold uppercase tracking-wider text-primary transition-colors hover:bg-surface-muted md:flex"
            >
              <span className="hidden lg:inline">{user?.email}</span>
              <span>Sign out</span>
            </button>
          ) : (
            <Link
              to="/login"
              className="hidden h-11 items-center rounded bg-fleet-blue px-4 text-xs font-semibold uppercase tracking-wider text-white transition-colors hover:bg-fleet-blue-hover md:flex"
            >
              Sign in
            </Link>
          )}

          <button
            type="button"
            aria-label="Toggle navigation menu"
            aria-expanded={menuOpen}
            aria-controls={menuId}
            onClick={() => setMenuOpen((open) => !open)}
            className="flex h-11 w-11 items-center justify-center rounded border border-outline-strong text-primary transition-colors hover:bg-surface-muted md:hidden"
          >
            <span className="material-symbols-outlined text-[22px]" aria-hidden>
              {menuOpen ? 'close' : 'menu'}
            </span>
          </button>
        </nav>
      </div>

      {menuOpen ? (
        <div
          id={menuId}
          className="absolute inset-x-0 top-16 origin-top border-b border-slate-200 bg-surface shadow-md md:hidden"
        >
          <nav className="mx-auto flex max-w-container flex-col gap-1 px-4 py-3">
            {appLinks.length > 0 ? (
              <>
                <p className="px-2 pb-1 pt-2 text-[11px] font-semibold uppercase tracking-wider text-outline-strong">
                  Workspace
                </p>
                {appLinks.map((link) => (
                  <MobileLink key={link.to} to={link.to} label={link.label} />
                ))}
                <hr className="my-2 border-slate-200" />
              </>
            ) : null}

            <p className="px-2 pb-1 pt-2 text-[11px] font-semibold uppercase tracking-wider text-outline-strong">
              Services
            </p>
            {PRIMARY_NAV_LINKS.map((link) => (
              <MobileLink key={link.to} to={link.to} label={link.label} />
            ))}

            <hr className="my-2 border-slate-200" />

            {isAuthenticated ? (
              <button
                type="button"
                onClick={handleLogout}
                className={cn(
                  'flex h-11 items-center rounded px-2 text-xs font-semibold uppercase tracking-wider',
                  'text-primary transition-colors hover:bg-surface-muted',
                )}
              >
                Sign out{user?.email ? ` · ${user.email}` : ''}
              </button>
            ) : (
              <Link
                to="/login"
                className="flex h-11 items-center justify-center rounded bg-fleet-blue px-4 text-xs font-semibold uppercase tracking-wider text-white transition-colors hover:bg-fleet-blue-hover"
              >
                Sign in
              </Link>
            )}
          </nav>
        </div>
      ) : null}
    </header>
  );
}

function MobileLink({ to, label }: NavLink): JSX.Element {
  return (
    <Link
      to={to}
      className="flex h-11 items-center rounded px-2 text-xs font-semibold uppercase tracking-wider text-steel-gray transition-colors hover:bg-surface-muted hover:text-on-surface"
    >
      {label}
    </Link>
  );
}
