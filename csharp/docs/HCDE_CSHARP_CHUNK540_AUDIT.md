# Chunk 540 — A_Weave, A_LineEffect, A_WolfAttack

## Scope

A_Weave, A_LineEffect, A_WolfAttack

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3484–3536 (53 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LineEffect** 3548–3571 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3584–3680 (97 lines)

**Credited native LOC:** 174 (tally `nativeLoc` 174).

## Managed

`VmActorWeaveWolf.cs`, `VmActorWeaveWolfTests.cs`
Managed LOC (tally): 133.
Conversion commit: `6e9bdcf5c4b11d45f10ff0c9c8d42a871fb492c0`.

## Tests

- `VmActorWeaveWolfTests.AdvanceWeaveIndex_WrapsAt64`
- `VmActorWeaveWolfTests.LineEffectSetsDone_WhenNonRepeating`
- `VmActorWeaveWolfTests.WolfHitChance_ReducesWithDistance`
- `VmActorWeaveWolfTests.WolfDamageAtRange_HalvesTwice`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 540 --to 540` (0 hard / 0 soft).

## Seams

`P_TryMove`, `P_ExecuteSpecial`, `P_CheckSight`, `P_DamageMobj`, and `S_Sound` are not hosted.

## Credit rationale

Credits weave bob math, linedef effect guard, and Wolf3D-style hit chance/damage from `p_actionfunctions.cpp` (3484–3680).
