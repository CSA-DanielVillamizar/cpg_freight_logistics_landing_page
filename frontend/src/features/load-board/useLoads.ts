import { useCallback, useEffect, useRef, useState } from 'react';
import { ApiError } from '@/shared/api/client';
import { loadsApi } from './api/loadsApi';
import type { LoadFilters } from './components/LoadFiltersSidebar';
import type { Load } from './types';

type Status = 'loading' | 'ready' | 'error';

interface UseLoadsResult {
  loads: Load[];
  status: Status;
  errorMessage: string | null;
  refetch: () => void;
}

/** Fetches the board from GET /api/loads, re-querying (debounced) whenever filters change. */
export function useLoads(filters: LoadFilters): UseLoadsResult {
  const [loads, setLoads] = useState<Load[]>([]);
  const [status, setStatus] = useState<Status>('loading');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const filtersRef = useRef(filters);
  filtersRef.current = filters;
  const abortRef = useRef<AbortController | null>(null);

  const refetch = useCallback(() => setReloadKey((key) => key + 1), []);

  const filterSignature = JSON.stringify([
    Array.from(filters.statuses).sort(),
    Array.from(filters.serviceTypes).sort(),
    filters.originQuery.trim().toLowerCase(),
    filters.destinationQuery.trim().toLowerCase(),
  ]);

  useEffect(() => {
    const timer = setTimeout(() => {
      const current = filtersRef.current;
      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;

      setStatus((previous) => (previous === 'ready' ? previous : 'loading'));

      loadsApi
        .list({
          statuses: Array.from(current.statuses),
          serviceTypes: Array.from(current.serviceTypes),
          origin: current.originQuery,
          destination: current.destinationQuery,
        })
        .then((data) => {
          if (controller.signal.aborted) {
            return;
          }
          setLoads(data);
          setStatus('ready');
          setErrorMessage(null);
        })
        .catch((error: unknown) => {
          if (controller.signal.aborted) {
            return;
          }
          setStatus('error');
          setErrorMessage(
            error instanceof ApiError && error.status === 401
              ? 'Your session has expired — sign in again to view the board.'
              : 'Unable to load the board right now.',
          );
        });
    }, 250);

    return () => clearTimeout(timer);
  }, [filterSignature, reloadKey]);

  return { loads, status, errorMessage, refetch };
}
