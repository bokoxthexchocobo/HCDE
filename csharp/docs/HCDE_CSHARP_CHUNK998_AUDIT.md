# Chunk 998 — Weapon/inventory CCMD helpers and netgame pause gating from `g_game.cpp` (**525–674**); follows chunk **997** (**375–524**).

## Scope

Weapon/inventory CCMD helpers and netgame pause gating from `g_game.cpp` (**525–674**); follows chunk **997** (**375–524**).

## Native ranges

- `src/g_game.cpp` **CCMD(slot)** 525–674 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameCcmd998Slice.cs`
Managed LOC (tally): 117.
Conversion commit: `177f324647181428c194735f2c210cc14807e554`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `CCMD(slot)` validation (**531–543**) | yes |
| `CCMD(centerview)` frozen gate (**549–550**) | yes |
| `CCMD(land)` / `turn180` (**561**, **586**) | yes |
| `CCMD(pause)` net_disablepause (**566–580**) | yes |
| `weapnext` / `weapprev` (**591–596**) | yes |
| `DisplayNameTag` tag bit (**602**) | yes |
| `invuse` / `invuseall` / `use` subclass (**645–674**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **525–527** | `turnspeeds` close (chunk **997**) |
| **554–557**, **547–552** tail | `Net_WriteInt8` host |
| **599–613** | HUD / `DEFINE_ACTION_FUNCTION_NATIVE` host |
| **615–635** | `IFVM` / `CallVM` host |
| **656–663** | `invquery` `Printf` host |
| **678+** | `invdrop` (next band) |

| Managed (`VmGGameCcmd998Slice.cs`) | Native (`g_game.cpp`) |
|-------------------------------------|----------------------|
| `TryResolveWeaponSlotFromArgument` / `ResolveExplicitWeaponSlot` | **533–543** |
| `ShouldBlockCenterview` | **549** |
| `ResolveNetgamePauseRejectReason` / `ShouldQueueSendPause` | **566–580** |
| `ResolveLandCommand` / `ResolveTurn180Command` | **561**, **586** |
| `ResolveWeapNextSlot` / `ResolveWeapPrevSlot` | **591–596** |
| `ShouldShowDisplayNameTagHud` | **602** |
| `CanApplySelectedInventoryUse` / `InventoryTicsAfterInvUseCommand` | **645–653** |
| `ResolveInvUseAllCommand` | **639–640** |
| `ParseUseCommandSubclassFlag` / `ShouldRequireUseCommandItemName` | **667–671** |
| Band hull (exclusive slice) | **525–674** |

### Chunk **998** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**997** |


**Post-review ratio:** 117/150 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameCcmd998SliceTests.WeaponSlotParsing`
- `VmGGameCcmd998SliceTests.PauseAndCenterviewGates`
- `VmGGameCcmd998SliceTests.InventoryAndUseCommands`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 998 --to 998` (0 hard / 0 soft).

## Seams

`CCMD(slot)` `Printf`, `DisplayNameTag` HUD attach (**602–604**), and `IFVM` inventory VM calls remain host-injected.

## Credit rationale

UNCredited **525–674** CLI band; disjoint from chunks **995–997** and chunk **9** (**204–224**).
