# Chunk 213 — InitActorNumsFromMapinfo resolve and P_GetSpawnableType. Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Scope

InitActorNumsFromMapinfo resolve and P_GetSpawnableType. Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `31cfab951902c55c824b92303a469b0032b36c7b`.

## Padding review (2026-10-21)



### Out-of-body uncredit (213)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **54** |
| **Out-of-body removed** | **0** |
| **Honest** | **54** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 213 --to 213` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
