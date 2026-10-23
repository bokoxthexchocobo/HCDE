# Chunk 891 — `SkipZandronumCompatCommand`, `ParseCompatibility`, and `MapLoader::CheckCompatibility` (`compatibility.cpp` 204–391). GL node vertex/`DoLoadGLNodes` bands deferred (under 150 each; credited 241–643 between).

## Scope

`SkipZandronumCompatCommand`, `ParseCompatibility`, and `MapLoader::CheckCompatibility` (`compatibility.cpp` 204–391). GL node vertex/`DoLoadGLNodes` bands deferred (under 150 each; credited 241–643 between).

## Native ranges

- `src/maploader/compatibility.cpp` **SkipZandronumCompatCommand** 204–214 (11 lines)
- `src/maploader/compatibility.cpp` **ParseCompatibility** 224–315 (92 lines)
- `src/maploader/compatibility.cpp` **MapLoader::CheckCompatibility** 323–391 (69 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Root cause (`CheckCompatibility_AppliesDatabaseFlagsForKnownMap31Hash`)

Native `CheckCompatibility` loads `compatibility.txt` when `BCompatMap` is empty, then merges `flags->BugCompatFlags` into `Level->ib_compatflags` (not `ii_compatflags`) before `ApplyCompatibility`. The map31 assertion (`IbCompatFlags == setslopeoverflow|resetplayerspeed`) failed because:

1. **`ParseHex` rejected upper-case MD5 letters** — native lowercases with `| ('a' ^ 'A')` then tests `'a'–'f'`; the first port tested `'A'–'F'` after masking, so hashes in tests and in `wadsrc/static/compatibility.txt` never entered `BCompatMap` and lookup left `IbCompatFlags` at 0.
2. **Shipped `compatibility.txt` discovery** — with the embedded fallback removed, an empty database must resolve `wadsrc/static/compatibility.txt` from the repo root; a fixed `../../../../..` hop from `bin/Debug/net8.0` stops at `csharp/`. Discovery now walks parent directories from `AppContext.BaseDirectory` and `Directory.GetCurrentDirectory()`.

## Managed

`VmMapLoaderCompatibility.cs`
Managed LOC (tally): 500.
Conversion commit: `fc83d807c4f53189ade66dc7229cf68770e4a9e9`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `SkipZandronumCompatCommand` **204–214** | **yes** |
| `ParseCompatibility` **224–315** | **yes** |
| `MapLoader::CheckCompatibility` **323–391** | **yes** |

| Managed | Native (`compatibility.cpp`) |
|---------|------------------------------|
| `SkipZandronumCompatCommand` | **204–214** |
| `ParseCompatibility` | **224–315** |
| `CheckCompatibility` | **323–391** |

### Padding map (hull **204–391**)

| Native lines | Reason |
|--------------|--------|
| **215–223**, **316–322** | comment/separator gaps between functions (in hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **172** |
| Net behavior-ported | **172** |


**Post-review ratio:** 500/172 managed/native ≈ 291% (src file LOC at HEAD; see tally).

## Tests

- `VmMapLoaderCompatibilityTests.ParseCompatibility_SkipsZandronumCommandBlock`
- `VmMapLoaderCompatibilityTests.ParseCompatibility_AssignsFlagsToMd5Block`
- `VmMapLoaderCompatibilityTests.ParseCompatibility_SharesBlockAcrossMultipleMd5Lines`
- `VmMapLoaderCompatibilityTests.ParseCompatibility_RejectsInvalidMd5Length`
- `VmMapLoaderCompatibilityTests.CheckCompatibility_AppliesDatabaseFlagsForKnownMap31Hash`
- `VmMapLoaderCompatibilityTests.CheckCompatibility_IwadDoomAppliesShortTexAndLight`
- `VmMapLoaderCompatibilityTests.CheckCompatibility_HexenHackSetsFloatBob`
- `VmMapLoaderCompatibilityTests.ApplyCompatibility_ClearsSubsectorLinksWhenPolyobjFlagToggles`
- `VmMapLoaderCompatibilityTests.CheckCompatibility_EmitsNotifyLineWhenDeveloperEnabled`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 891 --to 891` (0 hard / 0 soft).

## Seams

**Representation notes:** `compat.txt` bytes come from `ReadDefaultCompatibilityTxt()` (`wadsrc/static/compatibility.txt`) when the database is empty, matching native `fileSystem.GetNumForFullName("compatibility.txt")`. `ClearAllSubsectorLinks` is modeled as `CompatibilityLevelState.SubsectorLinksCleared` when `COMPATF_POLYOBJ` changes in `ApplyCompatibility` (`g_level.cpp` 2593–2600). Developer `Printf` lines are collected in an optional `notifyLines` list when `developer >= DMSG_NOTIFY`.

## Credit rationale

Faithful MD5-keyed compatibility table parse (including Zandronum command tolerance) and per-map checksum merge with IWAD and Hexen-hack policy before applying merged compat flags.
