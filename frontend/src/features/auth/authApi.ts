import { apiClient } from '@/shared/api/client';
import type {
  AuthResponse,
  LoginRequest,
  MyProfileResponse,
  RefreshRequest,
  RegisterRequest,
} from '@/shared/api/types';

export const authApi = {
  login: (body: LoginRequest): Promise<AuthResponse> =>
    apiClient.post<AuthResponse>('/auth/login', body, { anonymous: true }),

  refresh: (body: RefreshRequest): Promise<AuthResponse> =>
    apiClient.post<AuthResponse>('/auth/refresh', body, { anonymous: true }),

  register: (body: RegisterRequest): Promise<AuthResponse> =>
    apiClient.post<AuthResponse>('/auth/register', body, { anonymous: true }),

  getMe: (): Promise<MyProfileResponse> => apiClient.get<MyProfileResponse>('/me'),
};
