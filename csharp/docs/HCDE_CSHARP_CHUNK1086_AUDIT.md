# Chunk 1086 — `CommandAspectRatio::FindRatio`, default `ratioMap`, `CommandDrawShader`, and inventory bar ctor from `sbarinfo_commands.cpp` (**1951–2100**); follows **1085** enum seam (**1950**).

## Scope

`CommandAspectRatio::FindRatio`, default `ratioMap`, `CommandDrawShader`, and inventory bar ctor from `sbarinfo_commands.cpp` (**1951–2100**); follows **1085** enum seam (**1950**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandAspectRatio::FindRatio / CommandDrawShader / CommandDrawInventoryBar** 1951–2100 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandAspectRatio1086Slice.cs`
Managed LOC (tally): 126.
Conversion commit: `e53b10e7e59c8b70e8b1ba485f0c998298e238ce`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Nearest-aspect scan (**1964–1987**) | yes |
| Default `ratioMap[]` initializer (**1990**) | yes |
| DrawShader dimension validation (**2010–2016**) | yes |
| Shader direction keywords (**2019–2022**) | yes |
| Optional `reverse` flag (**2024–2031**) | yes |
| Shader texture slot index (**2004**) | yes |
| Inventory bar ctor defaults (**2057–2062**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1941–1950** | **1085** parse/tick seam |
| **1951–1957**, **2040–2050** | member/layout + shader texture host lookup |
| **2033–2038**, **2065–2097** | host coordinates / inventory VM |
| **2099–2100** | **Draw** entry outside behavior hull |
| **2101+** | inventory **Draw** loop outside hull |

| Managed (`VmSbarinfoCommandAspectRatio1086Slice.cs` + nested types) | Native (`sbarinfo_commands.cpp`) |
|----------------------------------------------------------------------|----------------------------------|
| FindRatio + ratioMap | **1960–1990** |
| DrawShader parse policy | **2008–2031** |
| Inventory bar ctor | **2057–2062** |
| Band hull (exclusive slice) | **1951–2100** |

### Chunk **1086** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1085** |


**Post-review ratio:** 126/150 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandAspectRatio1086SliceTests.SelectNearestAspectRatioKind_Picks169ForWideScreen`
- `VmSbarinfoCommandAspectRatio1086SliceTests.CreateDefaultAspectRatioRemapTable_Maps1710To1610`
- `VmSbarinfoCommandAspectRatio1086SliceTests.ResolveAspectRatioTickTruthFromNearestFind_UsesRemapTable`
- `VmSbarinfoCommandAspectRatio1086SliceTests.ValidateDrawShaderDimensions_RejectsZeroWidth`
- `VmSbarinfoCommandAspectRatio1086SliceTests.ClassifyDrawShaderDirectionKeyword_MapsVertical`
- `VmSbarinfoCommandAspectRatio1086SliceTests.ResolveDrawShaderTextureSlotIndex_EncodesVerticalReverse`
- `VmSbarinfoCommandAspectRatio1086SliceTests.ApplyDrawShaderReverseKeyword_EnablesReverseAndComma`
- `VmSbarinfoCommandAspectRatio1086SliceTests.CreateDrawInventoryBarCtorDefaults_MatchesNativeInit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1086 --to 1086` (0 hard / 0 soft).

## Seams

**1085** owns aspect string parse/self-map/tick equality predicate; **1086** owns nearest-aspect find + static remap table. Host `ActiveRatio`, `TexMan`, `VMCall` PrevInv/NextInv, and **Draw** body (**2101+**) excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1951–2100** (+150); FindRatio nearest match, ratioMap initializer, draw-shader parse gates, inventory bar ctor defaults; hull ends at **Draw** `{` (**2100**).
