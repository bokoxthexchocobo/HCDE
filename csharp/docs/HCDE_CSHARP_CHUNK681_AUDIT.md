# Chunk 681 — Whole `AActor::UnlinkFromWorld`, `FixMapthingPos`, `LinkToWorld`, and `SetOrigin` (`p_maputl.cpp` 256–319, 328–402, 412–534, 536–544).

## Scope

Whole `AActor::UnlinkFromWorld`, `FixMapthingPos`, `LinkToWorld`, and `SetOrigin` (`p_maputl.cpp` 256–319, 328–402, 412–534, 536–544).

## Native ranges

- `src/playsim/p_maputl.cpp` **AActor::UnlinkFromWorld** 256–319 (64 lines)
- `src/playsim/p_maputl.cpp` **AActor::FixMapthingPos** 328–402 (75 lines)
- `src/playsim/p_maputl.cpp` **AActor::LinkToWorld** 412–534 (123 lines)
- `src/playsim/p_maputl.cpp` **AActor::SetOrigin** 536–544 (9 lines)

**Credited native LOC:** 271 (tally `nativeLoc` 271).

## Managed

`VmPMapUtlActorWorldLink.cs`
Managed LOC (tally): 256.
Conversion commit: `2b05aefd0489c3c9c6203e40da2a09c345d9353f`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `UnlinkFromWorld` | `AActor::UnlinkFromWorld` 256–319 |
| `FixMapthingPos` | `AActor::FixMapthingPos` 328–402 |
| `LinkToWorld` | `AActor::LinkToWorld` 412–534 |
| `SetOrigin` | `AActor::SetOrigin` 536–544 |

### Out-of-body uncredit (681)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **271** |
| **Out-of-body removed** | **0** |
| **Honest** | **271** |


**Post-review ratio:** 256/271 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlActorWorldLinkTests.UnlinkFromWorld_DetachesSectorListAndBlockNodes`
- `VmPMapUtlActorWorldLinkTests.UnlinkFromWorld_SavesTouchingListsIntoContext`
- `VmPMapUtlActorWorldLinkTests.FixMapthingPos_PushesOffNearbyLine`
- `VmPMapUtlActorWorldLinkTests.FixMapthingPos_SkipsCoplanarTwoSidedLines`
- `VmPMapUtlActorWorldLinkTests.LinkToWorld_InsertsIntoSectorList`
- `VmPMapUtlActorWorldLinkTests.LinkToWorld_RestoresSavedSectorNodesFromContext`
- `VmPMapUtlActorWorldLinkTests.SetOrigin_RelinksAfterCoordinateChange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 681 --to 681` (0 hard / 0 soft).

## Seams

`P_DelSeclist`, `P_CreateSecNodeList`, `FBlockNode::Create`, `P_FindFloorCeiling`, and blockmap storage remain native.

## Credit rationale

Actor blockmap/sector link maintenance after path intercept chunks 668/680.
