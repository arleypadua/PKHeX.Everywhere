#!/usr/bin/env bash
set -euo pipefail
repo="$(cd "$(dirname "$0")/../../.." && pwd)"

if [ ! -f "$repo/src/PKHeX.Web/wwwroot/js/pkhex-web.js.iife.js" ]; then
  (cd "$repo/src/PKHeX.Web/_js" && npm ci --silent && VITE_FIREBASE_ENABLED=false npm run build >/dev/null 2>&1)
fi
[ -d "$repo/src/PKHeX.Web.Shell/node_modules" ] || (cd "$repo/src/PKHeX.Web.Shell" && npm ci --silent)

trap 'kill 0' EXIT
DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=1 dotnet watch --project "$repo/src/PKHeX.Web" --launch-profile http &
(cd "$repo/src/PKHeX.Web.Shell" && npm run dev)
