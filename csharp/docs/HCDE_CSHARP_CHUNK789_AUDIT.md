# Chunk 789 — `uallong`, `FACSStack` ctor/dtor, and `ACSStringPool` through `FindFirstFreeEntry` (`p_acs.cpp` 828–839, 882–901, 945–1291).

## Scope

`uallong`, `FACSStack` ctor/dtor, and `ACSStringPool` through `FindFirstFreeEntry` (`p_acs.cpp` 828–839, 882–901, 945–1291).

## Native ranges

- `src/playsim/p_acs.cpp` **uallong** 829–832 (4 lines)
- `src/playsim/p_acs.cpp` **uallong** 834–838 (5 lines)
- `src/playsim/p_acs.cpp` **FACSStack::FACSStack** 882–888 (7 lines)
- `src/playsim/p_acs.cpp` **FACSStack::~FACSStack** 890–901 (12 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::PoolEntry::Lock** 945–951 (7 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::PoolEntry::Lock** 953–960 (8 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::ACSStringPool** 962–966 (5 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::Clear** 976–981 (6 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::AddString** 992–1005 (14 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::AddString** 1007–1017 (11 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::GetString** 1025–1034 (10 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::LockString** 1044–1050 (7 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::MarkString** 1061–1067 (7 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::LockStringArray** 1081–1095 (15 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::MarkStringArray** 1105–1119 (15 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::MarkStringMap** 1129–1146 (18 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::UnlockAll** 1157–1164 (8 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::PurgeStrings** 1174–1209 (36 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::FindString** 1220–1235 (16 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::InsertString** 1245–1274 (30 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::FindFirstFreeEntry** 1284–1291 (8 lines)

**Credited native LOC:** 249 (tally `nativeLoc` 249).

## Managed

`VmPAcsStackStringPool.cs`, `VmPAcsStringPoolGc.cs`
Managed LOC (tally): 355.
Conversion commit: `0838e032bedbe9037fbbffd46443c48d98d57fef`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `UaLong` | `uallong` **828–839** |
| `AcsStack` ctor / unlink | **882–888**, **890–901** |
| `PoolEntry.Lock` / `Unlock` | **945–960** |
| `ACSStringPool` ctor / `Clear` | **962–966**, **976–981** |
| `AddString` / `GetString` | **992–1017**, **1025–1034** |
| `LockString` / `MarkString` | **1044–1050**, **1061–1067** |
| `LockStringArray` / `MarkStringArray` | **1081–1095**, **1105–1119** |
| `MarkStringMap` | **1129–1146** |
| `UnlockAll` / `PurgeStrings` | **1157–1164**, **1174–1209** |
| `FindString` / `InsertString` / `FindFirstFreeEntry` | **1220–1235**, **1245–1274**, **1284–1291** |

### Out-of-body uncredit (789)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **254** |
| **Out-of-body removed** | **5** |
| **Honest** | **249** |


**Post-review ratio:** 355/249 managed/native ≈ 143% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsStackStringPoolTests.UaLong_ReadsLittleEndian`
- `VmPAcsStackStringPoolTests.AcsStack_PushesHeadLinkedList`
- `VmPAcsStackStringPoolTests.LockStringArray_LocksValidPoolIds`
- `VmPAcsStackStringPoolTests.AddString_Deduplicates`
- `VmPAcsStackStringPoolTests.MarkStringMap_MarksValues`
- `VmPAcsStringPoolGcTests.PurgeStrings_RemovesUnmarked`
- `VmPAcsStringPoolGcTests.UnlockAll_ClearsLocks`
- `VmPAcsStringPoolGcTests.InsertString_TriggersGcCallbackWhenFull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 789 --to 789` (0 hard / 0 soft).

## Seams

`SuperFastHash`, `FWorldGlobalArray`, and `P_CollectACSGlobalStrings` are host seams.

## Credit rationale

Whole-function ACS stack + global string pool band (merged retired chunk790).
