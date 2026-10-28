# Chunk 763 — Thinker list ops, destroy/save/load, tick/profile, and client-side collection tick (`dthinker.cpp` 210–307, 452–795, 567–699; excludes chunk762 71–366 and SerializeThinkers 374+). Retired (2026-10-24): `--band` hull overlaps existing credit (p-saveg-acs-archive-chunk10 src/playsim/dthinker.cpp 374-443).

## Scope

Thinker list ops, destroy/save/load, tick/profile, and client-side collection tick (`dthinker.cpp` 210–307, 452–795, 567–699; excludes chunk762 71–366 and SerializeThinkers 374+). Retired (2026-10-24): `--band` hull overlaps existing credit (p-saveg-acs-archive-chunk10 src/playsim/dthinker.cpp 374-443).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c4f5b20d13f25202d427b4a019b28843e8d1a77b`.

## Padding review (2026-10-21)

| Managed | Native (`dthinker.cpp`) |
|---------|-------------------------|
| `RecreateDynamicLights` | **210–223** |
| `RunClientSideThinkers` | **225–307** |
| `AddTail` / `GetHead` / `GetTail` / `IsEmpty` | **452–559** |
| `FirstThinker` / `MarkRoots` | **483–517** |
| `DestroyThinkers` / `DoDestroyThinkers` | **567–629** |
| `SaveList` / `RemoveTravellers` / `OnLoad` | **637–699** |
| `TickThinkers` / `ProfileThinkers` | **707–795** |

### Out-of-body uncredit (763)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **358** |
| **Out-of-body removed** | **0** |
| **Honest** | **358** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 763 --to 763` (0 hard / 0 soft).

## Seams

Sentinel/GC, `DThinker::Remove`, iterators, `SetDynamicLights`, `P_RunEffect`, level lights, and VM hooks are injected.

## Credit rationale

Circular list mechanics plus client prediction tick path (fresh drain, dynamic lights, light list tick).
