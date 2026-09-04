import { apiClient } from '@/shared/api/client';
import type { AuthResponse, LoginRequest, RefreshRequest } from '@/shared/api/types';

export const authApi = {
  login: (body: LoginRequest): Promise<AuthResponse> =>
    apiClient.post<AuthResponse>('/auth/login', body, { anonymous: true }),

  refresh: (body: RefreshRequest): Promise<AuthResponse> =>
    apiClient.post<AuthResponse>('/auth/refresh', body, { anonymous: true }),
};
