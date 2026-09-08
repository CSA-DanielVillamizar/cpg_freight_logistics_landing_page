import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '@/shared/api/client';
import { cn } from '@/shared/lib/cn';
import type { ComplianceStatus, ComplianceStatusResponse } from '@/shared/api/types';
import { formatEnum } from '@/shared/lib/formatEnum';
import { Badge, Card } from '@/shared/ui';
import type { BadgeTone } from '@/shared/ui';
import { COMPLIANCE_DOCUMENTS, complianceApi } from './complianceApi';
import { ComplianceDropzone } from './ComplianceDropzone';

const STATUS_TONE: Record<ComplianceStatus, BadgeTone> = {
  PendingCompliance: 'dispatched',
  UnderReview: 'dispatched',
  Verified: 'delivered',
  Rejected: 'rejected',
};

/** Where each account status sits on the Pending -> Under review -> Verified track. */
const STATUS_STEP: Record<ComplianceStatus, number> = {
  PendingCompliance: 0,
  UnderReview: 1,
  Verified: 2,
  Rejected: 1,
};

const VERIFICATION_STEPS: readonly { label: string; blurb: string }[] = [
  { label: 'Packet filed', blurb: 'Upload the compliance documents below' },
  { label: 'Under review', blurb: 'CPG dispatch verifies coverage and authority' },
  { label: 'Verified', blurb: 'Accept high-value and over-dimensional loads' },
];

const NEXT_STEP: Record<ComplianceStatus, string> = {
  PendingCompliance: 'Upload your operating authority and insurance to start verification.',
  UnderReview: 'Your packet is with CPG dispatch — no action needed while it is reviewed.',
  Verified: 'You are cleared to book any load on the board, including superload lanes.',
  Rejected: 'One or more documents were declined — re-upload the corrected files below.',
};

const bytesToMb = (bytes: number): string => `${(bytes / (1024 * 1024)).toFixed(2)} MB`;

