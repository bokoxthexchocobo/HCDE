# Chunk 521 — PatchThing Bits/Mass/ID and finalize (part 2)

## Scope

PatchThing Bits/Mass/ID and finalize (part 2)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing (secondary loop)** 1632–1906 (275 lines)

**Credited native LOC:** 275 (tally `nativeLoc` 275).

## Managed

`VmDehackedPatchThingLoopSecondary.cs`
Managed LOC (tally): 185.
Conversion commit: `ea602e4dd85355e04bb56293cd85a558d3b10972`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `PatchThing` Bits/Mass/`ID #` loop and post-loop finalize in `d_dehacked.cpp` **1632–1906**. Excluded `GetLine`/`FindState`/`statedef.InstallStates`/`PushTouchedActor` call (**1901**), `I_Error` host (**1835–1839**), and `OrgHeights` table read (**1850**).

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `IsMassKey` / `MassFromVal` | `Mass` branch **1634–1636** |
| `IsBitsKey` / `ParseBitsToken` / `StealthKeywordSetsZdoom` | `Bits` token loop **1638–1676** |
| `ApplyTranslationBits` | **1680–1684** |
| `HasTouchy` / `ClearTouchy` | **1686–1690** |
| `HasBounces` / `ClearBounces` / `ApplyBouncesRemap` / `SelectMissileBounceFlags` / `GrenadeTrailSwap` / `DefaultBounceFactor` / `DefaultWallBounceFactor` / `ApplyNobloodForNonSentient` | **1692–1736** |
| `StealthFriendHack` / `RemapFriend` / `ApplyFriendRemap` | **1738–1748** |
| `TranslucentSetsVchanged2` / `ClearTranslucentBit` / `MarkTranslucentVchanged2` / `ResolveTranslucentAlpha` | **1750–1754**, **1806–1813** |
| `MissileSetsNoteleport` / `ApplyMissileNoteleportFlags` / `ClearNoteleportWhenUnmissiled` / `ShouldClearNoteleport` / `ClearNoteleportFlag` | **1756–1768** |
| `PlayerAlwaysFriendly` / `MergePlayerFriendlyFlags` | **1770–1772** |
| `DoomCompatBounce` | **1778–1781** |
| `IceDamageType` / `FireDamageType` / `GravityQuarter` | **1784–1797** |
| `SetInvisibleRender` / `ClearInvisibleRender` | **1815–1818** |
| `IsIdKey` / `IdKeySetsEdnum` | **1823–1825** |
| `HeightPairInvalid` | **1833–1839** |
| `ShouldRestoreCeilingHeight` | **1846–1851** |
| `ShadowBecameSet` / `ShadowCleared` / `FinalizeShadowStyle` / `DefaultShadowAlpha` / `UseNormalStyleWhenUnshadow` | **1854–1867** |
| `NormalizeSpeed` | **1871–1873** |
| `ShouldPushTouchedActor` | **1876–1878** |
| `ApplyCountKillFlags` / `ClearCountKillFlags` | **1883–1894** |

### Out-of-body uncredit (521)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **275** |
| **Out-of-body removed** | **0** |
| **Honest** | **275** |


**Post-review ratio:** 185/275 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchThingLoopSecondaryTests.ResolveTranslucentAlpha_Half`
- `VmDehackedPatchThingLoopSecondaryTests.HeightPairInvalid_WhenMismatched`
- `VmDehackedPatchThingLoopSecondaryTests.NormalizeSpeed_ScalesLarge`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 521 --to 521` (0 hard / 0 soft).

## Seams

`statedef.InstallStates`, `PushTouchedActor`, `I_Error`, and `OrgHeights` are not hosted.

## Credit rationale

Credits DeHackEd thing Bits remaps, Mass/ID#, height-pair validation, shadow/speed finalize, and COUNTKILL flags from `PatchThing` in `d_dehacked.cpp` (1632–1906).
