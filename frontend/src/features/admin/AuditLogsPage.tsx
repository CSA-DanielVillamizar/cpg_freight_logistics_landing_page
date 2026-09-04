import { useEffect, useState } from 'react';
import { ApiError, apiClient } from '@/shared/api/client';
import { Card } from '@/shared/ui';

interface AuditLogEntry {
  id: string;
  action: string;
  entityName: string;
  timestampUtc: string;
  userId?: string;
}

type Status = 'loading' | 'ready' | 'forbidden' | 'error';

/** Admin-only audit feed. Demonstrates RBAC end to end (SPEC.md US-01). */
export function AuditLogsPage(): JSX.Element {
  const [status, setStatus] = useState<Status>('loading');
  const [entries, setEntries] = useState<AuditLogEntry[]>([]);

  useEffect(() => {
    const controller = new AbortController();
    apiClient
      .get<AuditLogEntry[]>('/admin/audit-logs', { signal: controller.signal })
      .then((data) => {
        setEntries(data);
        setStatus('ready');
      })
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 403) {
          setStatus('forbidden');
        } else if (!(error instanceof DOMException && error.name === 'AbortError')) {
          setStatus('error');
        }
      });
    return () => controller.abort();
  }, []);

  return (
    <div className="mx-auto flex max-w-container flex-col gap-4 px-4 py-10">
      <h1 className="text-headline-lg">Audit Log</h1>

      {status === 'loading' ? <p className="text-body-sm text-steel-gray">Loading…</p> : null}
      {status === 'forbidden' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">Access denied</Card>
      ) : null}
      {status === 'error' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          Unable to load the audit log.
        </Card>
      ) : null}
      {status === 'ready' ? (
        <Card className="p-6">
          {entries.length === 0 ? (
            <p className="text-body-sm text-steel-gray">No audit entries yet.</p>
          ) : (
            <ul className="flex flex-col gap-2 font-mono text-body-sm">
              {entries.map((entry) => (
                <li key={entry.id}>
                  {entry.timestampUtc} — {entry.action} {entry.entityName}
                </li>
              ))}
            </ul>
          )}
        </Card>
      ) : null}
    </div>
  );
}
