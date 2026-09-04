import { useCallback, useState } from 'react';
import { ApiError, apiClient } from '@/shared/api/client';
import type { RateCalculationRequest, RateCalculationResponse } from '@/shared/api/types';

type Status = 'idle' | 'loading' | 'success' | 'error';

interface RateCalculatorState {
  status: Status;
  result: RateCalculationResponse | null;
  errorMessage: string | null;
  calculate: (request: RateCalculationRequest) => Promise<void>;
}

/** Thin hook around POST /api/rates/calculate (SPEC.md US-02). No pricing logic lives here. */
export function useRateCalculator(): RateCalculatorState {
  const [status, setStatus] = useState<Status>('idle');
  const [result, setResult] = useState<RateCalculationResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const calculate = useCallback(async (request: RateCalculationRequest): Promise<void> => {
    setStatus('loading');
    setErrorMessage(null);
    try {
      const response = await apiClient.post<RateCalculationResponse>('/rates/calculate', request);
      setResult(response);
      setStatus('success');
    } catch (error) {
      const message =
        error instanceof ApiError
          ? (error.problem?.detail ?? error.message)
          : 'Unable to reach the rate service.';
      setErrorMessage(message);
      setStatus('error');
    }
  }, []);

  return { status, result, errorMessage, calculate };
}
