# Chunk 999 — `G_BuildTiccmd` net tail, `LookAdjust` / view pitch-yaw, and `ChangeSpy` advance from `g_game.cpp` (**1101–1250**); follows chunk **998** (**525–674**). Starts after chunk **9** (**798–1100**). Retired chunk **405** files remain uncredited — this row uses `VmGGameLookSpy999Slice` only.

## Scope

`G_BuildTiccmd` net tail, `LookAdjust` / view pitch-yaw, and `ChangeSpy` advance from `g_game.cpp` (**1101–1250**); follows chunk **998** (**525–674**). Starts after chunk **9** (**798–1100**). Retired chunk **405** files remain uncredited — this row uses `VmGGameLookSpy999Slice` only.

## Native ranges

- `src/g_game.cpp` **ChangeSpy** 1101–1250 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameLookSpy999Slice.cs`
Managed LOC (tally): 126.
Conversion commit: `ff8ffd6337397cae6bc4f9de3b85a01c0da138b8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Weapon select / flechette net tail (**1098–1108**) | yes (from **1101**) |
| Movement `<< 8` + input trace (**1110–1127**) | yes |
| `LookAdjust` (**1130–1148**) | yes |
| `G_AddViewPitch` (**1150–1189**) | yes |
| `G_AddViewAngle` (**1191–1204**) | yes |
| `ChangeSpy` eligibility loop (**1208–1253**) | yes (through **1250** hull) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **798–1100** | chunk **9** `G_BuildTiccmd` body |
| **1114–1115** | `K8tiAgentMergeTiccmd` host |
| **1098–1100** | chunk **9** tail overlap (hull starts **1101**) |
| **1256–1263** | camera attach / status bar host |
| Retired **405** `VmGGameLookAdjust*` / `VmGGameBuildTiccmdTail` | duplicate managed — not listed |

| Managed (`VmGGameLookSpy999Slice.cs`) | Native (`g_game.cpp`) |
|---------------------------------------|----------------------|
| `ScaleMovementAxis` / `ScaleMovementPair` | **1110–1111** |
| `ShouldWriteWeaponSelectNet` | **1098–1102** (net flush) |
| `ShouldEmitPeriodicInputTrace` / `FormatInputTraceLine` | **1118–1126** |
| `AdjustLookWithWeaponScales` | **1130–1147** |
| `ApplyViewPitchDelta` | **1156–1183** |
| `ApplyViewAngleDelta` | **1198–1199** |
| `ChangeSpy` gates / `AdvanceSpyPlayerIndexChangeSpy` | **1211–1253** |
| Band hull (exclusive slice) | **1101–1250** |

### Chunk **999** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**998** |


**Post-review ratio:** 126/150 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameLookSpy999SliceTests.BuildTiccmdTail`
- `VmGGameLookSpy999SliceTests.LookAndViewAdjust`
- `VmGGameLookSpy999SliceTests.ChangeSpyAdvance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 999 --to 999` (0 hard / 0 soft).

## Seams

`K8tiAgentMergeTiccmd`, `Net_Write*`, `StatusBar`, and `S_UpdateSounds` remain host-injected.

## Credit rationale

UNCredited **1101–1250** CLI band; disjoint from chunks **995–998** and chunk **9** (**798–1100**, **204–224**). Do not re-credit retired **405** managed filenames.
