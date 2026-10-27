# Chunk 933 — `LS_GlassBreak` junk/quest tail through `LS_SetGlobalFogParameter` (`p_lnspec.cpp` **3305–3454**). Follows **932** (**≤3304**); glow specials at **3458+** deferred.

## Scope

`LS_GlassBreak` junk/quest tail through `LS_SetGlobalFogParameter` (`p_lnspec.cpp` **3305–3454**). Follows **932** (**≤3304**); glow specials at **3458+** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_GlassBreak** 3305–3454 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecGlassConversation933Slice.cs`
Managed LOC (tally): 170.
Conversion commit: `16a41edc7da1701f50d3069422344d9095264ce1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `BuildGlassShard` / yaw/vel helpers | `LS_GlassBreak` junk loop **3305–3335** |
| `GlassUsesCustomType` / `GlassSpawnTypeResolved` | typed spawn **3313–3319** |
| `GlassStateOffset` | default junk state **3323–3325** |
| `BuildQuestGrant` / `QuestGrantInventory` | quest rewards **3337–3356** |
| `ResolveQuestActivator` | player fallback **3339–3348** |
| `TryStartConversation` | `LS_StartConversation` **3362–3382** |
| `BuildConversationAssign` | `LS_Thing_SetConversation` **3384–3412** |
| `ConversationApplyToIterator` | TID loop **3396–3405** |
| portal seam | `LS_Line_SetPortalTarget` **3414–3418** |
| `BuildPlaneReflection` | `LS_Sector_SetPlaneReflection` **3420–3434** |
| `BuildGlobalFog` / `ParseGlobalFogKind` | `LS_SetGlobalFogParameter` **3436–3454** |
| `ApplyGlobalFogDensity` | cases **0–1** **3441–3447** |
| `ApplyGlobalFogSky` | case **2** **3449–3451** |
| `return false` glass tail | **3358–3360** |

### Chunk **933** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**932** |


**Post-review ratio:** 170/150 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecGlassConversation933SliceTests.BuildGlassShard_DefaultJunk_ComputesStateOffset`
- `VmPLnspecGlassConversation933SliceTests.BuildQuestGrant_SkipsWhenNotSwitched`
- `VmPLnspecGlassConversation933SliceTests.BuildQuestGrant_ReturnsInventoryList`
- `VmPLnspecGlassConversation933SliceTests.TryStartConversation_RejectsDeadPlayer`
- `VmPLnspecGlassConversation933SliceTests.BuildConversationAssign_RejectsMissingDialog`
- `VmPLnspecGlassConversation933SliceTests.BuildPlaneReflection_SkipsSlopedFloor`
- `VmPLnspecGlassConversation933SliceTests.BuildGlobalFog_DensityHalvesValue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 933 --to 933` (0 hard / 0 soft).

## Seams

`Spawn`, `pr_glass`, `NativeStartConversation`, `ChangePortal`, sector iterators, and `Level->fogdensity` mutation remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **3155–3982** corridor; exclusive `VmPLnspecGlassConversation933Slice.cs`.
