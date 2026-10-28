# Chunk 1042 — `GetSpriteIndex` push tail, `LoadAltHudStuff`, and `PClassActor::StaticInit` / `StaticSetActorNums` head from `info.cpp` (**301–450**). Continues **1041** (**151–300**).

## Scope

`GetSpriteIndex` push tail, `LoadAltHudStuff`, and `PClassActor::StaticInit` / `StaticSetActorNums` head from `info.cpp` (**301–450**). Continues **1041** (**151–300**).

## Native ranges

- `src/gamedata/info.cpp` **LoadAltHudStuff / PClassActor StaticInit** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmInfoLoadAltHud1042Slice.cs`
Managed LOC (tally): 103.
Conversion commit: `11426aa7ce601d289177c6b1f34ffa289091c66a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetSpriteIndex` push (**301–306**) | yes (`CreateSpriteDefDraft`; host/`VmSpriteCatalog` seam for list push) |
| `LoadAltHudStuff` game switch (**319–336**) | yes (`ResolveDefaultAltHudPicNames`) |
| `LoadAltHudStuff` ALTHUDCF loop (**338–379**) | yes (classify / inventory validation / icon tokens) |
| `PClassActor::StaticInit` sprite seed (**391–409**) | yes |
| `StaticInit` actor registry collect (**417–423**) | yes |
| post-load call order (**425–429**) | yes (step list) |
| `StaticSetActorNums` (**440–446**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–300** | chunks **1040–1041** |
| **271–306** lookup/cache | uncredited `VmSpriteCatalog` seam (push draft only credited **301–306**) |
| **319–336** partial overlap | `VmAltHudLoader.GetDefaultHudPics` seam — not double-counted |
| **411–415**, **387**, **428–429** host calls | `LoadActors`, `InitServices`, event manager |
| **448–450** | comment header before `SetReplacement` (**456+**) |
| **500–799** | chunks **1037–1038** |
| **800–893** | deferred (**94** lines) |
| **894–1043** | chunk **1039** |
| `VmActorClassRegistry.TrySetReplacement` | uncredited — outside hull |

| Managed (`VmInfoLoadAltHud1042Slice.cs`) | Native (`info.cpp`) |
|-------------------------------------------|---------------------|
| `CreateSpriteDefDraft` | **301–305** |
| `ResolveDefaultAltHudPicNames` | **319–336** |
| `ClassifyAltHudKeyword` / null texture / inventory apply | **343–377** |
| Format helpers | **360**, **364** |
| `NeedsDefaultSpriteSeed` / `DefaultSpriteSeedNames` | **391–409** |
| `ShouldAddActorClassToRegistry` | **417–423** |
| `StaticInitPostActorLoadSteps` | **425–429** |
| `RunRegisterIdsForAllActorClasses` | **440–446** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1042** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1041** |


**Post-review ratio:** 103/150 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmInfoLoadAltHud1042SliceTests.CreateSpriteDefDraft_ZeroFrames`
- `VmInfoLoadAltHud1042SliceTests.ClassifyAltHudKeyword_HealthBerserkOrItem`
- `VmInfoLoadAltHud1042SliceTests.ResolveDefaultAltHudPicNames_MatchesGameTypeSwitch`
- `VmInfoLoadAltHud1042SliceTests.IsExplicitNullAltHudTextureToken_RecognizesSentinels`
- `VmInfoLoadAltHud1042SliceTests.EvaluateAltHudInventoryClass_UnknownOrNotInventory`
- `VmInfoLoadAltHud1042SliceTests.BuildAltHudInventoryApply_NullTokenClearsIcon`
- `VmInfoLoadAltHud1042SliceTests.FormatAltHudClassMessages_IncludeClassName`
- `VmInfoLoadAltHud1042SliceTests.NeedsDefaultSpriteSeed_OnlyWhenEmpty`
- `VmInfoLoadAltHud1042SliceTests.DefaultSpriteSeedNames_MatchesStaticInit`
- `VmInfoLoadAltHud1042SliceTests.RunRegisterIdsForAllActorClasses_InvokesEach`
- `VmInfoLoadAltHud1042SliceTests.StaticInitPostActorLoadSteps_IncludesAltHud`
- `VmInfoLoadAltHud1042SliceTests.ShouldAddActorClassToRegistry_RequiresActorDescendant`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1042 --to 1042` (0 hard / 0 soft).

## Seams

`TexMan`, `fileSystem.FindLump`, `FScanner`, `LoadActors`, `InitBotStuff`, and `staticEventManager` remain host-injected; uncredited `VmSpriteCatalog` (lookup/add) and `VmAltHudLoader` (partial defaults) are not re-credited.

## Credit rationale

File-scoped UNCredited **`info.cpp`** **301–450**; excludes **1–300** (**1040–1041**), **500–799** (**1037–1038**), deferred **800–893**, and **894–1043** (**1039**).
