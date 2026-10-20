# Chunk 247 — Re-audit: default sprite seeding and SetReplacement guard only.

## Scope

Re-audit: default sprite seeding and SetReplacement guard only.

## Native ranges

- `src/gamedata/info.cpp` **PClassActor::StaticInit sprite seed** 392–408 (17 lines)
- `src/gamedata/info.cpp` **PClassActor::SetReplacement** 456–474 (19 lines)

**Credited native LOC:** 36 (tally `nativeLoc` 36).

## Managed

`VmActorClassRegistry.cs`
Managed LOC (tally): 30.
Conversion commit: `4c6ef298cf688f0b1404e158d3af31e6a98a95ab`.

## Tests

- `VmActorClassRegistryTests.EnsureDefaultSprites_SeedsPlaceholderSprites`
- `VmActorClassRegistryTests.TrySetReplacement_RequiresKnownActor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 247 --to 247` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
