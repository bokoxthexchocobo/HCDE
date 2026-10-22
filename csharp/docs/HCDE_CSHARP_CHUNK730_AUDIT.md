# Chunk 730 — ACS clientside script flag, stack head, fixed conversions, and iterator policy (`p_acs.cpp` 669–944).

## Scope

ACS clientside script flag, stack head, fixed conversions, and iterator policy (`p_acs.cpp` 669–944).

## Native ranges

- `src/playsim/p_acs.cpp` **IsClientSideScript** 669–944 (276 lines)

**Credited native LOC:** 276 (tally `nativeLoc` 276).

## Managed

`VmPAcsIsClientSideScript.cs`
Managed LOC (tally): 160.
Conversion commit: `14c0c931b67991c0add872f4a0008d0495fe80e5`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `VmPAcsIsClientSideScript` (aggregate band) | 669–944 |
| `IsClientSideScript` | 669–672 |
| `ScriptPtr` / `ScriptState` enum | `DLevelScript` state enum 681–693 |
| `LevelScriptMetadata` | `DLevelScript` fields 717–738, 729–733 |
| `CallReturn` | `CallReturn` struct 626–645 |
| `LevelScript.PickClientSideIterator` | `GetActorIterator` 757–760 |
| `AcsStack` / `StackPush` / `StackPop` | `FACSStack` 868–901 |
| `UaLong` | `uallong` 828–839 |
| `AcsToFloat` / `DoubleToAcs` / `AcsToAngle` / `PitchToAcs` | fixed helpers 601–624 (adjacent seam) |
| `ClientSideRandom` | `DLevelScript::Random` 3671 (policy) |
| `NormalizeWorldVarIndex` | world/global vars 848–855 |
| `BehaviorArrayValid` | `FBehavior::ArrayInfo` 814–818 |
| `AcsStringPool` | `GlobalACSStrings` 943 |
| `StringBuilderStart` / `StringBuilderFinish` | STRINGBUILDER_* 740–755 |
| `ScriptControllerIsClientSide` | clientside controller selection (seam) |
| `FixedMul` | ACS fixed math (seam) |
| `AdvanceScriptState` | `SetState` transitions (seam) |


**Post-review ratio:** 160/276 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsIsClientSideScriptTests.IsClientSideScript_FlagSet`
- `VmPAcsIsClientSideScriptTests.UaLong_ReadsLittleEndian`
- `VmPAcsIsClientSideScriptTests.LevelScript_PicksClientSideIterator`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 730 --to 730` (0 hard / 0 soft).

## Seams

`DLevelScript` method bodies beyond `GetActorIterator`, `ACSStringPool` GC, and `VMCall` are not hosted.

## Credit rationale

Single-function oversize exception (~276 native LOC): clientside script detection plus adjacent ACS infrastructure declarations in the credited band.
