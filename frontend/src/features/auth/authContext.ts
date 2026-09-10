import type { AuthenticatedUser, RegisterRequest, UserRole } from '@/shared/api/types';
import { createContext } from 'react';

export interface AuthContextValue {
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  hasRole: (role: UserRole) => boolean;
  login: (email: string, password: string) => Promise<AuthenticatedUser>;
  register: (request: RegisterRequest) => Promise<AuthenticatedUser>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
