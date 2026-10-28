# Chunk 1025 — `DSpotState` list lookup, add/remove, getters, destroy reset, and savegame key policy from `a_specialspot.cpp` (**151–300**). Follows chunk **1024** (**1–150** `FSpotList`).

## Scope

`DSpotState` list lookup, add/remove, getters, destroy reset, and savegame key policy from `a_specialspot.cpp` (**151–300**). Follows chunk **1024** (**1–150** `FSpotList`).

## Native ranges

- `src/playsim/a_specialspot.cpp` **DSpotState spot registry** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmASpecialSpotSpotState1025Slice.cs`
Managed LOC (tally): 90.
Conversion commit: `65476103d6fa61423b3f10c177fe3e1d90b9eec0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DSpotState::OnDestroy` reset (**198–202**) | yes |
| `DSpotState::FindSpotList` (**220–228**) | yes |
| `DSpotState::AddSpot` / `RemoveSpot` (**236–254**) | yes |
| `DSpotState::Serialize` spots key (**262–266**) | yes (key gate only) |
| `GetNextInList` / distance / random getters (**274–305**) | yes (delegates to chunk **1024** list ops) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **151–180** | `FSpotList::GetRandomSpot` tail + `Serialize(FSpotList)` on chunk **1024** |
| **188–190**, **210–212** | empty ctor / empty `Tick` |
| **301–305** | hull ends **300**; function body at EOF (5 lines, defer without padding) |

| Managed (`VmASpecialSpotSpotState1025Slice.cs`) | Native (`a_specialspot.cpp`) |
|-------------------------------------------------|------------------------------|
| `OnDestroyNative` | **198–202** |
| `FindSpotList` | **220–228** |
| `AddSpotNative` / `RemoveSpotNative` | **236–254** |
| `ShouldWriteSpotListsArchiveKey` | **265** |
| `GetNextInListNative` | **274–279** |
| `GetSpotWithMinMaxDistanceNative` | **287–292** |
| `GetRandomSpotNative` | **300–305** (orchestration; body seam **301+**) |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1025** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1024** |


**Post-review ratio:** 90/150 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmASpecialSpotSpotState1025SliceTests.FindSpotList_CreatesListOnFirstUse`
- `VmASpecialSpotSpotState1025SliceTests.FindSpotList_NullTypeReturnsNullWithoutInsert`
- `VmASpecialSpotSpotState1025SliceTests.AddSpotNative_RegistersOnMatchingClassList`
- `VmASpecialSpotSpotState1025SliceTests.RemoveSpotNative_DelegatesToList`
- `VmASpecialSpotSpotState1025SliceTests.GetNextInListNative_ForwardsToListPolicy`
- `VmASpecialSpotSpotState1025SliceTests.GetSpotWithMinMaxDistanceNative_ForwardsToList`
- `VmASpecialSpotSpotState1025SliceTests.GetRandomSpotNative_IgnoresOnlyOnceLikeNativeList`
- `VmASpecialSpotSpotState1025SliceTests.OnDestroyNative_ClearsSpotListsBeforeSuper`
- `VmASpecialSpotSpotState1025SliceTests.ShouldWriteSpotListsArchiveKey_MatchesNativeKey`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1025 --to 1025` (0 hard / 0 soft).

## Seams

`Super::OnDestroy`, `Super::Serialize`, `FSerializer`, `PClassActor`, and `AActor` remain native.

## Credit rationale

Preflight `--band` + tally + recognition: **151–300** UNCredited; no live `VmASpecialSpot*` duplicate (retired chunk **874** off-tally only). Natural follow-on after **1024**.
