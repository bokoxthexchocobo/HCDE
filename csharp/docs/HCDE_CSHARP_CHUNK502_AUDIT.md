# Chunk 502 — GetTag, counters, damage factor, translation, RestoreSpecialPosition, PrintMiscActorInfo

## Scope

GetTag, counters, damage factor, translation, RestoreSpecialPosition, PrintMiscActorInfo

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::GetTag** 8811–8833 (23 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetTag** 8835–8839 (5 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetCharacterName** 8841–8853 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::ClearCounters** 8855–8874 (20 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetModifiedDamage** 8876–8890 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::ApplyDamageFactor** 8892–8900 (9 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetTranslation** 8902–8918 (17 lines)
- `src/playsim/p_mobj.cpp` **AActor::RestoreSpecialPosition** 8927–8974 (48 lines)
- `src/playsim/p_mobj.cpp` **FDropItem** 8982–8985 (4 lines)
- `src/playsim/p_mobj.cpp` **PrintMiscActorInfo** 8987–9061 (75 lines)

**Credited native LOC:** 229 (tally `nativeLoc` 229).

## Managed

`VmPMobjPrintMiscActorInfo.cs`, `VmPMobjPrintMiscActorInfoTests.cs`, `VmPMobjTagDamageRestore.cs`, `VmPMobjTagDamageRestoreTests.cs`
Managed LOC (tally): 224.
Conversion commit: `8371183403ac2f142a9b1e3261f2418718de3aeb`.

## Tests

- `VmPMobjTagDamageRestoreTests.Tag_StringTableWhenDollar`
- `VmPMobjTagDamageRestoreTests.ClearCounters_KillWhenAlive`
- `VmPMobjTagDamageRestoreTests.DamageFactor_ScalesPositive`
- `VmPMobjTagDamageRestoreTests.Translation_EmptyResets`
- `VmPMobjTagDamageRestoreTests.Restore_SpawnFloatNeedsSpace`
- `VmPMobjTagDamageRestoreTests.Restore_ClampFloor`
- `VmPMobjPrintMiscActorInfoTests.Print_CountsFlagBits`
- `VmPMobjPrintMiscActorInfoTests.Print_SkipsNullQuery`
- `VmPMobjPrintMiscActorInfoTests.Print_LegacyStyleMatch`
- `VmPMobjPrintMiscActorInfoTests.Print_CustomStyleLabel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 502 --to 502` (0 hard / 0 soft).

## Seams

`GStrings`, `mStringPropertyData`, inventory `VMCall`, `R_FindCustomTranslation`, and `Printf` are not hosted.

## Credit rationale

Credits tag/character name resolution, kill/item/secret counter clearing, inventory damage walk, damage factor scaling, translation lookup, item respawn Z policy, and misc actor debug summary from `p_mobj.cpp` (8811–9061).
