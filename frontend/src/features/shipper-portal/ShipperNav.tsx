import { NavLink } from 'react-router-dom';
import { cn } from '@/shared/lib/cn';

const TABS = [
  { to: '/shipper/dashboard', label: 'Shipments' },
  { to: '/shipper/billing', label: 'Billing' },
];

/** Sub-navigation shared by the shipper portal pages. */
export function ShipperNav(): JSX.Element {
  return (
    <nav className="flex gap-2 border-b border-outline">
      {TABS.map((tab) => (
        <NavLink
          key={tab.to}
          to={tab.to}
          className={({ isActive }) =>
            cn(
              '-mb-px border-b-2 px-3 py-2 font-mono text-label-sm uppercase tracking-wide transition-colors',
              isActive
                ? 'border-hazard-orange text-on-surface'
                : 'border-transparent text-steel-gray hover:text-on-surface',
            )
          }
        >
          {tab.label}
        </NavLink>
      ))}
    </nav>
  );
}
