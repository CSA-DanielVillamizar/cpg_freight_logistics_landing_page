import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'sonner';
import { ApiError } from '@/shared/api/client';
import { Badge, Button, Card } from '@/shared/ui';
import type { BadgeTone } from '@/shared/ui';
import { ShipperNav } from './ShipperNav';
import { shipperApi } from './shipperApi';
import type { InvoiceStatus, ShipperInvoiceView, ShipperInvoicesResponse } from './shipperApi';

type PageStatus = 'loading' | 'ready' | 'forbidden' | 'error';

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
const formatDate = (iso: string): string => dateFormatter.format(new Date(iso));

const STATUS_TONE: Record<InvoiceStatus, BadgeTone> = {
  Draft: 'neutral',
  Pending: 'dispatched',
  Paid: 'delivered',
  Overdue: 'rejected',
};

export function ShipperBillingPage(): JSX.Element {
  const [status, setStatus] = useState<PageStatus>('loading');
  const [data, setData] = useState<ShipperInvoicesResponse | null>(null);
  const [payingId, setPayingId] = useState<string | null>(null);
  const [searchParams, setSearchParams] = useSearchParams();

  useEffect(() => {
    const outcome = searchParams.get('checkout');
    if (outcome === 'success') {
      toast.success('Payment received — thank you.');
    } else if (outcome === 'canceled') {
      toast.info('Checkout canceled — the invoice is still open.');
    }
    if (outcome) {
      searchParams.delete('checkout');
      setSearchParams(searchParams, { replace: true });
    }
  }, [searchParams, setSearchParams]);

  useEffect(() => {
    const controller = new AbortController();
    shipperApi
      .getInvoices()
      .then((response) => {
        if (!controller.signal.aborted) {
          setData(response);
          setStatus('ready');
        }
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return;
        }
        setStatus(error instanceof ApiError && error.status === 403 ? 'forbidden' : 'error');
      });
    return () => controller.abort();
  }, []);

  async function pay(invoice: ShipperInvoiceView): Promise<void> {
    setPayingId(invoice.id);
    try {
      const { checkoutUrl } = await shipperApi.payInvoice(invoice.id);
      window.location.href = checkoutUrl;
    } catch {
      toast.error('Could not start the payment — please retry.');
      setPayingId(null);
    }
  }

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-8">
      <header className="flex flex-col gap-2">
        <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
          Shipper portal
        </span>
        <h1 className="text-headline-lg">Billing &amp; Payments</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Invoices are raised automatically when your loads are delivered. Pay online through
          Stripe Checkout.
        </p>
      </header>

      <ShipperNav />

      {status === 'forbidden' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">Access denied</Card>
      ) : status === 'error' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          Unable to load your invoices right now.
        </Card>
      ) : status === 'loading' || !data ? (
        <Card className="flex h-40 items-center justify-center p-6 font-mono text-body-sm text-steel-gray">
          Loading invoices…
        </Card>
      ) : (
        <>
          <div className="grid gap-3 sm:grid-cols-[1.4fr_1fr]">
            <Card anchored className="flex flex-col gap-1 p-5">
              <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
                Total outstanding
              </span>
              <span className="font-heading text-display-lg leading-none text-primary">
                {currency.format(data.totalOutstandingUsd)}
              </span>
              {data.overdueCount > 0 ? (
                <span className="mt-1 font-mono text-label-sm uppercase text-error">
                  {data.overdueCount} overdue
                </span>
              ) : (
                <span className="mt-1 font-mono text-label-sm uppercase text-success">All current</span>
              )}
            </Card>
            <Card className="flex flex-col justify-center gap-1 p-5">
              <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
                Invoices
              </span>
              <span className="font-heading text-headline-lg tabular-nums text-fleet-blue">
                {data.invoices.length}
              </span>
              <span className="font-mono text-label-sm text-steel-gray">
                {data.invoices.filter((i) => i.status === 'Paid').length} paid ·{' '}
                {data.invoices.filter((i) => i.payable).length} open
              </span>
            </Card>
          </div>

          {data.invoices.length === 0 ? (
            <Card className="flex h-40 items-center justify-center p-6 font-mono text-body-sm text-steel-gray">
              No invoices yet — they appear once your loads are delivered.
            </Card>
          ) : (
            <div className="overflow-x-auto rounded-lg border border-outline bg-surface-card">
              <table className="w-full min-w-[720px] text-left">
                <thead>
                  <tr className="border-b border-outline bg-surface-muted">
                    {['Invoice', 'Load', 'Amount', 'Issued', 'Due', 'Status', ''].map((heading) => (
                      <th
                        key={heading}
                        className="whitespace-nowrap px-3 py-2 font-mono text-label-sm uppercase tracking-wider text-steel-gray"
                      >
                        {heading}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-outline">
                  {data.invoices.map((invoice) => (
                    <tr key={invoice.id}>
                      <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm font-semibold text-fleet-blue">
                        {invoice.reference}
                      </td>
                      <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm text-on-surface-variant">
                        {invoice.loadReference}
                      </td>
                      <td className="whitespace-nowrap px-3 py-3 text-right font-mono text-body-sm font-semibold tabular-nums text-primary">
                        {currency.format(invoice.amountUsd)}
                      </td>
                      <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                        {formatDate(invoice.issuedAtUtc)}
                      </td>
                      <td className="whitespace-nowrap px-3 py-3 font-mono text-body-sm tabular-nums text-steel-gray">
                        {formatDate(invoice.dueDate)}
                      </td>
                      <td className="whitespace-nowrap px-3 py-3">
                        <Badge tone={STATUS_TONE[invoice.status]}>{invoice.status}</Badge>
                      </td>
                      <td className="whitespace-nowrap px-3 py-3 text-right">
                        {invoice.payable ? (
                          <Button
                            variant="primary"
                            onClick={() => void pay(invoice)}
                            disabled={payingId !== null}
                          >
                            {payingId === invoice.id ? 'Redirecting…' : 'Pay now'}
                          </Button>
                        ) : (
                          <span className="font-mono text-label-sm uppercase tracking-wide text-steel-gray">
                            {invoice.paidAtUtc ? `Paid ${formatDate(invoice.paidAtUtc)}` : '—'}
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </div>
  );
}
