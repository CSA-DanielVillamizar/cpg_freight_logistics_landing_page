import { useCallback, useRef, useState } from 'react';
import { ApiError, apiClient } from '@/shared/api/client';
import type { RateCalculationRequest, RateCalculationResponse } from '@/shared/api/types';

type Status = 'idle' | 'loading' | 'success' | 'error';

/** After this long the request is almost certainly waiting on a cold container. */
const SLOW_AFTER_MS = 4000;
/** Hard ceiling so the button never hangs on "Calculating…" forever. */
const TIMEOUT_MS = 40000;

interface RateCalculatorState {
  status: Status;
  /** True while a slow request is in flight (the API is likely waking up). */
  slow: boolean;
  result: RateCalculationResponse | null;
  fieldErrors: Record<string, string[]>;
  errorMessage: string | null;
  calculate: (request: RateCalculationRequest) => Promise<void>;
  reset: () => void;
}

/** Thin hook around POST /api/rates/calculate (SPEC.md US-02). No pricing logic lives here. */
export function useRateCalculator(): RateCalculatorState {
  const [status, setStatus] = useState<Status>('idle');
  const [slow, setSlow] = useState(false);
  const [result, setResult] = useState<RateCalculationResponse | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const inFlight = useRef<AbortController | null>(null);

  const reset = useCallback((): void => {
    inFlight.current?.abort();
    setStatus('idle');
    setSlow(false);
    setResult(null);
    setFieldErrors({});
    setErrorMessage(null);
  }, []);

  const calculate = useCallback(async (request: RateCalculationRequest): Promise<void> => {
    inFlight.current?.abort();
    const controller = new AbortController();
    inFlight.current = controller;

    setStatus('loading');
    setSlow(false);
    setFieldErrors({});
    setErrorMessage(null);

    const slowTimer = window.setTimeout(() => setSlow(true), SLOW_AFTER_MS);
    const timeoutTimer = window.setTimeout(() => controller.abort(), TIMEOUT_MS);

    try {
      const response = await apiClient.post<RateCalculationResponse>('/rates/calculate', request, {
        anonymous: true,
        signal: controller.signal,
      });
      setResult(response);
      setStatus('success');
    } catch (error) {
      if (error instanceof ApiError) {
        setFieldErrors(error.problem?.errors ?? {});
        setErrorMessage(error.problem?.detail ?? error.message);
      } else if (error instanceof DOMException && error.name === 'AbortError') {
        setErrorMessage(
          'The rate service did not respond in time — it may be waking up. Try again in a few seconds.',
        );
      } else {
        setErrorMessage('Could not reach the rate service. Check your connection and try again.');
      }
      setStatus('error');
    } finally {
      window.clearTimeout(slowTimer);
      window.clearTimeout(timeoutTimer);
      setSlow(false);
      if (inFlight.current === controller) {
        inFlight.current = null;
      }
    }
  }, []);

  return { status, slow, result, fieldErrors, errorMessage, calculate, reset };
}
