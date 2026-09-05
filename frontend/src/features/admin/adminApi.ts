import { API_BASE, ApiError, apiClient, currentAccessToken } from '@/shared/api/client';
import type { ComplianceDocumentType, ComplianceStatus } from '@/shared/api/types';

export type ReviewDecision = 'Approve' | 'Reject';

export interface CarrierDocumentView {
  id: string;
  documentType: ComplianceDocumentType;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  status: ComplianceStatus;
  uploadedAtUtc: string;
}

export interface CarrierComplianceView {
  id: string;
  companyName: string;
  dotNumber: string | null;
  mcNumber: string | null;
  status: ComplianceStatus;
  submittedAtUtc: string | null;
  lastReviewedAtUtc: string | null;
  documents: CarrierDocumentView[];
}

export const adminApi = {
  listCarriers: (status?: ComplianceStatus): Promise<CarrierComplianceView[]> =>
    apiClient.get<CarrierComplianceView[]>(
      `/admin/carriers${status ? `?status=${status}` : ''}`,
    ),

  reviewCarrier: (
    carrierId: string,
    decision: ReviewDecision,
    notes?: string,
  ): Promise<CarrierComplianceView> =>
    apiClient.post<CarrierComplianceView>(`/admin/carriers/${carrierId}/review`, {
      decision,
      notes: notes ?? null,
    }),

  /** Fetches the document with the admin JWT and opens it in a new tab (blob URL). */
  openDocument: async (carrierId: string, documentId: string): Promise<void> => {
    const token = currentAccessToken();
    const response = await fetch(
      `${API_BASE}/admin/carriers/${carrierId}/documents/${documentId}/content`,
      { headers: token ? { Authorization: `Bearer ${token}` } : {} },
    );
    if (!response.ok) {
      throw new ApiError(response.status, null);
    }
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank', 'noopener');
    window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
  },
};