export function CarrierPortalPage(): JSX.Element {
  const [status, setStatus] = useState<ComplianceStatusResponse | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);

  const onUploaded = useCallback((next: ComplianceStatusResponse) => setStatus(next), []);

  useEffect(() => {
    const controller = new AbortController();
    complianceApi
      .getStatus()
      .then(setStatus)
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 404) {
          setLoadError('No carrier account is linked to your login.');
        } else if (!(error instanceof DOMException && error.name === 'AbortError')) {
          setLoadError('Unable to load your compliance status.');
        }
      });
    return () => controller.abort();
  }, []);

  const filedTypes = new Set(status?.documents.map((doc) => doc.documentType));
  const activeStep = status ? STATUS_STEP[status.status] : 0;
  const isRejected = status?.status === 'Rejected';

  return (
    <div className="mx-auto flex max-w-container flex-col gap-8 px-4 py-10">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Carrier Operations
        </span>
        <h1 className="text-headline-lg">Compliance &amp; Verification</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          File your compliance packet once. Your account moves from Pending to Under Review to
          Verified — Verified carriers can accept every load on the board.
        </p>
      </header>

      {loadError ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">{loadError}</Card>
      ) : null}

      {status ? (
        <>
          <Card raised className="flex flex-col gap-5 p-5">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div className="flex flex-col">
                <span className="font-heading text-headline-sm">{status.companyName}</span>
                <span className="text-body-sm text-steel-gray">
                  Carrier ID{' '}
                  <span className="font-mono tabular-nums">
                    {status.carrierId.slice(0, 8).toUpperCase()}
                  </span>
                </span>
              </div>
              <Badge tone={STATUS_TONE[status.status]}>{formatEnum(status.status)}</Badge>
            </div>

            {/* Verification track */}
            <ol className="grid gap-3 sm:grid-cols-3">
              {VERIFICATION_STEPS.map((step, index) => {
                const done = index < activeStep && !isRejected;
                const current = index === activeStep;
                return (
                  <li
                    key={step.label}
                    className={cn(
                      'flex flex-col gap-1 rounded-lg border p-3',
                      done && 'border-fleet-blue/30 bg-fleet-blue-soft',
                      current && !isRejected && 'border-fleet-blue bg-fleet-blue-soft',
                      current && isRejected && 'border-error/40 bg-error-container',
                      !done && !current && 'border-slate-200 bg-surface-card',
                    )}
                  >
                    <div className="flex items-center gap-2">
                      <span
                        className={cn(
                          'flex h-6 w-6 shrink-0 items-center justify-center rounded-full font-mono text-[11px] font-semibold tabular-nums',
                          done && 'bg-fleet-blue text-white',
                          current && !isRejected && 'bg-fleet-blue text-white',
                          current && isRejected && 'bg-error text-white',
                          !done && !current && 'bg-surface-muted text-steel-gray',
                        )}
                      >
                        {done ? (
                          <span className="material-symbols-outlined text-[16px]" aria-hidden>
                            check
                          </span>
                        ) : (
                          index + 1
                        )}
                      </span>
                      <span className="text-xs font-semibold uppercase tracking-wider text-on-surface">
                        {step.label}
                      </span>
                    </div>
                    <span className="text-body-sm text-steel-gray">{step.blurb}</span>
                  </li>
                );
              })}
            </ol>

            <p
              className={cn(
                'flex items-start gap-2 rounded-lg p-3 text-body-sm',
                isRejected ? 'bg-error-container text-error' : 'bg-surface-muted text-steel-gray',
              )}
            >
              <span className="material-symbols-outlined text-[18px]" aria-hidden>
                {isRejected ? 'error' : 'info'}
              </span>
              {NEXT_STEP[status.status]}
            </p>
          </Card>

          <div className="grid gap-6 md:grid-cols-[1.1fr_0.9fr]">
            <div className="flex flex-col gap-6">
              <Card className="p-6">
                <h2 className="mb-4 text-headline-sm">Upload a document</h2>
                <ComplianceDropzone onUploaded={onUploaded} />
              </Card>

              <Card className="p-6">
                <h2 className="mb-1 text-headline-sm">Required packet</h2>
                <p className="mb-4 text-body-sm text-steel-gray">
                  {filedTypes.size} of {COMPLIANCE_DOCUMENTS.length} document types on file.
                </p>
                <ul className="flex flex-col divide-y divide-slate-200">
                  {COMPLIANCE_DOCUMENTS.map((doc) => {
                    const filed = filedTypes.has(doc.value);
                    return (
                      <li key={doc.value} className="flex items-start gap-3 py-3">
                        <span
                          className={cn(
                            'material-symbols-outlined mt-0.5 text-[20px]',
                            filed ? 'text-success' : 'text-steel-gray',
                          )}
                          aria-hidden
                        >
                          {filed ? 'check_circle' : 'radio_button_unchecked'}
                        </span>
                        <div className="flex min-w-0 flex-col">
                          <span className="text-body-sm font-semibold text-on-surface">
                            {doc.label}
                          </span>
                          <span className="text-body-sm text-steel-gray">
                            {filed ? 'On file' : doc.hint}
                          </span>
                        </div>
                      </li>
                    );
                  })}
                </ul>
              </Card>
            </div>

            <Card className="p-6">
              <h2 className="mb-4 text-headline-sm">Filed documents</h2>
              {status.documents.length === 0 ? (
                <p className="text-body-sm text-steel-gray">Nothing filed yet.</p>
              ) : (
                <ul className="flex flex-col divide-y divide-slate-200">
                  {status.documents.map((doc) => (
                    <li key={doc.id} className="flex items-center justify-between gap-3 py-3">
                      <div className="flex min-w-0 flex-col">
                        <span className="truncate font-mono text-body-sm">{doc.originalFileName}</span>
                        <span className="text-body-sm text-steel-gray">
                          {formatEnum(doc.documentType)} ·{' '}
                          <span className="font-mono tabular-nums">
                            {bytesToMb(doc.sizeBytes)} ·{' '}
                            {new Date(doc.uploadedAtUtc).toLocaleDateString()}
                          </span>
                        </span>
                      </div>
                      <Badge tone={STATUS_TONE[doc.status]}>{formatEnum(doc.status)}</Badge>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          </div>
        </>
      ) : loadError ? null : (
        <p className="text-body-sm text-steel-gray">Loading…</p>
      )}
    </div>
  );
}
