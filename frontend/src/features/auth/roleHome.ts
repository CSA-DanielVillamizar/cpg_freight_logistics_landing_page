import type { UserRole } from '@/shared/api/types';

/** Default landing route per role once the visitor is authenticated. */
export const ROLE_HOME: Record<UserRole, string> = {
  Admin: '/admin/carriers',
  Carrier: '/carrier',
  Shipper: '/shipper/dashboard',
  Agent: '/agent',
};
