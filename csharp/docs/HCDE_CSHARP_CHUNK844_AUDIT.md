# Chunk 844 — `FParser::SF_PlayerKeys` … `SF_SetWeapon` from `fragglescript/t_func.cpp` (2394–2684).

## Scope

`FParser::SF_PlayerKeys` … `SF_SetWeapon` from `fragglescript/t_func.cpp` (2394–2684).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerKeys** 2394–2427 (34 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerAmmo** 2436–2444 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MaxPlayerAmmo** 2453–2461 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerWeapon** 2471–2547 (77 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerSelectedWeapon** 2556–2600 (45 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_GiveInventory** 2608–2623 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_TakeInventory** 2631–2646 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_CheckInventory** 2654–2669 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetWeapon** 2677–2684 (8 lines)

**Credited native LOC:** 230 (tally `nativeLoc` 230).

## Managed

`VmFScriptParserSfInventory.cs`
Managed LOC (tally): 266.
Conversion commit: `93e33e70639818caa77d2476dc5fd92730542a68`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_PlayerKeys` | yes |
| `SF_PlayerAmmo` | yes |
| `SF_MaxPlayerAmmo` | yes |
| `SF_PlayerWeapon` | yes |
| `SF_PlayerSelectedWeapon` | yes |
| `SF_GiveInventory` | yes |
| `SF_TakeInventory` | yes |
| `SF_CheckInventory` | yes |
| `SF_SetWeapon` | yes |

| Managed | Native (`t_func.cpp` 2394–2684) |
|---------|-------------------------|
| `SfPlayerKeys` | **2394–2427** |
| `SfPlayerAmmo` | **2436–2444** |
| `SfMaxPlayerAmmo` | **2453–2461** |
| `SfPlayerWeapon` | **2471–2547** |
| `SfPlayerSelectedWeapon` | **2556–2600** |
| `SfGiveInventory` | **2608–2623** |
| `SfTakeInventory` | **2631–2646** |
| `SfCheckInventory` | **2654–2669** |
| `SfSetWeapon` | **2677–2684** |
| (separators) | **2428–2435**, **2445–2452**, **2462–2470**, **2548–2555**, **2601–2607**, **2624–2630**, **2647–2653**, **2670–2676** |

### Out-of-body uncredit (844)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **291** |
| **Out-of-body removed** | **61** |
| **Honest** | **230** |


**Post-review ratio:** 266/230 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfInventoryTests.SfPlayerKeys_ReadBlueCard`
- `VmFScriptParserSfInventoryTests.SfPlayerAmmo_ReadBullets`
- `VmFScriptParserSfInventoryTests.SfMaxPlayerAmmo_ReturnsCap`
- `VmFScriptParserSfInventoryTests.SfPlayerWeapon_OwnsShotgun`
- `VmFScriptParserSfInventoryTests.SfPlayerWeapon_TakeRemovesOwnedWeapon`
- `VmFScriptParserSfInventoryTests.SfPlayerSelectedWeapon_ReturnsSelectedIndex`
- `VmFScriptParserSfInventoryTests.SfGiveInventory_GivesKey`
- `VmFScriptParserSfInventoryTests.SfCheckInventory_FindsArmor`
- `VmFScriptParserSfInventoryTests.SfSetWeapon_SelectsOwned`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 844 --to 844` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. Inventory uses existing `AcsPlayerInventory`, `GetPlayerNum` / `GetPlayerActor` / `GetAmmo` / `ClassType`, and `PlayerInventory` weapon bits.

## Credit rationale

Legacy Fraggle inventory band after **843** math; **1381–2241** sector/camera band remains deferred.
