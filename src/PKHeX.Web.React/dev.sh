#!/usr/bin/env bash
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
web="$here/../PKHeX.Web"

if [ ! -f "$web/wwwroot/js/pkhex-web.js.iife.js" ]; then
  (cd "$web/_js" && npm ci --include=dev && VITE_FIREBASE_ENABLED=false npm run build)
fi
[ -d "$here/node_modules" ] || (cd "$here" && npm ci --include=dev)

trap 'kill 0' EXIT
DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=1 dotnet watch --project "$web" --launch-profile http &
cd "$here" && NODE_ENV=development npx vite
