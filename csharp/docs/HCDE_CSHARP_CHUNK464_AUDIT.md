# Chunk 464 — TID hash, FindUniqueTID, GetMissileDamage, Howl

## Scope

TID hash, FindUniqueTID, GetMissileDamage, Howl

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::AddToHash** 3559–3582 (24 lines)
- `src/playsim/p_mobj.cpp` **AActor::RemoveFromHash** 3589–3602 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetTID** 3604–3618 (15 lines)
- `src/playsim/p_mobj.cpp` **FLevelLocals::IsTIDUsed** 3629–3641 (13 lines)
- `src/playsim/p_mobj.cpp` **FLevelLocals::FindUniqueTID** 3654–3700 (47 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetMissileDamage** 3720–3751 (32 lines)
- `src/playsim/p_mobj.cpp` **AActor::Howl** 3753–3767 (15 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmPEnemyLookPlayers.cs`, `VmPEnemyLookPlayersTests.cs`, `VmPMapAimLineAttack.cs`, `VmPMapAimLineAttackTests.cs`, `VmPMobjNightmareHash.cs`, `VmPMobjNightmareHashTests.cs`
Managed LOC (tally): 129.
Conversion commit: `ef628f22f2692eb799cbc51beaa22fb4e5cac25d`.

## Tests

- `VmPMobjNightmareHashTests.TidZeroSkipsHash`
- `VmPMobjNightmareHashTests.LinearSearch_WhenStartNonZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 464 --to 464` (0 hard / 0 soft).

## Seams

`Level->TIDHash`, `VMCall`, and `S_Sound` are not hosted.

## Credit rationale

Credits TID hash maintenance, unique TID search, missile damage roll, and howl action from `p_mobj.cpp` (3481+ band after recognition).
