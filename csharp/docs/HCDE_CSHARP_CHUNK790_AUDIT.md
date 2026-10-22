# Chunk 790 — `MarkStringMap`, `UnlockAll`, `PurgeStrings`, `FindString`, `InsertString`, and `FindFirstFreeEntry` (`p_acs.cpp` 1129–1291).

## Scope

`MarkStringMap`, `UnlockAll`, `PurgeStrings`, `FindString`, `InsertString`, and `FindFirstFreeEntry` (`p_acs.cpp` 1129–1291).

## Native ranges

- `src/playsim/p_acs.cpp` **ACSStringPool::MarkStringMap** 1129–1146 (18 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::UnlockAll** 1157–1164 (8 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::PurgeStrings** 1174–1209 (36 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::FindString** 1220–1235 (16 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::InsertString** 1245–1274 (30 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::FindFirstFreeEntry** 1284–1291 (8 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool GC seam comments** 1147–1156 (10 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool GC seam comments** 1165–1173 (9 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool GC seam comments** 1210–1219 (10 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool GC seam comments** 1275–1283 (9 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmPAcsStringPoolGc.cs`
Managed LOC (tally): 163.
Conversion commit: `bd0dc6076bdc29147fc2417d565fbc49c481fd08`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `MarkStringMap` iterator | **1129–1146** |
| `UnlockAll` mark/lock clear | **1157–1164** |
| `PurgeStrings` bucket reset | **1174–1178** |
| `PurgeStrings` free vs keep | **1180–1209** |
| `FindString` hash walk | **1220–1235** |
| `InsertString` GC hook | **1247–1252** |
| `InsertString` capacity/id guard | **1253–1256** |
| `InsertString` grow/scan free | **1257–1265** |
| `InsertString` bucket link | **1266–1274** |
| `FindFirstFreeEntry` | **1284–1291** |


**Post-review ratio:** 163/154 managed/native ≈ 106% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsStringPoolGcTests.PurgeStrings_RemovesUnmarked`
- `VmPAcsStringPoolGcTests.UnlockAll_ClearsLocks`
- `VmPAcsStringPoolGcTests.LockString_PreventsPurge`
- `VmPAcsStringPoolGcTests.InsertString_TriggersGcCallbackWhenFull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 790 --to 790` (0 hard / 0 soft).

## Seams

`P_CollectACSGlobalStrings` and `FSerializer` PNG I/O are callback seams.

## Credit rationale

Completes string pool GC/insert band after chunk789 add/lock/mark.
