# Chunk 1254 — Post-process shader enable and uniform writes from hw_postprocessshader.cpp (1-165), through end of file.

## Scope

Post-process shader enable and uniform writes from hw_postprocessshader.cpp (1-165), through end of file.

## Native ranges

- `src/common/rendering/hwrenderer/postprocessing/hw_postprocessshader.cpp` **ShaderSetEnabled** 1–165 (165 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmPostShader1254Slice.cs`
Managed LOC (tally): 98.
Conversion commit: `7a3ca7bc84b11a64ccbc06a9dfc55fcdedf6ed50`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SetEnabled (**27–45**) | yes |
| SetUniform1f (**47–71**) | yes |
| SetUniform2f (**73–94**) | yes |
| SetUniform3f (**96–118**) | yes |
| SetUniform4f (**120–143**) | yes |
| SetUniform1i (**145–165**) | yes |
| Band hull (**1–165**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Shader storage | host list |
| VM parameter prologue | host call |

Every shader whose name matches is updated, and a different case is a different name. A missing name changes nothing. Enabling does not touch uniforms. A one-float write stores the value and then 0, 0, and 1. Two floats store x, y, 0, and 1. Three floats store x, y, z, and 1. Four floats store all four lanes, including a w that is not forced to 1. An integer uniform stores the int as a double and then 0, 0, and 1. A later shorter write replaces the lanes a longer write left behind. The uniform type is left as it was, including the undefined type of a uniform created by the write. Each action returns 0.

| Managed (`VmPostShader1254Slice.cs`) | Native (`hw_postprocessshader.cpp`) |
|--------------------------------------|--------------------------------------|
| Enable and the five uniform writes | **1–165** |
| Band hull (exclusive slice) | **1–165** |

### Chunk **1254** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **165** |
| **Net headline** | **+165** vs post-**1253** |


**Post-review ratio:** 98/165 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmPostShader1254SliceTests.Enabled_UpdatesEveryCaseSensitiveMatch`
- `VmPostShader1254SliceTests.Uniform_FillsUnusedLanesAndLeavesTheTypeAlone`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1254 --to 1254` (0 hard / 0 soft).

## Seams

The shader list is host storage. A missing uniform is inserted the way TMap operator[] inserts, with an undefined type and four zeros, and these writers do not change that type. Name compare is case-sensitive.

## Credit rationale

UNCredited hw_postprocessshader.cpp 1-165 (+165). A full tally line scan shows no overlap. The file ends at line 165.
