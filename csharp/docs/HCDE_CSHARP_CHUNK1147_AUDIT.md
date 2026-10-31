# Chunk 1147 — Random chase direction from `p_enemy.cpp` (**1100–1257**); follows **1146** after the short **451–481** gap.

## Scope

Random chase direction from `p_enemy.cpp` (**1100–1257**); follows **1146** after the short **451–481** gap.

## Native ranges

- `src/playsim/p_enemy.cpp` **P_RandomChaseDir** 1100–1257 (158 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmPEnemyChase1147Slice.cs`
Managed LOC (tally): 143.
Conversion commit: `0c3810a17369aa9e784e641bb5718e88d46a1874`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Friendly player steer (**1116–1195**) | yes |
| Keep-heading and turn search (**1197–1219**) | yes |
| Turnaround and give-up (**1246–1256**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **301–450** | missile range already credited by **1146** |
| **451–481** | 31-line gap under the floor |
| **1220–1245** | commented-out search, not live |
| **1268+** | `P_IsVisible` |

`MF_FRIENDLY` is **0x08000000**. `MF8_DONTFOLLOWPLAYERS` is **0x04000000** and skips the player steer. `MAXPLAYERS` is **64**. A friend player uses `FriendPlayer - 1`. Otherwise single-player uses slot **0**, and multiplayer starts at `pr_newchasedir() & 63` and walks the mask. The steer block runs when that roll is odd or the monster cannot see the player. An axis counts only past **128**. The diagonal is `diags[((Y<0)<<1) + (X>0)]`: northwest, northeast, southwest, southeast. It is skipped when it is the turnaround. The two axes swap when the next roll is above **200** or `|Y| > |X|`, then a turnaround axis becomes `DI_NODIR`. A roll below **150** retries the current direction before any turn. The search then steps `±1` from `DI_NODIR` replaced by `roll & 7`, skipping the turnaround. Only a successful turnaround stores `movecount = roll & 15`. Failure sets `DI_NODIR`.

| Managed (`VmPEnemyChase1147Slice.cs`) | Native (`p_enemy.cpp`) |
|------------------------------------------|----------------------|
| `P_RandomChaseDir` | **1100–1257** |
| Band hull (exclusive slice) | **1100–1257** |

### Chunk **1147** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **158** |
| **Net headline** | **+158** vs post-**1146** |


**Post-review ratio:** 143/158 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyChase1147SliceTests.RandomChaseDir_TurnsAwayFromTheReverseAndKeepsAClearHeading`
- `VmPEnemyChase1147SliceTests.RandomChaseDir_SteersAFriendlyTowardThePlayer`
- `VmPEnemyChase1147SliceTests.RandomChaseDir_ReplacesNoDirBeforeTheSearchAndSkipsABlockedReverse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1147 --to 1147` (0 hard / 0 soft).

## Seams

Player slots, sight, and `P_TryWalk` stay host-supplied. The RNG is a scripted `pr_newchasedir` sequence. An empty multiplayer mask stops after **64** slots.

## Credit rationale

UNCredited **`p_enemy.cpp`** **1100–1257** (+158), inside the 3488-line file, with no conversion or recognition overlap.
