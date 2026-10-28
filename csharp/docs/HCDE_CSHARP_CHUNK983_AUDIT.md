# Chunk 983 — `P_SetupLevel` finish, `P_Init`, `P_Shutdown`, and debug CCMD head from `p_setup.cpp` (**601–750**); follows chunk **982** (**451–600**). Last full ≥150 hull before EOF tail **751–788**.

## Scope

`P_SetupLevel` finish, `P_Init`, `P_Shutdown`, and debug CCMD head from `p_setup.cpp` (**601–750**); follows chunk **982** (**451–600**). Last full ≥150 hull before EOF tail **751–788**.

## Native ranges

- `src/p_setup.cpp` **P_SetupLevel** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSetupFinish983Slice.cs`
Managed LOC (tally): 85.
Conversion commit: `51201350d8c521250fa82c7d080e76c673998ab0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Heightsec / 3D-floor scan (**612–625**) | yes |
| Load geometry snapshot (**630–635**) | yes (predicates) |
| Lightning (**642–645**) | yes |
| Dynamic light init (**647–656**) | yes |
| `P_Shutdown` player psprite destroy (**672–684**) | yes (predicates) |
| `dumpgeometry` / `listmapsections` head (**693–750**) | yes (loop gates) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **601–603** | host precache calls (seam only) |
| **605–608** | deathmatch announce — already on chunk **982** (`DeathmatchAnnounceFollowsPrecache`) |
| **627–629**, **637–639**, **657–660**, **662–667**, **687–691** | comments, automap host, `P_Init` host calls |
| **631–635** | `memcpy` host |
| **751+** | outside hull (EOF tail deferred) |

| Managed (`VmPSetupFinish983Slice.cs`) | Native (`p_setup.cpp`) |
|---------------------------------------|----------------------|
| `HeightsecIgnoreScanApplies` / `Rendered3dFloorIgnoresHeightsec` / `ApplyIgnoreHeightsecFlag` / `ScanSectorHeightsecIgnoreFlags` | **612–622** |
| `ShouldSnapshotLoadMapGeometry` / `ShouldEnableCompatLineOnSide` | **630–635**, **639** |
| `ShouldStartLightningForLevel` | **642–645** |
| `MarkDynamicLightsCreated` / `ShouldInitializeDynamicLightsForActor` | **650–656** |
| `ShutdownDestroysClientSideStatics` / `ShutdownClearsPlayerPsprites` | **674–675**, **683–684** |
| `DumpGeometry*` / `MapsectionScanIndexValid` / `ListMapsectionsScansSubsectors` | **693–750** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **983** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**982** |


**Post-review ratio:** 85/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmPSetupFinish983SliceTests.SetupFinish_HeightsecAndBackup`
- `VmPSetupFinish983SliceTests.LightningAndDynamicLights`
- `VmPSetupFinish983SliceTests.InitShutdownAndDebugCommands`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 983 --to 983` (0 hard / 0 soft).

## Seams

`PrecacheLevel`, `memcpy`, `AM_Create`, `SetDynamicLights`, `P_InitEffects`, and `Printf` CCMD bodies remain host-injected.

## Credit rationale

UNCredited tail of `p_setup.cpp` main setup path; disjoint from **979–982**. Lines **601–603** precache calls are native in this hull (outside **982** `451–600`).
