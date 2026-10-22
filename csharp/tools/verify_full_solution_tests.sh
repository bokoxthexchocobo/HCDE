#!/bin/bash
# Run HCDE.sln tests and fail unless every test project reports Passed with 0 failures.
# Usage: verify_full_solution_tests.sh [--no-build]
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$REPO_ROOT/csharp"
export DOTNET_MULTILEVEL_LOOKUP=0
DOTNET="${DOTNET:-/usr/lib/dotnet/dotnet}"

NO_BUILD=()
if [[ "${1:-}" == "--no-build" ]]; then
  NO_BUILD=(--no-build)
else
  $DOTNET build HCDE.sln -warnaserror -v q
fi

OUT="$(mktemp)"
set +e
$DOTNET test HCDE.sln -v q "${NO_BUILD[@]}" 2>&1 | tee "$OUT"
TEST_EXIT=$?
set -e

echo "--- per-project summary ---"
rg 'Passed!|Failed!' "$OUT" || true

if [[ "$TEST_EXIT" != 0 ]]; then
  echo "verify_full_solution_tests: dotnet test exited $TEST_EXIT"
  rm -f "$OUT"
  exit "$TEST_EXIT"
fi

if rg -q 'Failed!.*Failed:\s+[1-9]' "$OUT"; then
  echo "verify_full_solution_tests: one or more test projects reported failures"
  rm -f "$OUT"
  exit 1
fi

declare -A EXPECTED=(
  ["HCDE.Master.Tests.dll"]=1
  ["HCDE.Protocol.Tests.dll"]=15
  ["HCDE.Rcon.Tests.dll"]=6
  ["HCDE.Net.Transport.Tests.dll"]=10
  ["HCDE.Net.Pregame.Tests.dll"]=97
  ["HCDE.Net.Core.Tests.dll"]=613
  ["HCDE.MapLoader.Tests.dll"]=1072
  ["HCDE.Server.Tests.dll"]=142
  ["HCDE.Gamedata.Tests.dll"]=826
  ["HCDE.Playsim.Tests.dll"]=18700
  ["HCDE.Client.Tests.dll"]=2786
  ["HCDE.Scripting.Tests.dll"]=1112
  ["HCDE.MigrationMatrix.Tests.dll"]=8
  ["HCDE.ConversionInventory.Tests.dll"]=1
)

MISSING=0
for asm in "${!EXPECTED[@]}"; do
  line="$(rg "Passed!.*${asm}" "$OUT" || true)"
  if [[ -z "$line" ]]; then
    echo "verify_full_solution_tests: missing result for $asm"
    MISSING=1
    continue
  fi
  total="$(echo "$line" | sed -n 's/.*Total:\s*\([0-9][0-9,]*\).*/\1/p' | tr -d ',')"
  min="${EXPECTED[$asm]}"
  if [[ -z "$total" ]] || [[ "$total" -lt "$min" ]]; then
    echo "verify_full_solution_tests: $asm total $total below floor $min"
    MISSING=1
  fi
done

rm -f "$OUT"
if [[ "$MISSING" != 0 ]]; then
  exit 1
fi

echo "verify_full_solution_tests: all ${#EXPECTED[@]} projects passed (0 failed)"
