import { useCallback, useEffect, useState } from 'react';
import { toast } from 'sonner';
import { ApiError } from '@/shared/api/client';
import type { ComplianceDocumentType, ComplianceStatus } from '@/shared/api/types';
import { cn } from '@/shared/lib/cn';
import { Badge, Button, Card, EmptyState } from '@/shared/ui';
import type { BadgeTone } from '@/shared/ui';
import { adminApi } from './adminApi';
import type { CarrierComplianceView, ReviewDecision } from './adminApi';

type LoadStatus = 'loading' | 'ready' | 'forbidden' | 'error';

const FILTERS: { label: string; value: ComplianceStatus | 'All' }[] = [
  { label: 'Pending review', value: 'UnderReview' },
  { label: 'Verified', value: 'Verified' },
  { label: 'Rejected', value: 'Rejected' },
  { label: 'All carriers', value: 'All' },
];

const STATUS_TONE: Record<ComplianceStatus, BadgeTone> = {
  PendingCompliance: 'neutral',
  UnderReview: 'dispatched',
  Verified: 'delivered',
  Rejected: 'rejected',
};

const STATUS_LABEL: Record<ComplianceStatus, string> = {
  PendingCompliance: 'Pending',
  UnderReview: 'Under Review',
  Verified: 'Verified',
  Rejected: 'Rejected',
};

const DOC_TYPE_LABEL: Record<ComplianceDocumentType, string> = {
  CertificateOfInsurance: 'COI',
  GeneralLiabilityInsurance: 'General Liability',
  FdotPermit: 'FDOT Permit',
  OperatingAuthority: 'Operating Authority',
  W9: 'W-9',
};

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

const formatDate = (iso: string | null): string => (iso ? dateFormatter.format(new Date(iso)) : '—');
const formatMb = (bytes: number): string => `${(bytes / (1024 * 1024)).toFixed(2)} MB`;

/** Admin control tower for carrier compliance review (SPEC.md US-03 / US-01 RBAC). */
export function AdminDashboardPage(): JSX.Element {
  const [filter, setFilter] = useState<ComplianceStatus | 'All'>('UnderReview');
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [carriers, setCarriers] = useState<CarrierComplianceView[]>([]);

  const load = useCallback((next: ComplianceStatus | 'All', signal?: AbortSignal) => {
    setStatus('loading');
    adminApi
      .listCarriers(next === 'All' ? undefined : next)
      .then((data) => {
        if (signal?.aborted) {
          return;
        }
        setCarriers(data);
        setStatus('ready');
      })
      .catch((error: unknown) => {
        if (signal?.aborted) {
          return;
        }
        if (error instanceof ApiError && error.status === 403) {
          setStatus('forbidden');
        } else {
          setStatus('error');
        }
      });
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    load(filter, controller.signal);
    return () => controller.abort();
  }, [filter, load]);

  const handleReviewed = useCallback(
    (updated: CarrierComplianceView) => {
      setCarriers((previous) => {
        if (filter !== 'All' && updated.status !== filter) {
          return previous.filter((carrier) => carrier.id !== updated.id);
        }
        return previous.map((carrier) => (carrier.id === updated.id ? updated : carrier));
      });
    },
    [filter],
  );

  return (
    <div className="mx-auto flex max-w-container flex-col gap-5 px-4 py-8">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Admin Control Tower
        </span>
        <h1 className="text-headline-lg">Carrier Compliance Review</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Review the legal documents carriers filed through the compliance portal, then approve to
          activate them (<span className="font-medium text-on-surface">Verified</span>) or reject.
        </p>
      </header>

      <div className="flex flex-wrap gap-2">
        {FILTERS.map((entry) => (
          <button
            key={entry.value}
            type="button"
            onClick={() => setFilter(entry.value)}
            aria-pressed={filter === entry.value}
            className={cn(
              'rounded-full border px-3.5 py-1.5 text-xs font-semibold uppercase tracking-wider transition-colors',
              filter === entry.value
                ? 'border-fleet-blue bg-fleet-blue text-white'
                : 'border-slate-200 bg-surface-card text-steel-gray hover:bg-surface-muted',
            )}
          >
            {entry.label}
          </button>
        ))}
      </div>

      {status === 'forbidden' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">Access denied</Card>
      ) : status === 'error' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          Unable to load carriers.
        </Card>
      ) : status === 'loading' ? (
        <EmptyState icon="progress_activity" title="Loading carriers…" />
      ) : carriers.length === 0 ? (
        <EmptyState
          icon="verified_user"
          title="No carriers in this state"
          hint="Switch the filter above to review carriers at a different stage."
        />
      ) : (
        <div className="flex flex-col gap-3">
          {carriers.map((carrier) => (
            <CarrierRow key={carrier.id} carrier={carrier} onReviewed={handleReviewed} />
          ))}
        </div>
      )}
    </div>
  );
}

