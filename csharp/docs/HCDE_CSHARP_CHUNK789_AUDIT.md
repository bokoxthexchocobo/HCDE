# Chunk 789 — `FACSStack` ctor/dtor and `ACSStringPool` lock/add/mark through `MarkStringArray` (`p_acs.cpp` 882–901, 945–1119).

## Scope

`FACSStack` ctor/dtor and `ACSStringPool` lock/add/mark through `MarkStringArray` (`p_acs.cpp` 882–901, 945–1119).

## Native ranges

- `src/playsim/p_acs.cpp` **FACSStack::FACSStack** 882–888 (7 lines)
- `src/playsim/p_acs.cpp` **FACSStack::~FACSStack** 890–901 (12 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::PoolEntry::Lock** 945–960 (16 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::ACSStringPool** 962–966 (5 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::Clear** 976–981 (6 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::AddString** 992–1017 (26 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::GetString** 1025–1034 (10 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::LockString** 1044–1050 (7 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::MarkString** 1061–1067 (7 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::LockStringArray** 1081–1095 (15 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::MarkStringArray** 1105–1119 (15 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::MarkStringMap** 1129–1146 (18 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::UnlockAll** 1157–1164 (8 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPAcsStackStringPool.cs`
Managed LOC (tally): 195.
Conversion commit: `0eeabed679d7e9d5a19144b254c0f62d9a07c9b7`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `AcsStack` ctor | `FACSStack::FACSStack` **882–888** |
| `AcsStack.Unlink` | `FACSStack::~FACSStack` **890–901** |
| `PoolEntry.Lock` | `PoolEntry::Lock` **945–951** |
| `PoolEntry` unlock path | `PoolEntry::Unlock` **953–960** |
| `Clear` bucket init | `ACSStringPool::ACSStringPool` **962–966** |
| `Clear` | `ACSStringPool::Clear` **976–981** |
| `AddString` | `AddString` const char* **992–1005**; FString **1007–1017** |
| `GetString` | **1025–1034** |
| `LockString` | **1044–1050** |
| `MarkString` | **1061–1067** |
| `LockStringArray` | **1081–1095** |
| `MarkStringArray` | **1105–1119** |
| `MarkStringMap` | **1129–1146** |
| `UnlockAll` | **1157–1164** |


**Post-review ratio:** 195/152 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsStackStringPoolTests.AcsStack_PushesHeadLinkedList`
- `VmPAcsStackStringPoolTests.LockStringArray_LocksValidPoolIds`
- `VmPAcsStackStringPoolTests.AddString_Deduplicates`
- `VmPAcsStackStringPoolTests.MarkStringMap_MarksValues`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 789 --to 789` (0 hard / 0 soft).

## Seams

`SuperFastHash` body lives in `superfasthash.cpp`; `FWorldGlobalArray` iteration is dictionary-backed.

## Credit rationale

Fills uncredited ACS stack + string pool entry band before purge/insert chunk790.
