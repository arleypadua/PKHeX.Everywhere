#!/usr/bin/env bash
set -euo pipefail
repo="$(cd "$(dirname "$0")/../../.." && pwd)"
out="${TMPDIR:-/tmp}/pkhex-shell-spike"
port="${PORT:-5180}"

(cd "$repo/src/PKHeX.Web/_js" && npm ci --silent && VITE_FIREBASE_ENABLED=false npm run build >/dev/null)
dotnet publish "$repo/src/PKHeX.Web/PKHeX.Web.csproj" -c Release -o "$out/publish" -nodeReuse:false | tail -1
(cd "$repo/src/PKHeX.Web.Shell" && npm ci --silent && npm run build >/dev/null)

rm -rf "$out/shell"
cp -R "$out/publish/wwwroot" "$out/shell"
rm -f "$out/shell/index.html.br" "$out/shell/index.html.gz"
cp "$repo/src/PKHeX.Web.Shell/dist/index.html" "$out/shell/index.html"
cp -R "$repo/src/PKHeX.Web.Shell/dist/shell-assets" "$out/shell/"

echo "React shell:  http://127.0.0.1:$port/"
echo "Current app:  http://127.0.0.1:$((port + 1))/"
node --input-type=module -e "
import { serve } from '$repo/src/PKHeX.Web.Shell/spike-tools/serve.mjs'
await serve('$out/shell', $port)
await serve('$out/publish/wwwroot', $((port + 1)))
"
