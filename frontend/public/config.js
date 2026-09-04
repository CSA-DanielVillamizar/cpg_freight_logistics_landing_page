// Overwritten at container start by docker-entrypoint.sh from API_BASE_URL / VITE_API_BASE_URL.
// Empty apiBaseUrl -> the client falls back to the Vite build-time value, then to "/api".
window.__CPG_CONFIG__ = { apiBaseUrl: '' };
