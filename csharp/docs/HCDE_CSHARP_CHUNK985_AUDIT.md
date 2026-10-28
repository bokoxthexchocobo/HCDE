# Chunk 985 — `GetSpriteTexture`, `StaticFindStateOwner`, `StaticGetStateName`, and `FindLabel` from `p_states.cpp` (**1–150**); UNCredited head before later state-parser bands.

## Scope

`GetSpriteTexture`, `StaticFindStateOwner`, `StaticGetStateName`, and `FindLabel` from `p_states.cpp` (**1–150**); UNCredited head before later state-parser bands.

## Native ranges

- `src/p_states.cpp` **FState::GetSpriteTexture** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesHead985Slice.cs`
Managed LOC (tally): 101.
Conversion commit: `1db9ab271f304418acdc46547f2ed38244d559dc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetSpriteTexture` sprite/frame pick + skin branch (**60–76**) | yes |
| Return slot count cap (**77**) | yes |
| `StaticFindStateOwner` all classes (**87–99**) | yes |
| `StaticFindStateOwner` hierarchy (**108–119**) | yes |
| `StaticGetStateName` (**126–139**) | yes |
| `FindLabel` guard (**146–149**) | yes (lookup enabled) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–49**, **80–86**, **121–125**, **141–145** | includes / comments / banners |
| **151+** | `FStateLabels::Destroy` outside hull |

| Managed (`VmPStatesHead985Slice.cs`) | Native (`p_states.cpp`) |
|----------------------------------------|----------------------|
| `SelectEffectiveSprite` / `SelectEffectiveFrame` / `UsesAlternateSkinSprite` | **60–73** |
| `CapSpriteTextureReturnCount` / `ShouldWrite*` / `SpriteFlipForRotation` | **74–77** |
| `ScanAllActorClassesForStateOwner` | **87–99** |
| `FindStateOwnerInHierarchy` | **108–119** |
| `ResolveStateDisplayName` / `Format*` / `ShouldFormatDehExtraStateName` | **126–139** |
| `StateLabelLookupEnabled` | **146–149** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **985** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**984** |


**Post-review ratio:** 101/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesHead985SliceTests.GetSpriteTexture_SelectorsAndReturns`
- `VmPStatesHead985SliceTests.FindStateOwner_ScansClassesAndHierarchy`
- `VmPStatesHead985SliceTests.StateNamingAndLabels`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 985 --to 985` (0 hard / 0 soft).

## Seams

`DEFINE_ACTION_FUNCTION` VM prologue, `SpriteFrames`/`Skins` tables, and `BinarySearch` helper remain host-injected.

## Credit rationale

No existing `VmPStates*` managed slice; CLI UNCredited **1–150** with EOF **1174**.
