# Chunk 1304 — Shader compile, the program binary, and the present and shadow binds from gl_shaderprogram.cpp (1-366).

## Scope

Shader compile, the program binary, and the present and shadow binds from gl_shaderprogram.cpp (1-366).

## Native ranges

- `src/common/rendering/gl/gl_shaderprogram.cpp` **FShaderProgram::Compile** 1–366 (366 lines)

**Credited native LOC:** 366 (tally `nativeLoc` 366).

## Managed

`VmGlShaderProgram1304Slice.cs`
Managed LOC (tally): 292.
Conversion commit: `ec1f298029341c4a9d80201d72744d824b9699be`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct and destroy (**41–63**) | yes |
| CreateShader and both Compile overloads (**71–101**) | yes |
| CompileShader and Link (**104–198**) | yes |
| Uniform block, bind, and info logs (**206–255**) | yes |
| PatchShader (**263–293**) | yes |
| Present and shadow Bind (**298–364**) | yes |
| Band hull (**1–366**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| RemoveLayoutLocationDecl and RemoveSamplerBindings | chunk 1286 |
| Program-binary checksum and file cache | callee in gl_shader.cpp; this band passes the patched sources |
| Uniform block declaration text | callee used by the present and shadow prolog |

The constructor leaves both shader ids at zero. Destroy deletes a program id and each live shader id once. CreateShader uses a vertex shader for every type except fragment. Compile from a lump throws when the lump is missing, before any patch. The other compile stores the name and the patched source and does not compile yet. The patch writes the version, then the defines when the pointer is non-null, then highp int, highp float, and line 1. A max below 420 is raised to 420 when the device version is at least 4.2, and that raised max is what the sampler strip tests. The version is the device version times ten, rounded, times ten, then clamped to the max. Vertex shaders strip layout locations on out. Fragment shaders strip them on in. Below 420 the sampler bindings are removed and remembered. CompileShader creates the shader, labels it when debug is on, and throws the info log without attaching when compile status is false. Success creates the program on the first attach. Link labels the current program id first, which is zero when nothing has been compiled. The cache is consulted only when the vendor string contains Intel, and that decision sticks after the first call. A non-empty binary is used only when the program-binary entry point exists. A successful binary link skips both compiles. A failed one falls through. The compile path links and, when the binary entry point and the cache are both available, stores the queried binary under the patched sources. Below GLSL 4.20 the program is used and each remembered sampler with a non-negative location is written, then the list is cleared. At 4.20 and above that loop is skipped and the list is still cleared. The uniform-block bind runs only below 4.20, and an invalid block index does not bind. Present binds compile the screen quad and the present fragment with the uniform prolog and max 330, link the present name, set the block at binding 2, and create the buffer once. Checker, column, and row use their own fragment and program names. Shadow compiles the screen quad with an empty define string and the shadow fragment with the shadow prolog, max 430, and links shaders/glsl/shadowmap. A second bind only uses the program.

| Managed (`VmGlShaderProgram1304Slice.cs`) | Native (`gl_shaderprogram.cpp`) |
|------------------------------------------|-------------------------------|
| Compile, binary, present, and shadow | **1–366** |
| Band hull (exclusive slice) | **1–366** |

### Chunk **1304** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **366** |
| **Net headline** | **+366** vs post-**1303** |


**Post-review ratio:** 292/366 managed/native ≈ 80% (src file LOC at HEAD; see tally).

## Tests

- `VmGlShaderProgram1304SliceTests.Patch_BumpsPast330AndAFailedCompileDoesNotAttach`
- `VmGlShaderProgram1304SliceTests.Bind_LoadsAnIntelBinaryAndInitsPresentOnce`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1304 --to 1304` (0 hard / 0 soft).

## Seams

Layout and sampler edits are the shader patcher already credited in chunk 1286. The binary cache checksum lives in gl_shader.cpp; this band passes the patched vertex and fragment sources. The vendor gate is that callee's first-call result.

## Credit rationale

UNCredited gl_shaderprogram.cpp 1-366 (+366). A full tally line scan shows no overlap. The file ends at line 366.
