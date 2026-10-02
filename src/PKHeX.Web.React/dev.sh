#!/usr/bin/env bash
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
state="$here/.dev.pid"
port=5173

running_pid() {
  [ -f "$state" ] || return 1
  local pid
  pid="$(cat "$state")"
  kill -0 "$pid" 2>/dev/null || return 1
  echo "$pid"
}

if [ "${1:-}" = "stop" ]; then
  if pid="$(running_pid)"; then
    kill -INT -- "-$pid" 2>/dev/null || true
    for _ in 1 2 3 4 5 6 7 8 9 10; do
      kill -0 -- "-$pid" 2>/dev/null || break
      sleep 0.5
    done
    kill -KILL -- "-$pid" 2>/dev/null || true
  fi
  rm -f "$state"
  echo "dev server stopped"
  exit 0
fi

if pid="$(running_pid)"; then
  echo "dev server already running (pid $pid). Run '$0 stop' first." >&2
  exit 1
fi
if owner="$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null)"; then
  echo "port $port is in use by pid $owner, probably another checkout's dev server." >&2
  exit 1
fi

[ -d "$here/node_modules" ] || (cd "$here" && npm ci --include=dev)
MSBUILDDISABLENODEREUSE=1 dotnet build "$here/../PKHeX.Everywhere.Engine.Host"

# Vite gets its own process group so stop reaches everything it spawns.
set -m
(cd "$here" && NODE_ENV=development exec npx vite) &
vite_pid=$!

# Cleans up when this script is killed with KILL, which no trap can catch.
(
  trap '' INT TERM HUP
  while kill -0 $$ 2>/dev/null; do sleep 2; done
  kill -INT -- "-$vite_pid" 2>/dev/null || true
  rm -f "$state"
) </dev/null >/dev/null 2>&1 &
watchdog_pid=$!
set +m
echo "$vite_pid" > "$state"

cleanup() {
  trap - EXIT INT TERM HUP
  kill -KILL -- "-$watchdog_pid" 2>/dev/null || true
  kill -INT -- "-$vite_pid" 2>/dev/null || true
  rm -f "$state"
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM HUP

# Bash defers traps while a foreground command runs but not during wait, so the loop waits on a sleep.
while kill -0 "$vite_pid" 2>/dev/null; do
  sleep 1 &
  wait $! 2>/dev/null || true
done
