# Chunk 891 — `ParseCompatibility` / `CheckCompatibility` (`compatibility.cpp` 204–391)

## Scope

`SkipZandronumCompatCommand`, `ParseCompatibility`, and `MapLoader::CheckCompatibility` from `compatibility.cpp` (hull **204–391**). GL `glnodes.cpp` vertex (`101–233`) and `DoLoadGLNodes` (`651–803`) bands remain deferred (each under 150 native LOC; credited **241–643** between them).

## Native ranges

- `src/maploader/compatibility.cpp` **SkipZandronumCompatCommand** 204–214 (11 lines)
- `src/maploader/compatibility.cpp` **ParseCompatibility** 224–315 (92 lines)
- `src/maploader/compatibility.cpp` **MapLoader::CheckCompatibility** 323–391 (69 lines)

**Credited native LOC:** 172.

## Managed

`VmMapLoaderCompatibility.cs`

## Representation notes

- Default empty database loads `wadsrc/static/compatibility.txt` from disk (native `compatibility.txt` lump).
- `ApplyCompatibility` / `ApplyCompatibility2` from `g_level.cpp` run at end of `CheckCompatibility`; `ClearAllSubsectorLinks` is `SubsectorLinksCleared` when `COMPATF_POLYOBJ` toggles.
- Developer `Printf` output is optional `notifyLines` when `developer >= DMSG_NOTIFY`.

## Padding review

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `SkipZandronumCompatCommand` **204–214** | **yes** |
| `ParseCompatibility` **224–315** | **yes** |
| `MapLoader::CheckCompatibility` **323–391** | **yes** |

### Padding map (hull **204–391**)

| Native lines | Reason |
|--------------|--------|
| **215–223**, **316–322** | comment/separator gaps between functions |

### Gross vs net

| | Value |
|--|------:|
| Gross native | **172** |
| Net behavior-ported | **172** |

## Root cause (CheckCompatibility / map31 test)

Native `CheckCompatibility` loads `compatibility.txt` when `BCompatMap` is empty, then merges `flags->BugCompatFlags` into `Level->ib_compatflags` (not `ii_compatflags`) before `ApplyCompatibility`. The failing `CheckCompatibility_AppliesDatabaseFlagsForKnownMap31Hash` assertion (`IbCompatFlags == setslopeoverflow|resetplayerspeed`) had two port gaps:

1. **`ParseHex` upper-case MD5 letters** — native lowercases with `| ('a' ^ 'A')` then tests `'a'–'f'`; the first port tested `'A'–'F'` after masking, rejecting normal `compatibility.txt` / test hashes so entries never entered `BCompatMap` and lookup left `IbCompatFlags` at 0.
2. **Shipped `compatibility.txt` discovery** — with the embedded fallback removed, an empty database must resolve `wadsrc/static/compatibility.txt` from the repo root. A fixed `../../../../..` hop from `bin/Debug/net8.0` stops at `csharp/`, not the repo root; discovery now walks parent directories from `AppContext.BaseDirectory` and `Directory.GetCurrentDirectory()`.

## Verification

- `dotnet test` — `VmMapLoaderCompatibilityTests` (9 cases)
- `uncredited_range_check.py --chunk 891 --band` → exit 0
- `verify_full_solution_tests.sh` (14/14) before land
