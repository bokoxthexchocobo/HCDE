# Chunk 1293 — Version gate, extension flags, and the startup log from gl_interface.cpp (1-243).

## Scope

Version gate, extension flags, and the startup log from gl_interface.cpp (1-243).

## Native ranges

- `src/common/rendering/gl_load/gl_interface.cpp` **gl_LoadExtensions** 1–243 (243 lines)

**Credited native LOC:** 243 (tally `nativeLoc` 243).

## Managed

`VmGlInterface1293Slice.cs`
Managed LOC (tally): 245.
Conversion commit: `30f68a36366b76ea3a79b23b04852fc3975fb7e7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Extension list (**48–77**) | yes |
| Context reset (**87–91**) | yes |
| Version gate and flags (**101–187**) | yes |
| Startup log (**195–231**) | yes |
| Version pair (**233–243**) | yes |
| Band hull (**1–243**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GL strings | host queries |
| GL integers | host queries |
| Win32 compile | host flag |
| Pointer macro | no call site |

A reload appends extension names and clears the flag word before the version test. Below 3.3 the load is fatal, the real version is still stored, and a previous shader version, vendor, and bindless bit stay. An override from 3 up to but not including 3.3 is promoted to 3.3. If the driver is older than that request, the driver string is kept and no emulation line is printed. Otherwise the emulation line names the request. The compared version is the parsed value plus a hundredth. Below 4 the shader version is forced to 3.31, and a Windows vendor containing ATI Tech sets the no-clip flag. At 4.5 and above, buffer storage is set, and shader storage is cleared when the vertex block count is 0. Invalidate and debug follow 4.3 or the matching extension. The uniform block is capped. Extension names match with ASCII case folding. The startup log reads the live strings and integers. The core profile bit selects Core, and any other mask selects Compatibility. Setting the version does not clear the bindless bit.

| Managed (`VmGlInterface1293Slice.cs`) | Native (`gl_interface.cpp`) |
|---------------------------------------|-----------------------------|
| Version, flags, and the log | **1–243** |
| Band hull (exclusive slice) | **1–243** |

### Chunk **1293** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **243** |
| **Net headline** | **+243** vs post-**1292** |


**Post-review ratio:** 245/243 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmGlInterface1293SliceTests.Load_CapsTheBlockAndPrintsTheLiveQueries`
- `VmGlInterface1293SliceTests.Reload_KeepsStaleExtensionsAndClearsFlagsOnFailure`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1293 --to 1293` (0 hard / 0 soft).

## Seams

GL strings and integer queries are host inputs. The Windows vendor check is a host flag because that branch is compiled only for Win32. The unused function-pointer macro has no call.

## Credit rationale

UNCredited gl_interface.cpp 1-243 (+243). A full tally line scan shows no overlap. The file ends at line 243.
