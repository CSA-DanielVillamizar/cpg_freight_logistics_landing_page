/**
 * Runtime configuration.
 *
 * Vite bakes `import.meta.env.*` at build time. For the production container we instead read
 * `window.__CPG_CONFIG__`, which the nginx entrypoint (`docker-entrypoint.sh`) writes into
 * `/config.js` from the `API_BASE_URL` / `VITE_API_BASE_URL` container environment variable.
 * Precedence: runtime config.js -> Vite build-time env -> `/api` (dev proxy default).
 */
interface CpgRuntimeConfig {
  apiBaseUrl?: string;
}

declare global {
  interface Window {
    __CPG_CONFIG__?: CpgRuntimeConfig;
  }
}

export function resolveApiBaseUrl(): string {
  const runtime = typeof window !== 'undefined' ? window.__CPG_CONFIG__?.apiBaseUrl : undefined;
  if (runtime && runtime.trim() !== '') {
    return runtime.trim();
  }

  const buildTime = import.meta.env.VITE_API_BASE_URL;
  if (buildTime && buildTime.trim() !== '') {
    return buildTime.trim();
  }

  return '/api';
}
