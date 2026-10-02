#!/usr/bin/env bash
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
sdk="$here/.."
packs="$sdk/.packs"

(cd "$sdk" && npm run build)
rm -rf "$packs" && mkdir -p "$packs"
for package in engine react plugin-sdk; do
  tarball="$(cd "$sdk/packages/$package" && npm pack --silent --pack-destination "$packs")"
  mv "$packs/$tarball" "$packs/$package.tgz"
done

cd "$here"
rm -rf node_modules dist
npm install --include=dev
npx tsc --noEmit
npx vite build
npx playwright install --only-shell chromium
node smoke.mjs
