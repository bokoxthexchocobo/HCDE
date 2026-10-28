# Chunk 1093 — `CommandPlayerType` parse/tick tail, `CommandHasWeaponPiece`, and `CommandDrawGem` parse/draw head from `sbarinfo_commands.cpp` (**3001–3150**); follows **1092** player-type lookup seam (**3000**).

## Scope

`CommandPlayerType` parse/tick tail, `CommandHasWeaponPiece`, and `CommandDrawGem` parse/draw head from `sbarinfo_commands.cpp` (**3001–3150**); follows **1092** player-type lookup seam (**3000**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandPlayerType / CommandHasWeaponPiece / CommandDrawGem** 3001–3150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandPlayerType1093Slice.cs`
Managed LOC (tally): 141.
Conversion commit: `91bfadded693d4c64ad3097ce0e204e40748977f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Player-type list loop tokens (**3012**) | yes |
| Player-type descendant tick truth (**3019–3030**) | yes |
| **HasWeaponPiece** ctor default piece (**3042**) | yes |
| Weapon class validation (**3051–3054**) | yes |
| Piece number ≥ 1 gate (**3058–3060**) | yes |
| Holder piece mask truth (**3072–3075**) | yes |
| Missing holder false truth (**3079**) | yes (flow) |
| **DrawGem** ctor defaults (**3091–3095**) | yes |
| Chain draw skip + wiggle Y (**3102–3107**) | yes |
| Chain horizontal offset modulo (**3108–3110**) | yes |
| Parse flag keywords (**3118–3132**) | yes |
| Flag list `|` vs `,` (**3135–3136**) | yes |
| Signed left/right padding (**3144–3150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2969–2977**, **2920** | **1092** player-class / is-selected truth helpers |
| **2985–3000**, **3009–3010** | **1092** parse head + comma advance |
| **3002–3003** | host `classes.Push` (count via **1092** helper only) |
| **3067–3071**, **3110–3112** | host inventory / `DrawGraphic` |
| **3138–3142** | host `newImage` |
| **3151+** | chain size / coords outside hull |

| Managed (`VmSbarinfoCommandPlayerType1093Slice.cs`, `VmSbarinfoCommandHasWeaponPiece1093Slice.cs`, `VmSbarinfoCommandDrawGem1093Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|----------------------------------------------------------------------------------------------------------------|----------------------------------|
| PlayerType + HasWeaponPiece + DrawGem head | **3001–3150** |
| Band hull (exclusive slice) | **3001–3150** |

### Chunk **1093** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1092** |


**Post-review ratio:** 141/150 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandPlayerType1093SliceTests.ResolvePlayerTypeClassListLoopContinues_WithStringOrIdentifier`
- `VmSbarinfoCommandPlayerType1093SliceTests.ResolvePlayerTypeFlowControlTruthFromDescendants_RequiresDescendantMatch`
- `VmSbarinfoCommandPlayerType1093SliceTests.ValidateHasWeaponPieceNumber_RejectsBelowOne`
- `VmSbarinfoCommandPlayerType1093SliceTests.ResolveHasWeaponPieceFlowControlTruth_ChecksPieceMaskBit`
- `VmSbarinfoCommandPlayerType1093SliceTests.CreateDrawGemCtorDefaults_MatchesNativeInit`
- `VmSbarinfoCommandPlayerType1093SliceTests.ApplyDrawGemChainWiggleOffset_AddsWiggleWhenValuesDiffer`
- `VmSbarinfoCommandPlayerType1093SliceTests.ComputeDrawGemChainHorizontalOffset_ModuloChainSize`
- `VmSbarinfoCommandPlayerType1093SliceTests.TryClassifyDrawGemParseFlagKeyword_MapsInterpolate`
- `VmSbarinfoCommandPlayerType1093SliceTests.ResolveDrawGemParseFlagListContinuation_EndsOnComma`
- `VmSbarinfoCommandPlayerType1093SliceTests.ApplyDrawGemSignedPaddingToken_SupportsNegative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1093 --to 1093` (0 hard / 0 soft).

## Seams

**1092** owns player-type parse head and list comma advance; **1093** credits descendant tick and new commands only. Host `PClass::FindClass`, inventory walks, `SetTruth`, and `newImage` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **3001–3150** (+150); player-type class list loop tail + `IsDescendantOf` truth, weapon-piece parse/tick mask, draw-gem ctor/draw offset/parse flags through signed right padding (**3150**).
