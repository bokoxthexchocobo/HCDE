# Chunk 1310 — Save-list scan, GZDoom 4.14 import, and the save-name comment from loadsavemenu.cpp (1-285).

## Scope

Save-list scan, GZDoom 4.14 import, and the save-name comment from loadsavemenu.cpp (1-285).

## Native ranges

- `src/menu/loadsavemenu.cpp` **FSavegameManager::ReadSaveStrings** 1–285 (285 lines)

**Credited native LOC:** 285 (tally `nativeLoc` 285).

## Managed

`VmLoadSaveMenu1310Slice.cs`
Managed LOC (tally): 267.
Conversion commit: `6d1e7f80c06c1bf0ab433d2c3486f417ea3d1d8c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| GZDoom 4.14 save check (**52–125**) | yes |
| Save string scan (**127–225**) | yes |
| Load and save dispatch (**233–247**) | yes |
| Comment and save name (**255–271**) | yes |
| Manager singleton (**279–284**) | yes |
| Band hull (**1–285**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Directory scan and resource read | host returns the entries and the info.json bytes |
| Wad check and game load | callees; this band decides whether to call them |

The compatibility check requires the software string to start with GZDoom g, a 4.x version, and a minor of 14 or below. Minor 14 allows patch 0, 1, and 2, and rejects a two-digit patch. Any other prefix returns false. The engine argument is not read. ReadSaveStrings clears the list only when the sort cvar changed, and it scans only when the list is empty. The scan looks for *.zds. A missing resource, a missing info.json, or a reader that does not open is skipped. Engine GZDOOM is imported only when the 4.14 check passes, and that path does not also apply the gamesig or newer-than-4560 rejection. Other engines are kept when the name matches HCDE, ignoring case, or the allow list contains HCDE, and the save version is at most 4560. A version below 4556 is listed as old. Otherwise a matching IWAD asks the wad check, and a different IWAD is skipped. Load and save forward the file and the flag or title. The comment is the creation time, a newline when that time is not empty, then the comment. The save name formats the prefix and a two-digit slot, then asks the game for the path. GetManager returns the process manager.

| Managed (`VmLoadSaveMenu1310Slice.cs`) | Native (`loadsavemenu.cpp`) |
|----------------------------------------|----------------------------|
| Scan, import, comment, and save name | **1–285** |
| Band hull (exclusive slice) | **1–285** |

### Chunk **1310** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **285** |
| **Net headline** | **+285** vs post-**1309** |


**Post-review ratio:** 267/285 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmLoadSaveMenu1310SliceTests.Compat_AllowsGzdoom4142AndRejectsTheNeighbors`
- `VmLoadSaveMenu1310SliceTests.Read_SkipsForeignSavesAndKeepsTheSortCache`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1310 --to 1310` (0 hard / 0 soft).

## Seams

ScanDirectory, resource open, and the serializer are host calls. G_CheckSaveGameWads, G_LoadGame, G_SaveGame, and G_BuildSaveName are callees. InsertSaveNode is the list append in scan order. The sort-order memory is process lifetime. LOAD_GZDOOM_4142_SAVES is on, so a GZDOOM engine uses the 4.14 check instead of the gamesig test.

## Credit rationale

UNCredited loadsavemenu.cpp 1-285 (+285). A full tally line scan shows no overlap. The file ends at line 285.
