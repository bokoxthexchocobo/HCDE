# Chunk 876 — Corpse queue and `A_Unblock` from `a_action.cpp` (1–169).

## Scope

Corpse queue and `A_Unblock` from `a_action.cpp` (1–169).

## Native ranges

- `src/playsim/a_action.cpp` **a_action headers** 1–36 (36 lines)
- `src/playsim/a_action.cpp` **HCDE_QueueCorpse** 37–79 (43 lines)
- `src/playsim/a_action.cpp` **a_action separators** 80–86 (7 lines)
- `src/playsim/a_action.cpp` **A_Unblock** 87–136 (50 lines)
- `src/playsim/a_action.cpp` **a_action separators** 137–137 (1 lines)
- `src/playsim/a_action.cpp` **corpse queue comment** 138–143 (6 lines)
- `src/playsim/a_action.cpp` **a_action separators** 144–145 (2 lines)
- `src/playsim/a_action.cpp` **A_QueueCorpse** 146–155 (10 lines)
- `src/playsim/a_action.cpp` **a_action separators** 156–157 (2 lines)
- `src/playsim/a_action.cpp` **A_DeQueueCorpse** 158–169 (12 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmAAction.cs`
Managed LOC (tally): 108.
Conversion commit: `b4ae825fc62c9510704de31c7f8af30216651dfc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `HCDE_QueueCorpse` | yes |
| `A_Unblock` | yes |
| `A_QueueCorpse` | yes |
| `A_DeQueueCorpse` | yes |

| Managed | Native (`a_action.cpp`) |
|---------|------------------------|
| `QueueCorpse` | **37–79** |
| `Unblock` | **87–136** |
| `QueueCorpseAction` | **146–155** |
| `DeQueueCorpse` | **158–169** |
| (headers / comments / separators) | **1–36**, **80–86**, **137–145**, **156–157** |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **169** |

**Post-review ratio:** 108/169 managed/native ≈ 64% (src file LOC at HEAD; see tally).

## Tests

- `VmAActionTests.QueueCorpse_SkipsWhenFilterDisabled`
- `VmAActionTests.QueueCorpse_EvictsOldestAtCapacity`
- `VmAActionTests.Unblock_ClearsSolidAndQueuesCorpse`
- `VmAActionTests.Unblock_StealthRestoresAlpha`
- `VmAActionTests.QueueCorpseAction_RequiresCorpseFlag`
- `VmAActionTests.DeQueueCorpse_RemovesSelf`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 876 --to 876` (0 hard / 0 soft).

## Seams

`P_DropItem` / `PClass::FindActor` supplied at unblock sites; corpse list is host-owned `IList<Actor>`.

## Credit rationale

Dense logic TU under **170** native; complements existing `ActorUnblockActions` dispatch without re-crediting unrelated action bands.
