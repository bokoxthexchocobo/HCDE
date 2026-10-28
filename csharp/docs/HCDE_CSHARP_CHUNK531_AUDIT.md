# Chunk 531 — ACSStringPool add/lock/mark/purge

## Scope

ACSStringPool add/lock/mark/purge

## Native ranges

- `src/playsim/p_acs.cpp` **ACSStringPool::PoolEntry::Lock** 945–1274 (330 lines)

**Credited native LOC:** 330 (tally `nativeLoc` 330).

## Managed

`csharp/src/HCDE.Playsim/VmAcsStringPool.cs` (src-only credit).

Managed LOC (tally): 231.

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

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `AddString`, `FindString`, `InsertString`, `FindFirstFreeEntry` | 992–1274 |
| `GetString`, `LockString`, `MarkString`, `LockStringArray`, `MarkStringArray`, `UnlockAll`, `PurgeStrings`, `Clear` | 976–1208 |
| `SuperFastHash` | `SuperFastHash` (thirdparty) |
| `PoolEntry.Lock` | 945–960 |

**Removed:** no-op GC prelude in `InsertString` (native calls `P_CollectACSGlobalStrings`; seam).

**Ratio ~70%:** C# uses `List`/`foreach` vs. `TArray`/`memset`; `MarkStringMap`/serializer paths not ported (seams).
