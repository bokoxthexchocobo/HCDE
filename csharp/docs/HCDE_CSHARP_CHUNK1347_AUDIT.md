# Chunk 1347 — GOG, Steam, and Bethesda registry install paths from i_steam.cpp (1-289), through end of file.

## Scope

GOG, Steam, and Bethesda registry install paths from i_steam.cpp (1-289), through end of file.

## Native ranges

- `src/win32/i_steam.cpp` **I_GetGogPaths** 1–289 (289 lines)

**Credited native LOC:** 289 (tally `nativeLoc` 289).

## Managed

`VmSteamPaths1347Slice.cs`
Managed LOC (tally): 153.
Conversion commit: `8dad45d4aa9ae1d33cf0c3e8ff5595b836b12f4b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Registry query (**72–93**) | yes |
| GOG paths (**106–202**) | yes |
| Steam path (**213–224**) | yes |
| Bethesda paths (**235–289**) | yes |
| Band hull (**1–289**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Commented Bethesda directories | compiled out of the directory table |
| Registry APIs | the open and query results are supplied by the test store |

A missing key does not close a handle. A non-string type and a zero byte count skip the second read. A failed size query and a failed second read leave the value empty and still close the key. An empty string is not a path. A present string is returned and the value name is Path, SteamPath, InstallPath, or installLocation. GOG Ultimate Doom, Doom plus Doom II, Strife, Heretic, Hexen, and Deathkings use the install folder. Doom I Enhanced, Doom II, Doom II Enhanced, and Doom 3 BFG append their subdirectories. Final Doom appends TNT and Plutonia and does not append master/wads. Missing games are omitted and the remaining paths keep their order. The 32-bit roots omit Wow6432Node. Steam returns the current-user path without opening the local-machine key. An empty current-user value falls through to InstallPath. Both misses return an empty string. Bethesda returns no directories when the install location is missing. A present location is prefixed with /games/ and the eight live directories, including the space in DOOM II_Data. The commented Ultimate Doom and Doom Eternal paths are absent.

| Managed (VmSteamPaths1347Slice.cs) | Native (i_steam.cpp) |
|------------------------------------|----------------------|
| Registry query and the three install lists | **1–289** |
| Band hull (exclusive slice) | **1–289** |

### Chunk **1347** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **289** |
| **Net headline** | **+289** vs the prior chunk |


**Post-review ratio:** 153/289 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmSteamPaths1347SliceTests.QueryAndGog`
- `VmSteamPaths1347SliceTests.SteamAndBethesda`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1347 --to 1347` (0 hard / 0 soft).

## Seams

QueryPathKey clears the output, requires REG_SZ with a non-zero byte count, and closes the key when the open succeeds. An empty string is a failed query. GOG uses one key per game id and appends a fixed suffix. The 64-bit build inserts Wow6432Node. Steam tries the current-user SteamPath before the local-machine InstallPath. Bethesda appends /games/ and the eight uncommented directories.

## Credit rationale

UNCredited i_steam.cpp 1-289 (+289). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of d_net_aidirector.cpp 1-284.
