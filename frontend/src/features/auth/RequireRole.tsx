import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import type { UserRole } from '@/shared/api/types';
import { useAuth } from './useAuth';

interface RequireRoleProps {
  role?: UserRole;
  children: ReactNode;
}

/** Route guard: redirects to /login when unauthenticated, home when the role is wrong. */
export function RequireRole({ role, children }: RequireRoleProps): JSX.Element {
  const { isAuthenticated, hasRole } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (role && !hasRole(role)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
