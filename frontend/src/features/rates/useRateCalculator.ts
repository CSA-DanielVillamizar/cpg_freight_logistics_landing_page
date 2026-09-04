import { useCallback, useState } from 'react';
import { ApiError, apiClient } from '@/shared/api/client';
import type { RateCalculationRequest, RateCalculationResponse } from '@/shared/api/types';

type Status = 'idle' | 'loading' | 'success' | 'error';

interface RateCalculatorState {
  status: Status;
  result: RateCalculationResponse | null;
  fieldErrors: Record<string, string[]>;
  errorMessage: string | null;
  calculate: (request: RateCalculationRequest) => Promise<void>;
  reset: () => void;
}

/** Thin hook around POST /api/rates/calculate (SPEC.md US-02). No pricing logic lives here. */
export function useRateCalculator(): RateCalculatorState {
  const [status, setStatus] = useState<Status>('idle');
  const [result, setResult] = useState<RateCalculationResponse | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const reset = useCallback((): void => {
    setStatus('idle');
    setResult(null);
    setFieldErrors({});
    setErrorMessage(null);
  }, []);

  const calculate = useCallback(async (request: RateCalculationRequest): Promise<void> => {
    setStatus('loading');
    setFieldErrors({});
    setErrorMessage(null);
    try {
      const response = await apiClient.post<RateCalculationResponse>('/rates/calculate', request, {
        anonymous: true,
      });
      setResult(response);
      setStatus('success');
    } catch (error) {
      if (error instanceof ApiError) {
        setFieldErrors(error.problem?.errors ?? {});
        setErrorMessage(error.problem?.detail ?? error.message);
      } else {
        setErrorMessage('Unable to reach the rate service.');
      }
      setStatus('error');
    }
  }, []);

  return { status, result, fieldErrors, errorMessage, calculate, reset };
}
