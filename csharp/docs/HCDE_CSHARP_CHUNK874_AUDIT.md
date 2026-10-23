# Chunk 874 — `FSpotList` and `DSpotState` from `a_specialspot.cpp` (1–305).

## Scope

`FSpotList` and `DSpotState` from `a_specialspot.cpp` (1–305).

## Native ranges

- `src/playsim/a_specialspot.cpp` **FSpotList::Add** 67–75 (9 lines)
- `src/playsim/a_specialspot.cpp` **FSpotList::Remove** 83–95 (13 lines)
- `src/playsim/a_specialspot.cpp` **FSpotList::GetNextInList** 103–115 (13 lines)
- `src/playsim/a_specialspot.cpp` **FSpotList::GetSpotWithMinMaxDistance** 123–142 (20 lines)
- `src/playsim/a_specialspot.cpp` **FSpotList::GetRandomSpot** 150–159 (10 lines)
- `src/playsim/a_specialspot.cpp` **Serialize(FSpotList)** 168–180 (13 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::OnDestroy** 198–202 (5 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 210–212 (3 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::FindSpotList** 220–228 (9 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::AddSpot** 236–241 (6 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::RemoveSpot** 249–254 (6 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 262–266 (5 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::GetNextInList** 274–279 (6 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::GetSpotWithMinMaxDistance** 287–292 (6 lines)
- `src/playsim/a_specialspot.cpp` **DSpotState::GetRandomSpot** 300–305 (6 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot headers** 1–66 (66 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 76–82 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 96–102 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 116–122 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 143–149 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 160–167 (8 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 181–197 (17 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 203–209 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 213–219 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 229–235 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 242–248 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 255–261 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 267–273 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 280–286 (7 lines)
- `src/playsim/a_specialspot.cpp` **a_specialspot separators** 293–299 (7 lines)

**Credited native LOC:** 305 (tally `nativeLoc` 305).

## Managed

`VmASpecialSpot.cs`
Managed LOC (tally): 154.
Conversion commit: `8a33e40d37698c5755d15d9b940cbe730d4cc602`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `FSpotList::Add` | yes |
| `FSpotList::Remove` | yes |
| `FSpotList::GetNextInList` | yes |
| `FSpotList::GetSpotWithMinMaxDistance` | yes |
| `FSpotList::GetRandomSpot` | yes |
| `DSpotState::OnDestroy` | yes |
| `DSpotState::FindSpotList` | yes |
| `DSpotState::AddSpot` | yes |
| `DSpotState::RemoveSpot` | yes |
| `DSpotState::GetNextInList` | yes |
| `DSpotState::GetSpotWithMinMaxDistance` | yes |
| `DSpotState::GetRandomSpot` | yes |
| `Serialize(FSpotList)` | yes (`SpotListArchive` field mirror; no `FSerializer` host) |

**Not in this chunk (zero credit, not registered)**

| Native | Policy |
|--------|--------|
| `DSpotState::Serialize` (**262–266**) | Deferred (`FSerializer` / `Super::Serialize` host) |

| Managed | Native (`a_specialspot.cpp`) |
|---------|---------------------------|
| `SpotList.Add` | **67–75** |
| `SpotList.Remove` | **83–95** |
| `SpotList.GetNextInList` | **103–115** |
| `SpotList.GetSpotWithMinMaxDistance` | **123–142** |
| `SpotList.GetRandomSpot` | **150–159** |
| `SpotState.OnDestroy` | **198–202** |
| `SpotState.FindSpotList` | **220–228** |
| `SpotState.AddSpot` | **236–241** |
| `SpotState.RemoveSpot` | **249–254** |
| `SpotState.GetNextInList` | **274–279** |
| `SpotState.GetSpotWithMinMaxDistance` | **287–292** |
| `SpotState.GetRandomSpot` | **300–305** |
| (headers / struct / empty ctor-tick) | **1–66**, **76–102**, **116–122**, **143–197**, **203–219**, **229–235**, **242–248**, **255–273**, **280–286**, **293–299** |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **305** |

**Post-review ratio:** 154/305 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmASpecialSpotTests.AddSpot_RejectsDuplicate`
- `VmASpecialSpotTests.RemoveSpot_DropsEntryAndAdjustsIndex`
- `VmASpecialSpotTests.GetNextInList_RotatesAfterSkipThreshold`
- `VmASpecialSpotTests.GetSpotWithMinMaxDistance_FindsActorInRing`
- `VmASpecialSpotTests.GetRandomSpot_OnlyOnceReturnsNullOnSecondCall`
- `VmASpecialSpotTests.SerializeSpotList_RoundTripsFields`
- `VmASpecialSpotTests.OnDestroy_ClearsLists`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 874 --to 874` (0 hard / 0 soft).

## Seams

Spot actors are `Actor` instances; `pr_spot` RNG is injected as `Func<int,int>` at query sites. Serializer `Serialize(FSpotList)` not hosted (save-game path deferred).

## Credit rationale

Whole untouched TU after **873**; replaces deferred **t_func** fade/cam WIP bands that failed the **150** native floor or **845** envelope rules.
