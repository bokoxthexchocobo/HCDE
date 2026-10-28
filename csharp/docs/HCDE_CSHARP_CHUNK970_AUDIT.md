# Chunk 970 — Continued ACS P-code enum opcodes **221–368** from `p_acs.cpp` (**301–450**); follows chunk **969** (**≤300**).

## Scope

Continued ACS P-code enum opcodes **221–368** from `p_acs.cpp` (**301–450**); follows chunk **969** (**≤300**).

## Native ranges

- `src/playsim/p_acs.cpp` **PCD_COS** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsPcodeEnumContinued970Slice.cs`
Managed LOC (tally): 174.
Conversion commit: `4b96f0fa5635e1079ba123e0b0d6dfbcb27e296b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| P-code enum **221–368** (**301–450**) | yes (148 opcode constants + cluster classifiers) |
| Comment marker lines in band | layout only |

| Managed (`VmPAcsPcodeEnumContinued970Slice.cs`) | Native (`p_acs.cpp`) |
|--------------------------------------------------|---------------------|
| `Pcd*` constants | **301–450** |
| `IsContinuedOpcode` / trig / world-array / actor-inventory / callfunc helpers | enum clusters |
| Band hull (exclusive slice) | **301–450** |

### Chunk **970** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**969** |


**Post-review ratio:** 174/150 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsPcodeEnumContinued970SliceTests.ContinuedRange_MatchesNativeBand`
- `VmPAcsPcodeEnumContinued970SliceTests.IsTrigOpcode_CoversCosThroughVectorangle`
- `VmPAcsPcodeEnumContinued970SliceTests.IsWorldArrayOpcode_Span`
- `VmPAcsPcodeEnumContinued970SliceTests.IsCallfuncOpcode_IncludesPushfunction`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 970 --to 970` (0 hard / 0 soft).

## Seams

Interpreter bodies for these opcodes remain in later `p_acs.cpp` bands; row credits enum/constants only.

## Credit rationale

Primary **970** UNCredited continuation after **969**; zero conversion/recognition overlap and no existing `VmPAcsPcode*` coverage in band.
