# Chunk 520 — PatchThing setup and property/frame/sound loop (part 1)

## Scope

PatchThing setup and property/frame/sound loop (part 1)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing** 1400–1550 (151 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmDehackedPatchThingLoopPrimary.cs`
Managed LOC (tally): 193.
Conversion commit: `8beae14d2b20e57f6c5f60b50f341f3a105e9ee8`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|-------------------|
| `IsNumericInRange` / `IsHitPointsKey` / `IsReactionTimeKey` / `IsTranslucencyKey` / `IsHeightKey` / `IsMissileDamageKey` / `IsInfightingGroupKey` / `ClampInfightingGroup` / `NormalizeProjectileGroup` / `ClampSplashGroup` / `FastSpeedFromVal` / `MeleeRangeFromDeh` / `LineEndsWithFrame` / `LineEndsWithSound` / `MapFrameLabelPrefix` / `SkipPlayerMeleeMissile` / `MapSoundLabelPrefix` / `ShouldMakeStateDefines` / `LoopContinues` / `IsRenderStyleKey` / `IsObituaryKey` / `ClampScale` / `ShortPropertyKey` / `IsSpeedKey` / `IsWidthKey` / `IsAlphaKey` / `IsScaleKey` / `IsDecalKey` / `IsDroppedItemKey` / `IsFastSpeedKey` / `IsMeleeRangeKey` / `IsSplashGroupKey` / `IsProjectileGroupKey` / `IsNoIceDeathKey` / `IsPainChanceKey` / `PainChanceFromVal` / `SoundFromNumericIndex` / `TreatLine2AsSoundName` / `UnknownThingLine` / `FormatUnknownLine` / `ResolveThingIndex` / `LogThingOutOfRange` / `DehToDouble` / `ClearProjectilePassWhenHeightSet` / `KeyLengthMatches` / `StrIcmpMatch` / `IsCrispySoundKey` / `IsBloodColorKey` / `IsConversationKey` / `IsGravityKey` / `IsFrictionKey` / `IsDamageFactorKey` / `IsDamageMultiplierKey` / `IsMaxStepHeightKey` / `IsMaxDropoffHeightKey` / `IsBounceFactorKey` / `IsWallBounceFactorKey` / `IsProjectilePassHeightKey` / `IsPhysicalHeightKey` | `PatchThing` 1400–1550 (1401: `else if (linelen == 12 && stricmp(Line1, "splash group") == 0)`; 1403: `if (val < 0)`; 1410: `else if (linelen == 10 && stricmp(Line1, "fast speed") == 0)`) |

### Out-of-body uncredit (520)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **151** |
| **Out-of-body removed** | **0** |
| **Honest** | **151** |


**Post-review ratio:** 193/151 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchThingLoopPrimaryTests.MapFrameLabelPrefix_Spawn`
- `VmDehackedPatchThingLoopPrimaryTests.SkipPlayerMeleeMissile_BlocksPlayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 520 --to 520` (0 hard / 0 soft).

## Seams

`GetLine`, `FindState`, `statedef`, `DecalLibrary`, and actor mutation are not hosted.

## Credit rationale

Credits DeHackEd thing setup, numeric bounds, group clamps, obituaries, frame labels, and sound labels from `PatchThing` in `d_dehacked.cpp` (1234–1631).
