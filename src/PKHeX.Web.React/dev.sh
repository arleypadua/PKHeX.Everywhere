#!/usr/bin/env bash
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
web="$here/../PKHeX.Web"
state="$here/.dev.pid"

running_pids() {
  [ -f "$state" ] || return 1
  local pids
  pids="$(cat "$state")"
  kill -0 "${pids%% *}" 2>/dev/null || return 1
  echo "$pids"
}

kill_groups() {
  local pgid
  for pgid in "$@"; do kill -INT -- "-$pgid" 2>/dev/null || true; done
  for _ in 1 2 3 4 5 6 7 8 9 10; do
    local alive=0
    for pgid in "$@"; do kill -0 -- "-$pgid" 2>/dev/null && alive=1; done
    [ "$alive" = 0 ] && return 0
    sleep 0.5
  done
  # dotnet watch can ignore INT and TERM, so it gets KILL.
  for pgid in "$@"; do kill -KILL -- "-$pgid" 2>/dev/null || true; done
}

if [ "${1:-}" = "stop" ]; then
  if pids="$(running_pids)"; then
    script="${pids%% *}"
    kill -TERM "$script" 2>/dev/null || true
    for _ in 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20; do
      kill -0 "$script" 2>/dev/null || break
      sleep 0.5
    done
    # shellcheck disable=SC2086
    kill_groups ${pids#* }
  fi
  rm -f "$state"
  echo "dev servers stopped"
  exit 0
fi

if pids="$(running_pids)"; then
  echo "dev servers already running (pid ${pids%% *}). Run '$0 stop' first." >&2
  exit 1
fi
for port in 5062 5173; do
  if owner="$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null)"; then
    echo "port $port is in use by pid $owner, probably another checkout's dev server." >&2
    exit 1
  fi
done

if [ ! -f "$web/wwwroot/js/pkhex-web.js.iife.js" ]; then
  (cd "$web/_js" && npm ci --include=dev && VITE_FIREBASE_ENABLED=false npm run build)
fi
[ -d "$here/node_modules" ] || (cd "$here" && npm ci --include=dev)

# Each child gets its own process group so cleanup reaches everything it spawns.
set -m
MSBUILDDISABLENODEREUSE=1 DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=1 dotnet watch --project "$web" --launch-profile http &
watch_pid=$!
(cd "$here" && NODE_ENV=development exec npx vite) &
vite_pid=$!

# Cleans up when this script is killed with KILL, which no trap can catch.
(
  trap '' INT TERM HUP
  while kill -0 $$ 2>/dev/null; do sleep 2; done
  kill_groups "$watch_pid" "$vite_pid"
  rm -f "$state"
) </dev/null >/dev/null 2>&1 &
watchdog_pid=$!
set +m

echo "$$ $watch_pid $vite_pid $watchdog_pid" > "$state"

cleanup() {
  trap - EXIT INT TERM HUP
  kill -KILL -- "-$watchdog_pid" 2>/dev/null || true
  kill_groups "$watch_pid" "$vite_pid"
  rm -f "$state"
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM HUP

# Bash defers traps while a foreground command runs but not during wait, so the loop waits on a sleep.
while kill -0 "$watch_pid" 2>/dev/null && kill -0 "$vite_pid" 2>/dev/null; do
  sleep 1 &
  wait $! 2>/dev/null || true
done
