import { apiClient } from '@/shared/api/client';

export type DisbursementStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

/** A single row of GET /api/carriers/payouts (T-SDD Epica 2B). */
export interface PayoutHistoryEntry {
    disbursementId: string;
    invoiceId: string;
    loadReference: string;
    grossAmountUsd: number;
    cpgMarginAmountUsd: number;
    agentCommissionAmountUsd: number;
    quickPayFeeAmountUsd: number;
    carrierNetAmountUsd: number;
    quickPayRequested: boolean;
    status: DisbursementStatus;
    failureReason: string | null;
    processedAtUtc: string | null;
    createdAtUtc: string;
}

export interface StripeConnectOnboardingResponse {
    accountLinkUrl: string;
}

export const payoutsApi = {
    /** GET /api/carriers/payouts — the authenticated carrier's payout ledger, newest first. */
    getPayouts: (): Promise<PayoutHistoryEntry[]> => apiClient.get<PayoutHistoryEntry[]>('/carriers/payouts'),

    /** POST /api/carriers/stripe-connect — starts (or resumes) Stripe Connect onboarding. */
    connectStripe: (): Promise<StripeConnectOnboardingResponse> =>
        apiClient.post<StripeConnectOnboardingResponse>('/carriers/stripe-connect', undefined),

    /** POST /api/invoices/{id}/quick-pay — opt into immediate settlement for a fee. */
    requestQuickPay: (invoiceId: string): Promise<void> =>
        apiClient.post<void>(`/invoices/${invoiceId}/quick-pay`, undefined),
};
