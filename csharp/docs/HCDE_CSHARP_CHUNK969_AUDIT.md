# Chunk 969 — Continued ACS P-code enum opcodes **71–220** from `p_acs.cpp` (**151–300**); follows chunk **968** (**≤150**).

## Scope

Continued ACS P-code enum opcodes **71–220** from `p_acs.cpp` (**151–300**); follows chunk **968** (**≤150**).

## Native ranges

- `src/playsim/p_acs.cpp` **PCD_ORLOGICAL** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsPcodeEnumContinued969Slice.cs`
Managed LOC (tally): 180.
Conversion commit: `2f477650d90c89ed098de6536bed438122c843ea`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| P-code enum **71–220** (**151–300**) | yes (constants + cluster classifiers) |
| Comment markers (`/* 70*/` … `/*220*/`) | layout only |

| Managed (`VmPAcsPcodeEnumContinued969Slice.cs`) | Native (`p_acs.cpp`) |
|--------------------------------------------------|---------------------|
| `Pcd*` constants | **151–300** |
| `IsContinuedOpcode` / logical / print / inventory / stack / call helpers | enum clusters |
| Band hull (exclusive slice) | **151–300** |

### Chunk **969** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**968** |


**Post-review ratio:** 180/150 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsPcodeEnumContinued969SliceTests.ContinuedRange_SpansOneFiftyOpcodes`
- `VmPAcsPcodeEnumContinued969SliceTests.IsPrintClusterOpcode_MatchesNativeBand`
- `VmPAcsPcodeEnumContinued969SliceTests.IsInventoryOpcode_CoversSpawnHead`
- `VmPAcsPcodeEnumContinued969SliceTests.StackAndCallHelpers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 969 --to 969` (0 hard / 0 soft).

## Seams

Interpreter bodies for these opcodes remain in later `p_acs.cpp` bands; this row credits enum/constants only.

## Credit rationale

Primary **969** target `p_trace.cpp` **945–1094** rejected (`VmPTraceThingPlaneEdit` **CheckPlane**/**EditTraceResult** duplicate). Alternate `p_destructible` **822–971** rejected (`VmPDestructibleSerializeScript` off-tally duplicate). UNCredited `p_acs` continuation with zero tally/Vm overlap.
