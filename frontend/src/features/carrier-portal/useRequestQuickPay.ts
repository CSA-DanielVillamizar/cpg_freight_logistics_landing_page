import { useState } from 'react';
import { payoutsApi } from './payoutsApi';

/** Requests Quick Pay for a single invoice, tracking which one is in flight. */
export function useRequestQuickPay(): {
  requestQuickPay: (invoiceId: string) => Promise<boolean>;
  submittingInvoiceId: string | null;
} {
  const [submittingInvoiceId, setSubmittingInvoiceId] = useState<string | null>(null);

  async function requestQuickPay(invoiceId: string): Promise<boolean> {
    setSubmittingInvoiceId(invoiceId);
    try {
      await payoutsApi.requestQuickPay(invoiceId);
      return true;
    } catch {
      return false;
    } finally {
      setSubmittingInvoiceId(null);
    }
  }

  return { requestQuickPay, submittingInvoiceId };
}
