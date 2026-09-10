import { useCallback, useEffect, useState } from 'react';
import { payoutsApi } from './payoutsApi';
import type { PayoutHistoryEntry } from './payoutsApi';

export type CarrierPayoutsStatus = 'loading' | 'ready' | 'error';

/** Loads (and lets a caller refresh) the authenticated carrier's payout ledger. */
export function useCarrierPayouts(): {
  status: CarrierPayoutsStatus;
  payouts: PayoutHistoryEntry[];
  refresh: () => void;
} {
  const [status, setStatus] = useState<CarrierPayoutsStatus>('loading');
  const [payouts, setPayouts] = useState<PayoutHistoryEntry[]>([]);
  const [refreshToken, setRefreshToken] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setStatus('loading');
    payoutsApi
      .getPayouts()
      .then((data) => {
        if (!cancelled) {
          setPayouts(data);
          setStatus('ready');
        }
      })
      .catch(() => {
        if (!cancelled) {
          setStatus('error');
        }
      });
    return () => {
      cancelled = true;
    };
  }, [refreshToken]);

  const refresh = useCallback(() => setRefreshToken((token) => token + 1), []);

  return { status, payouts, refresh };
}
