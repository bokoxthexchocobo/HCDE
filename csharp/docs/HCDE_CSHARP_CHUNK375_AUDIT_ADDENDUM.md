# Chunk 375 — addendum (coverage-anchor removal)

## What changed

- **Removed** `VmMapLoaderSegLoadPort.cs` — a one-line forwarding surface added only to inflate `conversionManagedCommit` `csharp/src` additions past the 50% `nativeLoc` gate. It did not port new native behavior.
- **Removed** `VmMapLoaderLoadSegsPolicy.cs` — bulk helpers added in follow-up “coverage” commits without named tests; `DescribeBspFailure` lives on `VmMapLoaderLoadSegs` next to `FormatBadSegMessage`.
- **Trimmed** credited `nativeRanges` from whole-function spans (802–975, 985–1047) to **six bands** that match the existing named tests (60 native LOC total).

## Policy going forward

Do **not** add wrapper/forwarding types to satisfy `thin_managed_coverage`. If the honest port is thin, **credit fewer native lines** (narrow ranges) instead of padding managed LOC.

## Chunks 364–374 scan

No other `*Port` / forwarding-anchor files in this band. Chunk 370’s `PlanPolygonDraw` helpers are covered by `VmAutomapDrawSubsectorsTests` (not metric padding).

## Verified

`reaudit_chunks.py audit --from 364 --to 375` (0 hard / 0 soft); full `dotnet build` / `dotnet test` after this addendum commit.
