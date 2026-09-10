/**
 * Typed mirror of the backend API contracts (SPEC.md section 4 + DTOs).
 * Kept hand-written for Phase 1; a later phase can generate these from the
 * OpenAPI document served at /swagger/v1/swagger.json.
 */

export type ServiceType = 'ColdChain' | 'HeavyHaul' | 'Flatbed' | 'FdotConcrete';

export type UserRole = 'Admin' | 'Carrier' | 'Shipper' | 'Agent';

/** POST /api/auth/login request body (SPEC.md US-01). */
export interface LoginRequest {
  email: string;
  password: string;
}

/** POST /api/auth/refresh request body. */
export interface RefreshRequest {
  refreshToken: string;
}

/** POST /api/auth/register request body — the Tri-Sign-Up flow (T-SDD Epica 1). */
export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  role: UserRole;
  companyName?: string;
  phoneNumber?: string;
  /** Carrier-only. */
  dotNumber?: string;
  /** Carrier-only. */
  mcNumber?: string;
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

/** Confidence signal for `RateCalculationResponse.suggestedRateUsd` (T-SDD Epica 5). */
export type RateConfidence = 'Low' | 'Medium' | 'High';

/** POST /api/rates/calculate - 200 response body. */
export interface RateCalculationResponse {
  baseRate: number;
  coldChainSurcharge: number;
  fuelSurcharge: number;
  totalEstimated: number;
  currency: string;
  calculatedAt: string;
  /** Market-informed suggestion for this lane/service/season; falls back to `totalEstimated`. */
  suggestedRateUsd: number | null;
  suggestedRateConfidence: RateConfidence;
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

export type ComplianceStatus = 'PendingCompliance' | 'UnderReview' | 'Verified' | 'Rejected';

export type ComplianceDocumentType =
  | 'CertificateOfInsurance'
  | 'GeneralLiabilityInsurance'
  | 'FdotPermit'
  | 'OperatingAuthority'
  | 'W9';

export interface ComplianceDocumentSummary {
  id: string;
  documentType: ComplianceDocumentType;
  originalFileName: string;
  sizeBytes: number;
  status: ComplianceStatus;
  uploadedAtUtc: string;
}

/** GET /api/compliance response (SPEC.md US-03 portal). */
export interface ComplianceStatusResponse {
  carrierId: string;
  companyName: string;
  status: ComplianceStatus;
  documents: ComplianceDocumentSummary[];
}

/** POST /api/compliance/upload 202 response. */
export interface UploadComplianceDocumentResult {
  carrierId: string;
  documentId: string;
  status: ComplianceStatus;
  blobUri: string;
}

export type AgentStatus = 'PendingActivation' | 'Active' | 'Suspended';

export interface CarrierProfileSummary {
  carrierId: string;
  companyName: string;
  complianceStatus: ComplianceStatus;
}

export interface AgentProfileSummary {
  agentId: string;
  companyName: string;
  status: AgentStatus;
  commissionRatePercent: number;
}

/** GET /api/me response (T-SDD Epica 1). */
export interface MyProfileResponse {
  userId: string;
  email: string;
  fullName: string;
  role: UserRole;
  companyName?: string;
  phoneNumber?: string;
  carrier?: CarrierProfileSummary;
  agent?: AgentProfileSummary;
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
