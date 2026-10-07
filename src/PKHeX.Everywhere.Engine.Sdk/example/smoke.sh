#!/usr/bin/env bash
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
sdk="$here/.."
packs="$sdk/.packs"

for readme in "$sdk"/packages/react/README.md "$sdk"/docs/src/content/docs/docs/guides/react.mdx; do
  if ! diff <(awk '/^```tsx$/{on=1; next} /^```$/{if (on) exit} on' "$readme") "$here/src/App.tsx"; then
    echo "The React example in $readme doesn't match example/src/App.tsx." >&2
    exit 1
  fi
done

(cd "$sdk" && npm run build)
rm -rf "$packs" && mkdir -p "$packs"
for package in engine react plugin-sdk; do
  tarball="$(cd "$sdk/packages/$package" && npm pack --silent --pack-destination "$packs")"
  mv "$packs/$tarball" "$packs/$package.tgz"
done

cd "$here"
rm -rf node_modules dist dist-self-hosted
npm install --include=dev
node --input-type=module -e 'import notices from "@pkhex-everywhere/engine/notices.json" with { type: "json" }; if (!notices.length) throw new Error("notices.json is empty")'
npx tsc --noEmit
npx vite build
npx vite build --mode self-hosted --outDir dist-self-hosted
npx playwright install --only-shell chromium
node smoke.mjs
node smoke.mjs self-hosted
