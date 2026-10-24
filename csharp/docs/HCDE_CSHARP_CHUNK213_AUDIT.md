# Chunk 213 — InitActorNumsFromMapinfo resolve and P_GetSpawnableType.

## Scope

InitActorNumsFromMapinfo resolve and P_GetSpawnableType.

## Native ranges

- `src/gamedata/g_doomedmap.cpp` **InitActorNumsFromMapinfo** 251–284 (34 lines)
- `src/gamedata/g_doomedmap.cpp` **P_GetSpawnableType** 286–305 (20 lines)

**Credited native LOC:** 54 (tally `nativeLoc` 54).

## Managed

`VmDoomEdMapinfoResolver.cs`, `VmDoomEdMapinfoResolverTests.cs`
Managed LOC (tally): 48.
Conversion commit: `4d88af30fe50d6fece1418301b8eea64ab86b229`.

## Tests

- `VmDoomEdMapinfoResolverTests.TryGetSpawnableClassName_UsesNumberedTable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 213 --to 213` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
