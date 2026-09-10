import { Link } from 'react-router-dom';
import { Card } from '@/shared/ui';
import { RoleCard } from './RoleCard';

/** /signup — the Tri-Sign-Up entry point: Shipper, Carrier or Independent Agent (T-SDD Epica 1). */
export function RoleSelectionPage(): JSX.Element {
  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-8 px-4 py-16">
      <header className="flex flex-col gap-2 text-center">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Join the CPG Network
        </span>
        <h1 className="text-headline-lg">How will you work with CPG Enterprises?</h1>
        <p className="mx-auto max-w-prose text-body-sm text-steel-gray">
          Pick the workspace that matches your business — you can start moving freight in minutes.
        </p>
      </header>

      <div className="grid gap-4 sm:grid-cols-3">
        <RoleCard
          to="/signup/shipper"
          icon="local_shipping"
          title="Shipper"
          description="Post loads, track shipments live and pay invoices from one dashboard."
        />
        <RoleCard
          to="/signup/carrier"
          icon="badge"
          title="Carrier"
          description="Book loads off the board, file compliance docs and get paid fast."
        />
        <RoleCard
          to="/signup/agent"
          icon="handshake"
          title="Independent Agent"
          description="Run your own brokerage under CPG's authority — invite clients, publish loads, earn commission."
        />
      </div>

      <Card className="mx-auto max-w-md p-4 text-center">
        <p className="text-body-sm text-steel-gray">
          Already have an account?{' '}
          <Link to="/login" className="font-semibold text-fleet-blue hover:underline">
            Sign in
          </Link>
        </p>
      </Card>
    </div>
  );
}
