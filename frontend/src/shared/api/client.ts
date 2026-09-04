import type { ProblemDetails } from './types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

export class ApiError extends Error {
  public readonly status: number;
  public readonly problem: ProblemDetails | null;

  public constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.title ?? `Request failed with status ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }
}

/**
 * Bridge between the HTTP client and the auth store. The store registers itself so the
 * client can attach the bearer token and drive the 401 -> refresh -> retry flow
 * (SPEC.md US-01) without importing React.
 */
export interface AuthBridge {
  getAccessToken: () => string | null;
  refresh: () => Promise<boolean>;
  onAuthLost: () => void;
}

let authBridge: AuthBridge | null = null;

export function registerAuthBridge(bridge: AuthBridge | null): void {
  authBridge = bridge;
}

export interface RequestOptions {
  /** SPEC.md section 2 - required for idempotent write endpoints. */
  idempotencyKey?: string;
  signal?: AbortSignal;
  /** Skip bearer-token attachment and the refresh dance (used by /auth endpoints). */
  anonymous?: boolean;
}

async function request<TResponse>(
  method: 'GET' | 'POST',
  path: string,
  body?: unknown,
  options?: RequestOptions,
  isRetry = false,
): Promise<TResponse> {
  const headers: Record<string, string> = { Accept: 'application/json' };

  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  if (options?.idempotencyKey) {
    headers['Idempotency-Key'] = options.idempotencyKey;
  }

  const token = options?.anonymous ? null : authBridge?.getAccessToken() ?? null;
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const init: RequestInit = { method, headers };
  if (body !== undefined) {
    init.body = JSON.stringify(body);
  }
  if (options?.signal) {
    init.signal = options.signal;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, init);

  if (response.status === 401 && !options?.anonymous && !isRetry && authBridge) {
    const refreshed = await authBridge.refresh();
    if (refreshed) {
      return request<TResponse>(method, path, body, options, true);
    }
    authBridge.onAuthLost();
  }

  if (!response.ok) {
    throw new ApiError(response.status, await safeParseProblem(response));
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

async function safeParseProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return null;
  }
}

export const apiClient = {
  get: <TResponse>(path: string, options?: RequestOptions): Promise<TResponse> =>
    request<TResponse>('GET', path, undefined, options),
  post: <TResponse>(path: string, body: unknown, options?: RequestOptions): Promise<TResponse> =>
    request<TResponse>('POST', path, body, options),
};
