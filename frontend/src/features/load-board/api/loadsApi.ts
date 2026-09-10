import { apiClient } from '@/shared/api/client';
import type {
  CreateLoadInput,
  Load,
  LoadLocationResponse,
  LoadServiceType,
  LoadStatus,
  TelemetryLogEntryResponse,
} from '../types';

export interface LoadQueryFilters {
  statuses?: LoadStatus[];
  serviceTypes?: LoadServiceType[];
  origin?: string;
  destination?: string;
}

function buildQueryString(filters: LoadQueryFilters): string {
  const params = new URLSearchParams();
  filters.statuses?.forEach((status) => params.append('status', status));
  filters.serviceTypes?.forEach((serviceType) => params.append('serviceType', serviceType));
  if (filters.origin?.trim()) {
    params.set('origin', filters.origin.trim());
  }
  if (filters.destination?.trim()) {
    params.set('destination', filters.destination.trim());
  }
  const query = params.toString();
  return query ? `?${query}` : '';
}

export const loadsApi = {
  list: (filters: LoadQueryFilters = {}): Promise<Load[]> =>
    apiClient.get<Load[]>(`/loads${buildQueryString(filters)}`),

  accept: (loadId: string): Promise<Load> =>
    apiClient.post<Load>(`/loads/${loadId}/accept`, undefined),

  /** POST /api/loads — a shipper posts their own freight; it lands on the board as Available. */
  create: (input: CreateLoadInput): Promise<Load> => apiClient.post<Load>('/loads', input),

  /** GET /api/loads/{id}/location — last-known GPS position (T-SDD Epica 2A). */
  getLocation: (loadId: string): Promise<LoadLocationResponse> =>
    apiClient.get<LoadLocationResponse>(`/loads/${loadId}/location`),

  /** GET /api/loads/{id}/telemetry-history — GPS trail, most recent first (T-SDD Epica 2A). */
  getTelemetryHistory: (loadId: string, take = 200): Promise<TelemetryLogEntryResponse[]> =>
    apiClient.get<TelemetryLogEntryResponse[]>(`/loads/${loadId}/telemetry-history?take=${take}`),
};
