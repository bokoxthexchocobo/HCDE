# Chunk 763 — Thinker list ops, destroy/save/load, tick/profile, and client-side collection tick (`dthinker.cpp` 210–307, 452–795, 567–699; excludes chunk762 71–366 and SerializeThinkers 374+).

## Scope

Thinker list ops, destroy/save/load, tick/profile, and client-side collection tick (`dthinker.cpp` 210–307, 452–795, 567–699; excludes chunk762 71–366 and SerializeThinkers 374+).

## Native ranges

- `src/playsim/dthinker.cpp` **RecreateDynamicLights** 210–223 (14 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::RunClientSideThinkers** 225–307 (83 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::AddTail** 452–475 (24 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::FirstThinker** 483–501 (19 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::MarkRoots** 509–517 (9 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::GetHead** 525–533 (9 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::GetTail** 541–548 (8 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::IsEmpty** 556–559 (4 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::TickThinkers** 707–745 (39 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::ProfileThinkers** 752–795 (44 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::DestroyThinkers** 567–573 (7 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::DoDestroyThinkers** 581–629 (49 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::SaveList** 637–649 (13 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::RemoveTravellers** 657–675 (19 lines)
- `src/playsim/dthinker.cpp` **FThinkerList::OnLoad** 683–699 (17 lines)

**Credited native LOC:** 358 (tally `nativeLoc` 358).

## Managed

`VmDThinkerListOps.cs`, `VmDThinkerClientSide.cs`, `VmDThinkerListDestroy.cs`
Managed LOC (tally): 425.
Conversion commit: `c2db57e528ee481b32d4d3f0e7879b8e4be517a9`.

## Padding review (2026-10-21)

| Managed | Native (`dthinker.cpp`) |
|---------|-------------------------|
| `RecreateDynamicLights` | **210–223** |
| `RunClientSideThinkers` | **225–307** |
| `AddTail` / `GetHead` / `GetTail` / `IsEmpty` | **452–559** |
| `FirstThinker` / `MarkRoots` | **483–517** |
| `DestroyThinkers` / `DoDestroyThinkers` | **567–629** |
| `SaveList` / `RemoveTravellers` / `OnLoad` | **637–699** |
| `TickThinkers` / `ProfileThinkers` | **707–795** |


**Post-review ratio:** 425/358 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmDThinkerClientSideTests.RecreateDynamicLights_ClearsFlagAndRunsEffect`
- `VmDThinkerClientSideTests.RunClientSideThinkers_TicksFreshUntilStableAndAdvancesTimers`
- `VmDThinkerClientSideTests.RunClientSideThinkers_SkipsWorldTimerWhenPaused`
- `VmDThinkerListOpsTests.AddTail_LinksCircularList`
- `VmDThinkerListOpsTests.FirstThinker_PrefersMainListThenFresh`
- `VmDThinkerListOpsTests.MarkRoots_InvokesMarkForEachSentinel`
- `VmDThinkerListOpsTests.TickThinkers_MovesJustSpawnedToDestAndTicks`
- `VmDThinkerListOpsTests.ProfileThinkers_AccumulatesClassProfiles`
- `VmDThinkerListDestroyTests.DoDestroyThinkers_ClearsSentinelAndMarksNodes`
- `VmDThinkerListDestroyTests.RemoveTravellers_StripsFlagWhenSaveGame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 763 --to 763` (0 hard / 0 soft).

## Seams

Sentinel/GC, `DThinker::Remove`, iterators, `SetDynamicLights`, `P_RunEffect`, level lights, and VM hooks are injected.

## Credit rationale

Circular list mechanics plus client prediction tick path (fresh drain, dynamic lights, light list tick).
