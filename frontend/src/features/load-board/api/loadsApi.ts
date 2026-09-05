import { apiClient } from '@/shared/api/client';
import type { Load, LoadServiceType, LoadStatus } from '../types';

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
};
