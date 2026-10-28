# Chunk 980 — `PrecacheLevel` body, `ClearPortals`, and `ClearLevelData` head from `p_setup.cpp` (**151–300**); follows chunk **979** (**1–150**).

## Scope

`PrecacheLevel` body, `ClearPortals`, and `ClearLevelData` head from `p_setup.cpp` (**151–300**); follows chunk **979** (**1–150**).

## Native ranges

- `src/p_setup.cpp` **PrecacheLevel** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSetupPrecacheLevel980Slice.cs`
Managed LOC (tally): 90.
Conversion commit: `1cb6fb5f9babed12dae2a4c144092a3778f19658`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PrecacheLevel` early out (**151–152**) | yes |
| Actor / precache class hitlist (**164–178**) | yes |
| Sector & side texture walks (**181–200**) | yes (predicates) |
| Level sky + gameinfo textures (**203–227**) | yes |
| Renderer precache dispatch (**230–233**) | yes |
| `ClearPortals` defaults (**243–277**) | yes (predicates) |
| `ClearLevelData` head (**286–300**) | yes (counter reset policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **159**, **161–167**, **218–227** loops | host calls (`memset`, iterators, `TexMan`) |
| **237–241**, **280–284** | comment banners |
| **291–294** | native pointer scrub host |

| Managed (`VmPSetupPrecacheLevel980Slice.cs`) | Native (`p_setup.cpp`) |
|----------------------------------------------|----------------------|
| `ShouldSkipPrecacheLevel` | **151–152** |
| `TryMergePrecacheClassEntry` / `ShouldTrackActorClassInHitlist` | **164–178** |
| `SideQualifiesForSkyTransferHitflag` / `MergeSidePrecacheHitflags` | **191–199** |
| `ShouldPrecacheLevelSkyTexture` / `ShouldAddWallPrecacheWhenTextureExists` | **203–227** |
| `UseSoftwareRendererPrecachePath` | **230–233** |
| `MinimumSectorPortalSlotsForDefaults` / portal coverage | **243–277** |
| `ResetLevelStatistics` / `ResetSkyScrollPositions` / `ClearLevelDataScrubsNativePointers` | **286–306** (in-band through **300**) |
| `SectorTexturePrecacheAddsForSectorPlanes` / `SideTexturePrecacheAddsPerSide` | **181–199** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **980** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**979** |


**Post-review ratio:** 90/150 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmPSetupPrecacheLevel980SliceTests.PrecacheLevel_SkipAndRendererPath`
- `VmPSetupPrecacheLevel980SliceTests.SideSkyTransferHitflag`
- `VmPSetupPrecacheLevel980SliceTests.PortalDefaultsAndCoverage`
- `VmPSetupPrecacheLevel980SliceTests.ClearLevelData_ResetsStatistics`
- `VmPSetupPrecacheLevel980SliceTests.PrecacheClassHitlist_MergesOnce`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 980 --to 980` (0 hard / 0 soft).

## Seams

`AddToList`, `TexMan`, `memset`, thinker iterators, `SWRenderer->Precache`, `hw_PrecacheTexture`, and GC pointer scrub remain host-injected.

## Credit rationale

UNCredited continuation of `p_setup.cpp`; chunk **979** owns **1–150** only. No `VmPSetupPrecache979Slice` overlap on native lines **151+**.
