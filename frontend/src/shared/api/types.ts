/**
 * Typed mirror of the backend API contracts (SPEC.md section 4 + DTOs).
 * Kept hand-written for Phase 1; a later phase can generate these from the
 * OpenAPI document served at /swagger/v1/swagger.json.
 */

export type ServiceType = 'ColdChain' | 'HeavyHaul' | 'Flatbed' | 'FdotConcrete';

export type UserRole = 'Admin' | 'Carrier' | 'Shipper';

/** POST /api/auth/login request body (SPEC.md US-01). */
export interface LoginRequest {
  email: string;
  password: string;
}

/** POST /api/auth/refresh request body. */
export interface RefreshRequest {
  refreshToken: string;
}

export interface AuthenticatedUser {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
}

/** Login / refresh success response. */
export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken: string;
  user: AuthenticatedUser;
}

export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Won' | 'Lost';

/** POST /api/rates/calculate - request body. */
export interface RateCalculationRequest {
  serviceType: ServiceType;
  originZip: string;
  destinationZip: string;
  weightLbs: number;
  targetTemperatureCelsius?: number;
}

/** POST /api/rates/calculate - 200 response body. */
export interface RateCalculationResponse {
  baseRate: number;
  coldChainSurcharge: number;
  fuelSurcharge: number;
  totalEstimated: number;
  currency: string;
  calculatedAt: string;
}

/** POST /api/leads - request body (SPEC.md US-04). */
export interface CreateLeadRequest {
  companyName: string;
  contactEmail: string;
  contactName?: string;
  phone?: string;
  verticalSlug: string;
  serviceType?: ServiceType;
  cargoDetails?: string;
}

export interface CreateLeadResponse {
  id: string;
  status: LeadStatus;
}

/** RFC 7807 problem document returned by the API on failure. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
