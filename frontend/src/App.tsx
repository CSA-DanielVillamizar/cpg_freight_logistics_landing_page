import { Outlet } from 'react-router-dom';
import { SiteHeader } from '@/features/landing/components/SiteHeader';

export function App(): JSX.Element {
  return (
    <div className="flex min-h-screen flex-col">
      <SiteHeader />
      <main className="flex-1">
        <Outlet />
      </main>
      <footer className="border-t border-slate-200 py-8 text-center text-body-sm text-steel-gray">
        <p>CPG Enterprises of Orlando, Inc. — Heavy Haul &amp; Specialized Logistics</p>
        <p className="text-xs font-semibold uppercase tracking-widest">
          DOT Compliance: <span className="font-mono font-medium tracking-normal">FL-ORL-982</span> ·{' '}
          <span className="font-mono font-medium tracking-normal">MC-749211</span>
        </p>
      </footer>
    </div>
  );
}
