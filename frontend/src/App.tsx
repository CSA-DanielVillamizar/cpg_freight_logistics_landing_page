import { Outlet } from 'react-router-dom';
import { SiteHeader } from '@/features/landing/components/SiteHeader';

export function App(): JSX.Element {
  return (
    <div className="flex min-h-screen flex-col">
      <SiteHeader />
      <main className="flex-1">
        <Outlet />
      </main>
      <footer className="border-t border-outline py-8 text-center text-body-sm text-steel-gray">
        <p>CPG Enterprises of Orlando, Inc. — Heavy Haul &amp; Specialized Logistics</p>
        <p className="font-mono text-label-sm uppercase tracking-widest">
          DOT Compliance: FL-ORL-982 • MC-749211
        </p>
      </footer>
    </div>
  );
}
