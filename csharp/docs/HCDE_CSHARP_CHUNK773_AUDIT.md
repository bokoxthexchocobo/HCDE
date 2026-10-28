# Chunk 773 — `DThinker` instance lifecycle, profile CCMD, tick/GC, and `FThinkerIterator` (`dthinker.cpp` 804–1255; excludes `SerializeThinkers` 374–443 / chunk10; merged former chunk774).

## Scope

`DThinker` instance lifecycle, profile CCMD, tick/GC, and `FThinkerIterator` (`dthinker.cpp` 804–1255; excludes `SerializeThinkers` 374–443 / chunk10; merged former chunk774).

## Native ranges

- `src/playsim/dthinker.cpp` **DThinker::OnDestroy** 809–819 (11 lines)
- `src/playsim/dthinker.cpp` **DThinker::Serialize** 821–826 (6 lines)
- `src/playsim/dthinker.cpp` **DThinker::Remove** 851–870 (20 lines)
- `src/playsim/dthinker.cpp` **DThinker::PostBeginPlay** 878–880 (3 lines)
- `src/playsim/dthinker.cpp` **DThinker::CallPostBeginPlay** 894–901 (8 lines)
- `src/playsim/dthinker.cpp` **DThinker::ChangeStatNum** 935–947 (13 lines)
- `src/playsim/dthinker.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(DThinker, ChangeStatNum)** 973–979 (7 lines)
- `src/playsim/dthinker.cpp` **AddToTravellingList** 989–1023 (35 lines)
- `src/playsim/dthinker.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(DThinker, AddToTravellingList)** 1025–1030 (6 lines)
- `src/playsim/dthinker.cpp` **CCMD(profilethinkers)** 1038–1086 (49 lines)
- `src/playsim/dthinker.cpp` **DThinker::Tick** 1094–1096 (3 lines)
- `src/playsim/dthinker.cpp` **DThinker::CallTick** 1110–1116 (7 lines)
- `src/playsim/dthinker.cpp` **DThinker::PropagateMark** 1124–1135 (12 lines)
- `src/playsim/dthinker.cpp` **FThinkerIterator::FThinkerIterator** 1143–1158 (16 lines)
- `src/playsim/dthinker.cpp` **FThinkerIterator::FThinkerIterator(prev)** 1166–1189 (24 lines)
- `src/playsim/dthinker.cpp` **FThinkerIterator::Reinit** 1197–1201 (5 lines)
- `src/playsim/dthinker.cpp` **FThinkerIterator::Next** 1209–1255 (47 lines)

**Credited native LOC:** 272 (tally `nativeLoc` 272).

## Managed

`VmDThinkerInstance.cs`, `VmDThinkerIterator.cs`
Managed LOC (tally): 317.
Conversion commit: `e459e831413f1da08f380adafb7594ffc53cffe3`.

## Padding review (2026-10-21)

| Managed | Native (`dthinker.cpp`) |
|---------|-------------------------|
| `OnDestroy` / `Serialize` / `Remove` / `PostBeginPlay` / `ChangeStatNum` / `AddToTravellingList` | **809–1030** |
| `ParseProfileThinkersCommand` | `CCMD(profilethinkers)` **1038–1086** |
| `Tick` / `CallTick` / `PropagateMark` | **1094–1135** |
| `VmDThinkerIterator` | `FThinkerIterator` **1143–1255** |

### Out-of-body uncredit (773)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **272** |
| **Out-of-body removed** | **0** |
| **Honest** | **272** |


**Post-review ratio:** 317/272 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmDThinkerInstanceTests.Remove_UnlinksDoublyLinkedNode`
- `VmDThinkerInstanceTests.CallPostBeginPlay_UsesVirtualWhenPresent`
- `VmDThinkerInstanceTests.ChangeStatNumAction_BlocksTravellingAndStaticActor`
- `VmDThinkerInstanceTests.ParseProfileThinkersCommand_ParsesModes`
- `VmDThinkerInstanceTests.PropagateMark_MarksNeighbors`
- `VmDThinkerIteratorTests.Next_ReturnsMatchingThinkersAcrossFreshList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 773 --to 773` (0 hard / 0 soft).

## Seams

`FThinkerCollection::Link`, VM virtuals, `Printf`, travelling globals, GC mark, and `PClass` matching are injected. `VmDThinkerSerializeCollection` mirrors chunk10 `SerializeThinkers` for tests only (uncredited).

## Credit rationale

Per-thinker list/stat/travel helpers, profiling command parsing, and iterator traversal after chunk763; save-game array relink remains on chunk10.
