import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '@/shared/api/client';
import type { ComplianceStatus, ComplianceStatusResponse } from '@/shared/api/types';
import { Badge, Card } from '@/shared/ui';
import type { BadgeTone } from '@/shared/ui';
import { complianceApi } from './complianceApi';
import { ComplianceDropzone } from './ComplianceDropzone';

const STATUS_TONE: Record<ComplianceStatus, BadgeTone> = {
  PendingCompliance: 'dispatched',
  UnderReview: 'dispatched',
  Verified: 'delivered',
  Rejected: 'oversize',
};

const STATUS_LABEL: Record<ComplianceStatus, string> = {
  PendingCompliance: 'Pending Compliance',
  UnderReview: 'Under Review',
  Verified: 'Verified',
  Rejected: 'Rejected',
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

  return (
    <div className="mx-auto flex max-w-container flex-col gap-8 px-4 py-10">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Carrier Portal · SPEC.md US-03
        </span>
        <h1 className="text-headline-lg">Compliance &amp; Verification</h1>
        <p className="text-body-sm text-steel-gray">
          Upload your mandatory legal documents (COI, insurance, FDOT permits). Your account moves
          from Pending to Under Review to Verified so you can accept high-value loads.
        </p>
      </header>

      {loadError ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">{loadError}</Card>
      ) : null}

      {status ? (
        <>
          <Card raised className="flex flex-wrap items-center justify-between gap-3 p-5">
            <div className="flex flex-col">
              <span className="font-heading text-headline-sm">{status.companyName}</span>
              <span className="text-body-sm text-steel-gray">
                Carrier ID{' '}
                <span className="font-mono tabular-nums">
                  {status.carrierId.slice(0, 8).toUpperCase()}
                </span>
              </span>
            </div>
            <Badge tone={STATUS_TONE[status.status]}>{STATUS_LABEL[status.status]}</Badge>
          </Card>

          <div className="grid gap-6 md:grid-cols-[0.9fr_1.1fr]">
            <Card className="p-6">
              <h2 className="mb-4 text-headline-sm">Upload a document</h2>
              <ComplianceDropzone onUploaded={onUploaded} />
            </Card>

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
                          {doc.documentType} ·{' '}
                          <span className="font-mono tabular-nums">
                            {bytesToMb(doc.sizeBytes)} ·{' '}
                            {new Date(doc.uploadedAtUtc).toLocaleDateString()}
                          </span>
                        </span>
                      </div>
                      <Badge tone={STATUS_TONE[doc.status]}>{STATUS_LABEL[doc.status]}</Badge>
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
