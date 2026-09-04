#!/bin/sh
# Generates the SPA runtime configuration (window.__CPG_CONFIG__) from the container
# environment. Runs automatically as an nginx /docker-entrypoint.d hook on every start.
# API_BASE_URL wins; VITE_API_BASE_URL is accepted as an alias.
set -e

API_URL="${API_BASE_URL:-${VITE_API_BASE_URL:-}}"
CONFIG_PATH="${CPG_CONFIG_PATH:-/usr/share/nginx/html/config.js}"

cat > "$CONFIG_PATH" <<EOF
window.__CPG_CONFIG__ = { apiBaseUrl: "${API_URL}" };
EOF

echo "cpg: runtime config written -> apiBaseUrl=\"${API_URL}\""
