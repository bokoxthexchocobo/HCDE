# ACS pcode + CallFunc matrix re-check — chunk 7 audit

## ACS pcode (by wire index)

Classification already keys on `AcsPcode` opcode integers from `AcsVm.cs` switch cases vs `MapBehaviorBytecodeWalker` decode tables. Regenerated snapshot (unchanged intent):

| Status | Count |
| --- | ---: |
| executed | 273 |
| partial | 10 |
| decoded-only | 79 |
| absent | 12 |

**Absent wire indices (true gaps):** 94 SectorSound, 95 AmbientSound, 96 SoundSequence, 98 SetLineBlocking, 99 SetLineSpecial, 343 SetMugshotState, 384 TranslationRange5, 421 CheckActorFloorTexture, 428 UnmorphActor, 429 GetPlayerInput, 431 PrintBinary, 432 PrintHex.

No additional ACS opcode misclassification found (contrast with pre-chunk-7 VM name-based classifier).

## CallFunc (by native wire index)

**Bug:** `generate_native_managed_matrix.py` treated `case (\d+):` in `AcsCallFunctions.cs` (none) and enum **ordinal** `index + 1`, marking every row partial/absent.

**Fix:** Parse native `EACSFunctions` explicit wire values and mark **executed** when that wire matches a `TryInvoke` `case` const dispatch slot.

Regenerated snapshot:

| Status | Was | Now |
| --- | ---: | ---: |
| executed | 0 | **94** |
| partial | 94 | **0** |
| absent | 44 | **44** |

The 44 absent rows are genuinely undispatched (HUD/CVar/named-ACS stubs, invasion API, Eternity/OpenGL/ZDaemon specials, etc.) — not candidates to “port” under mistaken partial credit.

## Verified

`python3 csharp/tools/generate_native_managed_matrix.py` updates `loc_tally.json` via `sync_loc_tally` and refreshes `native_managed_matrix.json` / matrix markdown.
