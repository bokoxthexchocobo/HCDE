# Chunk 1414 — Title demo advance, the endgame command, and CVARINFO parsing in d_main.cpp (1950-2241).

## Scope

Title demo advance, the endgame command, and CVARINFO parsing in d_main.cpp (1950-2241).

## Native ranges

- `src/d_main.cpp` **D_DoAdvanceDemo** 1950–2241 (292 lines)

**Credited native LOC:** 292 (tally `nativeLoc` 292).

## Managed

`VmDMain1414Slice.cs`
Managed LOC (tally): 438.
Conversion commit: `abe44d2ad59d818c37694a4ebc6b378165dc89a4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Title demo advance (**1950–2046**) | yes |
| Start title (**2054–2060**) | yes |
| Endgame command (**2070–2078**) | yes |
| CVARINFO parse (**2086–2241**) | yes |
| Band hull (**1950–2241**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| D_DoStrifeAdvanceDemo | ends at line 1942 |
| ConsiderPatches | starts at line 2251 |
| Map check, new game, Strife advance, demo play, music, console hide, texture check, and cvar creation | callees |

Advance clears the advance flag first. A game action other than nothing returns immediately. Otherwise the console player becomes live, the user game flag clears, and the pause clears. A TITLEMAP starts that map and returns before the Strife check. Strife calls the Strife advance and returns. Sequence 3 with a non-zero advisory time shows ADVISOR, sets the sequence to 1, and uses the advisory time. Otherwise sequence 3 falls into the demo case. That case invalidates the advisory. Unless demo playback is disabled, the demo count increases and the name is DEMO plus at most three digits. A missing lump resets the count and falls through to the title. A present lump clears the single-demo flag, defers that demo, and sets the sequence to 2. The title sets the demo screen, the title page, the title time, plays the title music once, sets the sequence to 3, resets the page count, and hides the console. Sequence 2 uses the page time and cycles the credit pages when the list is not empty. A non-empty page name replaces the page. Start title clears the music flag, the game action, and the sequence, then sets the advance flag. Endgame does nothing in a net game. Otherwise it selects the fullscreen console, resets the sequence, and checks the demo. Each global CVARINFO lump is parsed in C mode. Attributes are server, user, noarchive, cheat, latch, nosave, and handlerClass. nosave clears server and user before the scope check, so server plus user plus nosave is legal and server plus user is not. The type is bool, int, float, color, or string. A duplicate name is an error. An optional default keeps the source text, including MAXINT. A bad number or float aborts with the scanner message. The mod config loads only after a cvar was added.

| Managed (`VmDMain1414Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Advance, endgame, and CVARINFO | **1950–2241** |
| Band hull (exclusive slice) | **1950–2241** |

### Chunk **1414** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **292** |
| **Net headline** | **+292** vs the prior chunk |


**Post-review ratio:** 438/292 managed/native ≈ 150% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1414SliceTests.AdvanceDemoTitleAndEndgame`
- `VmDMain1414SliceTests.ParseCVarInfoReadsEveryGlobalLump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1414 --to 1414` (0 hard / 0 soft).

## Seams

Map checks, music, demo playback, texture lookup, and cvar creation stay callees. ConsiderPatches starts at line 2251.

## Credit rationale

UNCredited d_main.cpp 1950-2241 (+292). D_DoAdvanceDemo, D_StartTitle, the endgame command, and ParseCVarInfo are whole functions. The comment banners between them sit inside the span. ConsiderPatches starts at line 2251. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
