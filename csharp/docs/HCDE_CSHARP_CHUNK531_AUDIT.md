# Chunk 531 — ACSStringPool add/lock/mark/purge

## Scope

ACSStringPool add/lock/mark/purge

## Native ranges

- `src/playsim/p_acs.cpp` **ACSStringPool::PoolEntry::Lock** 945–1274 (330 lines)

**Credited native LOC:** 330 (tally `nativeLoc` 330).

## Managed

`VmAcsStringPool.cs`, `VmAcsStringPoolTests.cs`
Managed LOC (tally): 234.
Conversion commit: `0715c4b8e21d6266d38b2139e0d87a00e01b84e5`.

## Tests

- `VmAcsStringPoolTests.AddString_DeduplicatesIdentical`
- `VmAcsStringPoolTests.PurgeStrings_RemovesUnlockedUnmarked`
- `VmAcsStringPoolTests.LockString_PreventsPurge`
- `VmAcsStringPoolTests.MarkString_PreventsPurgeUntilCleared`
- `VmAcsStringPoolTests.LockStringArray_LocksPoolIdsOnly`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 531 --to 531` (0 hard / 0 soft).

## Seams

`P_CollectACSGlobalStrings`, serializer I/O, and `FWorldGlobalArray` iteration are not hosted.

## Credit rationale

Credits ACS global string pool hashing, deduplicated insert, per-level locks, mark/purge retention, and bucket rehash from `p_acs.cpp` (945–1274).
