# Chunk 250 — Re-audit: RecordIssue sample cap only.

## Scope

Re-audit: RecordIssue sample cap only.

## Native ranges

- `src/gamedata/hcde_edf.cpp` **RecordIssue** 216–230 (15 lines)

**Credited native LOC:** 15 (tally `nativeLoc` 15).

## Managed

`HcdeEdfIssueTracker.cs`, `HcdeEdfIssueTrackerTests.cs`
Managed LOC (tally): 21.
Conversion commit: `311d9290ee3938d083b2fffc685eaf9c710f9fd7`.

## Tests

- `HcdeEdfIssueTrackerTests.TryRecordIssue_CapsAtTwenty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 250 --to 250` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
