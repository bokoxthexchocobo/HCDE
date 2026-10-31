# Chunk 1356 — Engine interface and startup selection from i_interface.cpp (1-257), through end of file.

## Scope

Engine interface and startup selection from i_interface.cpp (1-257), through end of file.

## Native ranges

- `src/common/engine/i_interface.cpp` **EngineInterface** 1–257 (257 lines)

**Credited native LOC:** 257 (tally `nativeLoc` 257).

## Managed

`VmEngineInterface1356Slice.cs`
Managed LOC (tally): 512.
Conversion commit: `8721f80e1d06d000109407e167d4d8a819c9bdb3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Pointer width, globals, and cvar rows (**29–87**) | yes |
| Pride switch, quoting, and startup file arguments (**89–115**) | yes |
| Selection constructor and SaveInfo (**118–246**) | yes |
| UUID format (**249–257**) | yes |
| Band hull (**1–257**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| UpdateLanguage | the string table lives outside this file |
| UpdateGenericUI | the UI refresh lives outside this file |
| HCDE_MigrateRendererCvars | the renderer migration lives outside this file |
| GenRand32 | the PCG lives outside this file |

The pointer-width check accepts 8 and rejects every other size. Twenty-five cvars are registered here. vid_fps has flags 0. language uses archive, no-init-call, and global-config, and its callback runs UpdateLanguage, then UpdateGenericUI on ui_generic, then LanguageChanged when that hook is set. The previous value is unused. A missing injected hook throws InvalidOperationException. Float 0.35 is stored as float of the double. Extern cvars are read and not registered. Pride is the switch -pride. Quote replaces every quote, then wraps the result. BuildStartupFileArgs splits on |, drops empty tokens, strips C whitespace while keeping bytes at or above 128, and emits -file plus the quoted path. A cvar string is open when its first byte is not NUL. Wad compare is stricmp and stops at the first NUL. The first match wins. The constructor calls the renderer migration before it copies the backend and fullscreen. A null wad list throws when a scan runs. A null args pointer throws after the copies. CheckParm of any non-zero index selects list; zero keeps ui_colors. SaveInfo strips the text fields, writes query, language, and fullscreen, and writes the backend only when it differs. The net path returns the net wad index and still appends the stripped net arguments when the save flag is clear. The local path clears the addon cvars when saveargs is clear and still appends -file arguments built from the member. String cvars assigned from GetChars stop at the first NUL. AppendRawArgsString receives the full string. GenerateUUID does not store into GameUUID. The six draws are the last short bound, the second word, the variant bound, the version bound, the first short bound, then the first word. The format is 8 hex, 4 hex, a literal 4, 3 hex, a literal 9, 3 hex, 8 hex, and 4 hex, with minimum widths.

| Managed (VmEngineInterface1356Slice.cs) | Native (i_interface.cpp) |
|----------------------------------------|--------------------------------|
| Engine interface, cvars, and startup selection | **1–257** |
| Band hull (exclusive slice) | **1–257** |

### Chunk **1356** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **257** |
| **Net headline** | **+257** vs the prior chunk |


**Post-review ratio:** 512/257 managed/native ≈ 199% (src file LOC at HEAD; see tally).

## Tests

- `VmEngineInterface1356SliceTests.DefaultsFlagsAndLanguage`
- `VmEngineInterface1356SliceTests.SelectionSaveAndUuid`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1356 --to 1356` (0 hard / 0 soft).

## Seams

UpdateLanguage, UpdateGenericUI, and HCDE_MigrateRendererCvars stay injected. GenRand32 and GenRand32BoundExclusive stay injected and are drawn in g++ right-to-left order. CheckParm and AppendRawArgsString stay injected.

## Credit rationale

UNCredited i_interface.cpp 1-257 (+257). A full tally line scan shows no overlap. This is tied for the largest remaining hole of at most 400 lines with p_saveg.cpp 336-592. Path order takes the engine interface.
