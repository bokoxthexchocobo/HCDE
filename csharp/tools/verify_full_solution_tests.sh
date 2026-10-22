#!/bin/bash
# Run HCDE.sln tests and fail unless every test project reports Passed with 0 failures.
# Usage: verify_full_solution_tests.sh [--no-build]
# Do not pipe this script (e.g. to tail); that masks the exit status of dotnet test.
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$REPO_ROOT/csharp"
export DOTNET_MULTILEVEL_LOOKUP=0
DOTNET="${DOTNET:-/usr/lib/dotnet/dotnet}"

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
EXPECTED_COUNT="${#EXPECTED[@]}"

NO_BUILD=()
if [[ "${1:-}" == "--no-build" ]]; then
  NO_BUILD=(--no-build)
else
  BUILD_OUT="$(mktemp)"
  set +e
  $DOTNET build HCDE.sln -warnaserror -v q 2>&1 | tee "$BUILD_OUT"
  BUILD_EXIT="${PIPESTATUS[0]}"
  set -e
  BUILD_ERRORS="$(rg -c ': error CS' "$BUILD_OUT" 2>/dev/null || true)"
  BUILD_ERRORS="${BUILD_ERRORS:-0}"
  if [[ "$BUILD_EXIT" != 0 ]] || [[ "$BUILD_ERRORS" != 0 ]] || rg -q 'Build FAILED' "$BUILD_OUT"; then
    echo "verify_full_solution_tests: build failed (exit=$BUILD_EXIT, cs_errors=$BUILD_ERRORS)"
    rg ': error CS' "$BUILD_OUT" | head -20 || true
    rm -f "$BUILD_OUT"
    exit 1
  fi
  rm -f "$BUILD_OUT"
  echo "verify_full_solution_tests: build ok (0 CS errors)"
fi

OUT="$(mktemp)"
set +e
$DOTNET test HCDE.sln -v q "${NO_BUILD[@]}" 2>&1 | tee "$OUT"
TEST_EXIT="${PIPESTATUS[0]}"
set -e

echo "--- per-project summary ---"
rg '^(Passed|Failed)!' "$OUT" || true

# Only trust VSTest per-assembly summary lines (leading Passed!/Failed!). Log noise can
# contain the substring "Failed!" and must not trip the gate.
if rg -q '^Failed!.*Failed:\s+[1-9][0-9,]*' "$OUT"; then
  echo "verify_full_solution_tests: one or more test projects reported test failures"
  rg '^Failed!' "$OUT" || true
  rm -f "$OUT"
  exit 1
fi

if rg -q '^Test Run Failed\.' "$OUT" || rg -q 'Test Run Aborted\.' "$OUT"; then
  echo "verify_full_solution_tests: solution test run reported failure"
  rm -f "$OUT"
  exit 1
fi

REPORTED_PASSED=0
MISSING=0
echo "--- parsed per-project counts ---"
for asm in $(printf '%s\n' "${!EXPECTED[@]}" | sort); do
  failed_line="$(rg -m1 "^Failed!.*${asm}" "$OUT" 2>/dev/null || true)"
  if [[ -n "$failed_line" ]]; then
    failed_n="$(echo "$failed_line" | sed -n 's/^Failed!.*Failed:\s*\([0-9][0-9,]*\).*/\1/p' | tr -d ',')"
    echo "FAIL $asm (assembly reported Failed!; failed=$failed_n)"
    MISSING=1
    continue
  fi
  line="$(rg -m1 "^Passed!.*${asm}" "$OUT" 2>/dev/null || true)"
  if [[ -z "$line" ]]; then
    echo "MISSING $asm (no Passed! summary line)"
    MISSING=1
    continue
  fi
  failed_n="$(echo "$line" | sed -n 's/^Passed!.*Failed:\s*\([0-9][0-9,]*\).*/\1/p' | tr -d ',')"
  passed_n="$(echo "$line" | sed -n 's/^Passed!.*Passed:\s*\([0-9][0-9,]*\).*/\1/p' | tr -d ',')"
  total_n="$(echo "$line" | sed -n 's/^Passed!.*Total:\s*\([0-9][0-9,]*\).*/\1/p' | tr -d ',')"
  min="${EXPECTED[$asm]}"
  if [[ -z "$failed_n" ]] || [[ "$failed_n" != 0 ]]; then
    echo "FAIL $asm failed=$failed_n (expected 0)"
    MISSING=1
    continue
  fi
  if [[ -z "$total_n" ]] || [[ "$total_n" -lt "$min" ]]; then
    echo "FAIL $asm total=$total_n (floor $min)"
    MISSING=1
    continue
  fi
  REPORTED_PASSED=$((REPORTED_PASSED + 1))
  echo "OK   $asm passed=$passed_n failed=0 total=$total_n (floor $min)"
done

rm -f "$OUT"

if [[ "$REPORTED_PASSED" -lt "$EXPECTED_COUNT" ]]; then
  echo "verify_full_solution_tests: only $REPORTED_PASSED/$EXPECTED_COUNT projects parsed as OK"
  exit 1
fi

if [[ "$TEST_EXIT" != 0 ]]; then
  echo "verify_full_solution_tests: dotnet test exited $TEST_EXIT (pipeline; see PIPESTATUS)"
  exit 1
fi

echo "verify_full_solution_tests: all $EXPECTED_COUNT projects passed (0 failed); build_errors=0"
