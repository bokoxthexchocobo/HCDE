# Chunk 1286 — Legacy uniform blanking, sampler binding removal, and the shader count from hw_shaderpatcher.cpp (1-316).

## Scope

Legacy uniform blanking, sampler binding removal, and the shader count from hw_shaderpatcher.cpp (1-316).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_shaderpatcher.cpp` **RemoveLegacyUserUniforms** 1–316 (316 lines)

**Credited native LOC:** 316 (tally `nativeLoc` 316).

## Managed

`VmShaderPatch1286Slice.cs`
Managed LOC (tally): 399.
Conversion commit: `b5d4ce5130d248422a32ccf15674a2a3e818c5b2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Tokens (**35–67**) | yes |
| Legacy uniforms (**70–156**) | yes |
| Sampler bindings (**158–224**) | yes |
| Layout locations (**226–276**) | yes |
| Tables and the count (**280–316**) | yes |
| Band hull (**1–316**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Deprecation print | warning flag |
| Name walk | stored, not used in the product |

The exact timer and sampler phrases are replaced before the keyword scan. The texture2d scan continues from that cursor, so an earlier texture2d stays and a later non token still raises the warning. An underscore does not protect texture2d. A vertical tab does not start a uniform keyword. A timer with no semicolon blanks through the last character and still warns. timerX is left alone. A sampler binding accepts a hexadecimal, octal, or negative value and records only a real sampler uniform. A float, a sampler array, a capital sampler, and a prefixed layout stay. A failed binding search resumes seven characters later. Layout blanking replaces every character, including a newline, and an unclosed layout stops the scan. The keyword in does not match inout. The shader count multiplies the pass count by the full default table, including its terminator, plus the user shaders, the effect count, and the no texture index.

| Managed (`VmShaderPatch1286Slice.cs`) | Native (`hw_shaderpatcher.cpp`) |
|---------------------------------------|----------------------------------|
| Blanking, bindings, and the count | **1–316** |
| Band hull (exclusive slice) | **1–316** |

### Chunk **1286** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **316** |
| **Net headline** | **+316** vs post-**1285** |


**Post-review ratio:** 399/316 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmShaderPatch1286SliceTests.Legacy_SkipsAnEarlyTexture2dAndBlanksTheTabTimer`
- `VmShaderPatch1286SliceTests.Bindings_ParseTheBaseAndStopAtAnOpenLayout`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1286 --to 1286` (0 hard / 0 soft).

## Seams

Deprecation prints are warning flags. A missing semicolon blanks through the last stored character.

## Credit rationale

UNCredited hw_shaderpatcher.cpp 1-316 (+316). A full tally line scan shows no overlap. The file ends at line 316.
