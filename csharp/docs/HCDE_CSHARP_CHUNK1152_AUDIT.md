# Chunk 1152 — State duration, SetState, and inventory teardown from `p_mobj.cpp` (**823–1044**).

## Scope

State duration, SetState, and inventory teardown from `p_mobj.cpp` (**823–1044**).

## Native ranges

- `src/playsim/p_mobj.cpp` **Actor state and inventory teardown** 823–1044 (222 lines)

**Credited native LOC:** 222 (tally `nativeLoc` 222).

## Managed

`VmPMobjState1152Slice.cs`
Managed LOC (tally): 163.
Conversion commit: `ee5a1b3460e5c402618e9712c5cb5bcb4ceab946`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetTics` (**834–846**) | yes |
| `AActor::SetState` (**856–961**) | yes |
| `DestroyAllInventory` (**978–1005**) | yes |
| `AActor::UseInventory` (**1024–1035**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **818–822** | `IsMapActor` sits before this hole |
| **858–859** | predicting-client log only |
| **963–969** and **1007–1012** | VM thunks forwarded to the host |
| **1037–1044** | `DropInventory` comment; the body begins at **1045** and is already credited |
| **1501–1685**, **2421–5075**, **5577–5726**, **7880–7900** | standing skips |

`SPR_FIXED` is **1** and `SPR_NOCHANGE` is **2**. `SUF_ACTOR` is **1**. `RF_FULLBRIGHT` is **0x0010**. `MF4_NOSKIN` is **0x00000020**. `OF_EuthanizeMe` is **1<<5**. `MF8_RECREATELIGHTS` is **0x00000100**. `LEVEL3_LIGHTCREATED` is **0x00080000**. The state loop limit is the literal **300000**: the check decrements first and fails on the **300000**th entry, after **299999** states. `GetTics` uses client tics on the client side. A fast actor on a fast state returns `duration - (duration >> 1)` and wins over slow. Otherwise a slow actor on a slow state returns `duration << 1`. A null state, a missing actor-use flag, or the loop limit destroys the actor and returns false before the light flags. `SPR_FIXED` keeps both sprite and frame. Otherwise the frame changes unless the same-frame flag is set, and `SPR_NOCHANGE` keeps the sprite. With no `MF4_NOSKIN`, a sprite equal to the spawn sprite uses the player skin when a player and at least one skin exist; otherwise it changes only when it differs from the previous state's sprite. Every other sprite is copied. An action that sets `OF_EuthanizeMe` returns false before the jump. A returned jump state forces tics to **0** and skips `GetNextState`. `nofunction` skips the action. After a live state, light associations, a lit new state, or a lit previous state set both light flags. `DestroyAllInventory` unlinks each item, then destroys the ones that are not already euthanized, in list order. `UseInventory` returns the VM result when a virtual exists and false otherwise.

| Managed (`VmPMobjState1152Slice.cs`) | Native (`p_mobj.cpp`) |
|--------------------------------------|------------------------|
| Tics, state, inventory | **823–1044** |
| Band hull (exclusive slice) | **823–1044** |

### Chunk **1152** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **222** |
| **Net headline** | **+222** vs post-**1151** |


**Post-review ratio:** 163/222 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjState1152SliceTests.GetTics_HalvesFastStatesBeforeDoublingSlowOnes`
- `VmPMobjState1152SliceTests.SetState_WalksZeroTicsAndKeepsASkinSprite`
- `VmPMobjState1152SliceTests.Inventory_DestroysSurvivorsAndUsesOnlyThroughTheVm`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1152 --to 1152` (0 hard / 0 soft).

## Seams

Skin lookup, action calls, and the use-inventory VM stay host-supplied. Predicting-client logging is not behavior.

## Credit rationale

UNCredited **`p_mobj.cpp`** **823–1044** (+222), inside the 9061-line file, with no conversion or recognition overlap.
