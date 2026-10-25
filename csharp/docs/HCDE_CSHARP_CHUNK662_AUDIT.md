# Chunk 662 — Whole `FLevelLocals::CreateLinkedPortals` (`portal.cpp` 851–1039).

## Scope

Whole `FLevelLocals::CreateLinkedPortals` (`portal.cpp` 851–1039).

## Native ranges

- `src/playsim/portal.cpp` **FLevelLocals::CreateLinkedPortals** 851–1039 (189 lines)

**Credited native LOC:** 189 (tally `nativeLoc` 189).

## Managed

`VmPortalCreateLinkedPortals.cs`
Managed LOC (tally): 115.
Conversion commit: `3bb2704eba57cd767aec19235c119422ee4b5fd1`.

## Padding review (2026-10-21)

| Managed | Native (`portal.cpp`) |
|---------|---------------------|
| `CreateLinkedPortals` / `CreateLinkedPortalsContext` | `CreateLinkedPortals` 851–1039 |

### Out-of-body uncredit (662)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **189** |
| **Out-of-body removed** | **0** |
| **Honest** | **189** |


**Post-review ratio:** 115/189 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmPortalCreateLinkedPortalsTests.CreateLinkedPortals_CollectsWithLinkedIds`
- `VmPortalCreateLinkedPortalsTests.CreateLinkedPortals_FlagsDisplacementMismatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 662 --to 662` (0 hard / 0 soft).

## Seams

`CollectSectors`, `AddDisplacementForPortal`, and map mutation stay native.

## Credit rationale

Fresh portal TU; linked portal grouping hosted.
