import { API_BASE, ApiError, apiClient, currentAccessToken } from '@/shared/api/client';
import type { LoadServiceType } from '@/features/load-board/types';

export type ShipperLoadStatus = 'Dispatched' | 'InTransit' | 'Delivered';

export interface ShipperLoadView {
  id: string;
  reference: string;
  status: ShipperLoadStatus;
  serviceType: LoadServiceType;
  equipmentType: string;
  originCity: string;
  originState: string;
  destinationCity: string;
  destinationState: string;
  distanceMiles: number;
  weightLbs: number;
  rateUsd: number;
  pickupAtUtc: string;
  deliveryAtUtc: string;
  carrierName: string | null;
  podAvailable: boolean;
}

export interface ShipperLoadMetrics {
  activeCount: number;
  inTransitCount: number;
  deliveredCount: number;
  activeSpendUsd: number;
}

export interface ShipperLoadsResponse {
  active: ShipperLoadView[];
  history: ShipperLoadView[];
  metrics: ShipperLoadMetrics;
}

export type InvoiceStatus = 'Draft' | 'Pending' | 'Paid' | 'Overdue';

export interface ShipperInvoiceView {
  id: string;
  reference: string;
  loadReference: string;
  amountUsd: number;
  status: InvoiceStatus;
  issuedAtUtc: string;
  dueDate: string;
  paidAtUtc: string | null;
  payable: boolean;
}

export interface ShipperInvoicesResponse {
  invoices: ShipperInvoiceView[];
  totalOutstandingUsd: number;
  overdueCount: number;
}

export const shipperApi = {
  getLoads: (): Promise<ShipperLoadsResponse> => apiClient.get<ShipperLoadsResponse>('/shipper/loads'),

  getInvoices: (): Promise<ShipperInvoicesResponse> =>
    apiClient.get<ShipperInvoicesResponse>('/shipper/invoices'),

  /** Opens a Stripe Checkout session and returns the URL to redirect the browser to. */
  payInvoice: (invoiceId: string): Promise<{ checkoutUrl: string }> =>
    apiClient.post<{ checkoutUrl: string }>(`/shipper/invoices/${invoiceId}/pay`, undefined),

  /** Fetches the POD PDF with the shipper JWT and opens it in a new tab (blob URL). */
  openPod: async (loadId: string): Promise<void> => {
    const token = currentAccessToken();
    const response = await fetch(`${API_BASE}/shipper/loads/${loadId}/pod`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    if (!response.ok) {
      throw new ApiError(response.status, null);
    }
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank', 'noopener');
    window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
  },
};
