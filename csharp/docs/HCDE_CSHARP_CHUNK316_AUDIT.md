# Chunk 316 — FinishDehPatch retarget and pickup replacement

## Scope

FinishDehPatch retarget and pickup replacement

## Native ranges

- `src/gamedata/d_dehacked.cpp` **FinishDehPatch retarget and pickups** 3799–3880 (82 lines)

**Credited native LOC:** 82 (tally `nativeLoc` 82).

## Managed

`VmDehackedFinishDehPatch.cs` / `VmDehackedFinishWeaponAmmo.cs`
Managed LOC (tally): 95.
Conversion commit: `da9d29ab0ed47f513d37e35a5c6faf1bb9663769`.

## Tests

- `VmDehackedFinishDehPatchTests.ApplyRetargetAfterSlamFlags_SetsMf8Bit`
- `VmDehackedFinishDehPatchTests.ShouldCreatePickupWrapper_RequiresSpecial`
- `VmDehackedFinishDehPatchTests.TryAllocatePickupClassName_SkipsExisting`
- `VmDehackedFinishDehPatchTests.LinkPickupReplacement_ChainsPriorReplacement`
- `VmDehackedFinishDehPatchTests.MarkStateUseWeapon_SetsFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 316 --to 316` (0 hard / 0 soft).

## Seams

Engine types (`PClass`, `FState`, `S_FindSound`, scanner lump I/O) are injected via callbacks or omitted where the managed layer only models parse/validation rules.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