function CarrierRow({
  carrier,
  onReviewed,
}: {
  carrier: CarrierComplianceView;
  onReviewed: (updated: CarrierComplianceView) => void;
}): JSX.Element {
  const [expanded, setExpanded] = useState(carrier.status === 'UnderReview');
  const [busy, setBusy] = useState<ReviewDecision | null>(null);

  async function review(decision: ReviewDecision): Promise<void> {
    setBusy(decision);
    try {
      const updated = await adminApi.reviewCarrier(carrier.id, decision);
      toast.success(
        decision === 'Approve'
          ? `${updated.companyName} verified — cleared to accept loads.`
          : `${updated.companyName} rejected.`,
      );
      onReviewed(updated);
    } catch (error) {
      const message =
        error instanceof ApiError && error.status === 409
          ? 'This carrier has no documents to review.'
          : 'Could not submit the review — please retry.';
      toast.error(message);
    } finally {
      setBusy(null);
    }
  }

  async function openDocument(documentId: string): Promise<void> {
    try {
      await adminApi.openDocument(carrier.id, documentId);
    } catch {
      toast.error('Could not open the document.');
    }
  }

  const canReview = carrier.documents.length > 0;

  return (
    <Card className="flex flex-col">
      <div className="flex flex-wrap items-center justify-between gap-3 p-4">
        <button
          type="button"
          onClick={() => setExpanded((value) => !value)}
          className="flex min-w-0 flex-col items-start gap-1 text-left"
        >
          <span className="flex items-center gap-2">
            <span className="font-heading text-headline-sm">{carrier.companyName}</span>
            <Badge tone={STATUS_TONE[carrier.status]}>{STATUS_LABEL[carrier.status]}</Badge>
          </span>
          <span className="text-body-sm text-steel-gray">
            DOT <span className="font-mono tabular-nums">{carrier.dotNumber ?? '—'}</span> · MC{' '}
            <span className="font-mono tabular-nums">{carrier.mcNumber ?? '—'}</span> · submitted{' '}
            <span className="font-mono tabular-nums">{formatDate(carrier.submittedAtUtc)}</span> ·{' '}
            {carrier.documents.length} doc{carrier.documents.length === 1 ? '' : 's'}
          </span>
        </button>

        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => void review('Reject')}
            disabled={busy !== null || !canReview}
          >
            {busy === 'Reject' ? 'Rejecting…' : 'Reject'}
          </Button>
          <Button
            variant="primary"
            onClick={() => void review('Approve')}
            disabled={busy !== null || !canReview}
          >
            {busy === 'Approve' ? 'Approving…' : 'Approve'}
          </Button>
        </div>
      </div>

      {expanded ? (
        <div className="border-t border-slate-200">
          {carrier.documents.length === 0 ? (
            <p className="p-4 text-body-sm text-steel-gray">No documents filed yet.</p>
          ) : (
            <table className="w-full min-w-[640px] text-left">
              <thead>
                <tr className="border-b border-slate-200 bg-surface-muted">
                  {['Type', 'File', 'Size', 'Uploaded', 'Status', ''].map((heading) => (
                    <th
                      key={heading}
                      className="whitespace-nowrap px-4 py-2.5 text-[11px] font-semibold uppercase tracking-wider text-steel-gray"
                    >
                      {heading}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {carrier.documents.map((document) => (
                  <tr key={document.id}>
                    <td className="whitespace-nowrap px-4 py-2 text-body-sm font-medium text-on-surface">
                      {DOC_TYPE_LABEL[document.documentType]}
                    </td>
                    <td className="max-w-[220px] truncate px-4 py-2 font-mono text-body-sm text-on-surface-variant">
                      {document.originalFileName}
                    </td>
                    <td className="whitespace-nowrap px-4 py-2 font-mono text-body-sm tabular-nums text-steel-gray">
                      {formatMb(document.sizeBytes)}
                    </td>
                    <td className="whitespace-nowrap px-4 py-2 font-mono text-body-sm tabular-nums text-steel-gray">
                      {formatDate(document.uploadedAtUtc)}
                    </td>
                    <td className="whitespace-nowrap px-4 py-2">
                      <Badge tone={STATUS_TONE[document.status]}>{STATUS_LABEL[document.status]}</Badge>
                    </td>
                    <td className="whitespace-nowrap px-4 py-2 text-right">
                      <button
                        type="button"
                        onClick={() => void openDocument(document.id)}
                        className="text-xs font-semibold uppercase tracking-wider text-fleet-blue hover:underline"
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      ) : null}
    </Card>
  );
}
