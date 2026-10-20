# Chunk 450 — Remaining p_spec.cpp bands including P_PredictLine moved from chunk438

## Scope

Remaining p_spec.cpp bands including P_PredictLine moved from chunk438

## Native ranges

- `src/playsim/p_spec.cpp` **p_spec.cpp 1-73** 1–73 (73 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 108-118** 108–118 (11 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 196-196** 196–196 (1 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 218-218** 218–218 (1 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 357-405** 357–405 (49 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 478-495** 478–495 (18 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 501-508** 501–508 (8 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 532-532** 532–532 (1 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 586-592** 586–592 (7 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 595-595** 595–595 (1 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 628-628** 628–628 (1 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 638-644** 638–644 (7 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 676-684** 676–684 (9 lines)
- `src/playsim/p_spec.cpp` **p_spec.cpp 697-750** 697–750 (54 lines)

**Credited native LOC:** 241 (tally `nativeLoc` 241).

## Managed

`VmPSpecActorSpecialFinish.cs`, `VmPSpecPredictLine.cs`, `VmPSpecPredictLineTests.cs`, `VmPSpecRemainderBands.cs`, `VmPSpecRemainderBandsTests.cs`, `VmPSpecTestActivateLine.cs`, `VmPSpecTestActivateLineTests.cs`
Managed LOC (tally): 121.
Conversion commit: `ae9b1c78278c742cc80d1a7c081b70dc43f28d3e`.

## Tests

- `VmPSpecRemainderBandsTests.NegativeDamageHealTick`
- `VmPSpecPredictLineTests.SpecialIsPredictable_TeleportLine`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 450 --to 450` (0 hard / 0 soft).

## Seams

Hosted playsim iterators, terrain tables, and VM calls are not hosted.

## Credit rationale

Credits EXTERN cvars, action stubs, P_PredictLine predict path, negative-sector heal, sector-damage and secret headers, and timelimit/friction comment bands from `p_spec.cpp`.
