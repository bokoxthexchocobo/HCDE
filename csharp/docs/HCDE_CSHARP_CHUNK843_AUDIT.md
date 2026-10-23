# Chunk 843 — Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **101** (< **150** floor). Prior tally **152** included banned separator padding. Matrix credit **0**.

## Scope

Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **101** (< **150** floor). Prior tally **152** included banned separator padding. Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e2058e0f6555d2be338828e6f66bbb65b883f275`.

## Padding review (2026-10-21)

### Separator uncredit → retire

| | `nativeLoc` |
|--|----------:|
| **Before** (incl. separators) | **152** |
| **After uncredit** (functions only) | **101** |
| **After retire** (matrix) | **0** |

Removed **51** separator lines across **6** `nativeRanges` entries.


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 843 --to 843` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. In-band `fixedvalue` coercion matches `t_variable.cpp`; `gameskill` / `gamemode` read `AuthoritySimulation.Skill` and `GameMode`; `isplayerobj` uses `PlayerPawn` / `PlayerNum`.

## Credit rationale

Separator padding between `SF_*` bands must not count toward `nativeLoc`. After uncredit, band is below floor → retire (behavior may remain off-tally).
