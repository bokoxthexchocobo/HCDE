# Chunk 891 — `SkipZandronumCompatCommand`, `ParseCompatibility`, and `MapLoader::CheckCompatibility` (`compatibility.cpp` 204–391). GL node vertex/`DoLoadGLNodes` bands deferred (under 150 each; credited 241–643 between).

## Scope

`SkipZandronumCompatCommand`, `ParseCompatibility`, and `MapLoader::CheckCompatibility` (`compatibility.cpp` 204–391). GL node vertex/`DoLoadGLNodes` bands deferred (under 150 each; credited 241–643 between).

## Native ranges

- `src/maploader/compatibility.cpp` **SkipZandronumCompatCommand** 204–213 (10 lines)
- `src/maploader/compatibility.cpp` **ParseCompatibility** 224–315 (92 lines)
- `src/maploader/compatibility.cpp` **MapLoader::CheckCompatibility** 323–391 (69 lines)

**Credited native LOC:** 171 (tally `nativeLoc` 171).

## Managed

`VmMapLoaderCompatibility.cs`
Managed LOC (tally): 497.
Conversion commit: `933d8d6c517f25d99d1fd3bfa052777c93294d27`.

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

### Removed tally-padding (2026-10-23)

| Removed managed (`CheckCompatibility`) | Native (`compatibility.cpp` **323–391**) | Reason |
|----------------------------------------|------------------------------------------|--------|
| `if (mapChecksum.Length != 16)` + `throw new ArgumentException(...)` (**3** LOC) | **none** — native **350** `map->GetChecksum(md5.Bytes)` into fixed `FMD5Holder` bytes; no checksum-length guard in band | Added in **`77650fae`** only to hold **`managedLoc` 500** after empty exception ctor removal; not faithful port — removed; tally refreshed via **`land_chunk_fast.sh --resume 891`** (scripted `fix_chunk_tally.py` / `sync_loc_tally.py`, no hand matrix/tally edits). |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **172** |
| Net behavior-ported | **172** |

### Out-of-body uncredit (891)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **172** |
| **Out-of-body removed** | **1** |
| **Honest** | **171** |


**Post-review ratio:** 497/171 managed/native ≈ 291% (src file LOC at HEAD; see tally).

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
