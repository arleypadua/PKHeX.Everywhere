#!/usr/bin/env bash
set -euo pipefail
repo="$(cd "$(dirname "$0")/../../.." && pwd)"
out="${TMPDIR:-/tmp}/pkhex-shell-spike"
port="${PORT:-5180}"

echo "[1/4] Building PKHeX.Web JS assets"
(cd "$repo/src/PKHeX.Web/_js" && npm ci --silent && VITE_FIREBASE_ENABLED=false npm run build >/dev/null 2>&1)
echo "[2/4] Publishing PKHeX.Web in Release (trimming takes a few minutes)"
dotnet publish "$repo/src/PKHeX.Web/PKHeX.Web.csproj" -c Release -o "$out/publish" -nodeReuse:false -v minimal
echo "[3/4] Building the React shell"
(cd "$repo/src/PKHeX.Web.Shell" && npm ci --silent && npm run build >/dev/null 2>&1)
echo "[4/4] Assembling $out/shell"

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
