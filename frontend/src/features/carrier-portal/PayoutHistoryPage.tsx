import { EmptyState, Card } from '@/shared/ui';
import { PayoutStatusBadge } from './PayoutStatusBadge';
import { QuickPayToggle } from './QuickPayToggle';
import { useCarrierPayouts } from './useCarrierPayouts';

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
const formatDate = (iso: string): string => dateFormatter.format(new Date(iso));

/** /carrier/payouts — the Carrier's Quick Pay / Stripe Connect payout ledger (T-SDD Epica 2B). */
export function PayoutHistoryPage(): JSX.Element {
  const { status, payouts, refresh } = useCarrierPayouts();

  const totalNet = payouts
    .filter((p) => p.status === 'Completed')
    .reduce((sum, p) => sum + p.carrierNetAmountUsd, 0);
  const pendingCount = payouts.filter((p) => p.status === 'Pending' || p.status === 'Processing').length;

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-8">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Carrier portal
        </span>
        <h1 className="text-headline-lg">Payouts</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Every delivered load's revenue split — CPG's margin, an optional Agent commission, an
          optional Quick Pay fee, and your net payout via Stripe Connect.
        </p>
      </header>

      {status === 'error' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          Unable to load your payouts right now.
        </Card>
      ) : status === 'loading' ? (
        <EmptyState icon="progress_activity" title="Loading payouts…" />
      ) : payouts.length === 0 ? (
        <EmptyState
          icon="payments"
          title="No payouts yet"
          hint="Payouts appear here once a shipper invoice is raised for one of your delivered loads."
        />
      ) : (
        <>
          <div className="grid gap-3 sm:grid-cols-2">
            <Card raised className="flex flex-col gap-1 p-5">
              <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                Total settled
              </span>
              <span className="font-heading text-display-lg leading-none tabular-nums text-primary">
                {currency.format(totalNet)}
              </span>
            </Card>
            <Card className="flex flex-col justify-center gap-1 p-5">
              <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                In progress
              </span>
              <span className="font-heading text-headline-lg tabular-nums text-fleet-blue">
                {pendingCount}
              </span>
            </Card>
          </div>

          <div className="overflow-x-auto rounded-lg border border-slate-200 bg-surface-card shadow-sm">
            <table className="w-full min-w-[880px] text-left">
              <thead>
                <tr className="border-b border-slate-200 bg-surface-muted">
                  {['Load', 'Gross', 'Margin', 'Agent', 'Quick Pay fee', 'Net payout', 'Status', ''].map(
                    (heading) => (
                      <th
                        key={heading}
                        className="whitespace-nowrap px-3 py-2.5 text-[11px] font-semibold uppercase tracking-wider text-steel-gray"
                      >
                        {heading}
                      </th>
                    ),
                  )}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {payouts.map((payout) => (
                  <tr key={payout.disbursementId}>
                    <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm font-semibold text-fleet-blue">
                      {payout.loadReference}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-steel-gray">
                      {currency.format(payout.grossAmountUsd)}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-steel-gray">
                      -{currency.format(payout.cpgMarginAmountUsd)}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-steel-gray">
                      {payout.agentCommissionAmountUsd > 0
                        ? `-${currency.format(payout.agentCommissionAmountUsd)}`
                        : '—'}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm tabular-nums text-steel-gray">
                      {payout.quickPayFeeAmountUsd > 0
                        ? `-${currency.format(payout.quickPayFeeAmountUsd)}`
                        : '—'}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm font-semibold tabular-nums text-primary">
                      {currency.format(payout.carrierNetAmountUsd)}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3">
                      <div className="flex flex-col gap-1">
                        <PayoutStatusBadge status={payout.status} />
                        {payout.status === 'Failed' && payout.failureReason ? (
                          <span className="text-xs text-error">{payout.failureReason}</span>
                        ) : payout.processedAtUtc ? (
                          <span className="text-xs text-steel-gray">{formatDate(payout.processedAtUtc)}</span>
                        ) : null}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right">
                      {payout.status === 'Pending' && !payout.quickPayRequested ? (
                        <QuickPayToggle invoiceId={payout.invoiceId} onRequested={refresh} />
                      ) : payout.quickPayRequested ? (
                        <span className="text-xs font-semibold uppercase tracking-wider text-success">
                          Quick Pay
                        </span>
                      ) : null}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
