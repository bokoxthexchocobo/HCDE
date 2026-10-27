# Chunk 575 — `A_DropInventory`, `A_SetBlend`, `A_CountdownArg`, `A_Burst`, and `A_Respawn` (`p_actionfunctions.cpp` **1989–2211**). `CheckRange` on **574** (**1954–1981**).

## Scope

`A_DropInventory`, `A_SetBlend`, `A_CountdownArg`, `A_Burst`, and `A_Respawn` (`p_actionfunctions.cpp` **1989–2211**). `CheckRange` on **574** (**1954–1981**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_DropInventory** 1989–2004 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetBlend** 2012–2032 (21 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CountdownArg** 2041–2065 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Burst** 2073–2123 (51 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2139–2211 (73 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmActionRangeInventory.cs`, `VmActionRespawn.cs`
Managed LOC (tally): 113.
Conversion commit: `dcd674340b48484ff9090af20c1f10785f4c4b3f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `ShouldDropInventory` | `A_DropInventory` **1989–2004** |
| `NormalizeBlendColor` | `A_SetBlend` **2012–2032** |
| `CountdownArgInRange` / `DecrementCountdownArg` / `ShouldJumpWhenCountdownZero` | `A_CountdownArg` **2041–2065** (explode/damage seam) |
| `BurstChunkNull` / `BurstChunkCount` / `BurstOffsetFromPr` / `BurstZFromPr` / `BurstShardVelocityZ` / `BurstVelocityFromPr` | `A_Burst` **2073–2123** (boss/unblock/destroy seam) |
| `UseTelefragMove` / `ApplySolidFlag` / `RestoreDefaultHeight` / target clear / flag merge / fog gate / `ApplyRespawnSuccess` | `A_Respawn` **2139–2211** |

### vs chunk **574**

| | Lines |
|--|------:|
| **574** (sight/range) | **1777–1981** |
| **575** (this row) | **1989–2211** |
| Gap | **1982–1988** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **186** |
| Net behavior-ported | **186** |


**Post-review ratio:** 113/186 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmActionRangeInventoryTests.ShouldDropInventory_WhenFound`
- `VmActionRangeInventoryTests.NormalizeBlendColor_ClearsWhite`
- `VmActionRangeInventoryTests.CountdownArgInRange_RejectsFive`
- `VmActionRangeInventoryTests.ShouldJumpWhenCountdownZero_AtZero`
- `VmActionRangeInventoryTests.BurstChunkCount_ScalesWithRadius`
- `VmActionRangeInventoryTests.BurstShardVelocityZ_MatchesNative2105`
- `VmActionRespawnTests.UseTelefragMove_WhenFlagSet`
- `VmActionRespawnTests.ApplyRespawnSuccess_RestoresHealthWhenOk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 575 --to 575` (0 hard / 0 soft).

## Seams

`DropInventory`, `CreateThinker`, `P_ExplodeMissile`, `P_DamageMobj`, `Spawn`, `P_TeleportMove`, `P_CheckPosition`, fog spawn, and monster counters are not hosted.

## Credit rationale

Inventory drop gating, blend color normalize, countdown arg bounds, burst shard math, and respawn flag/merge policy from retired **576** hull (disjoint **1989+**).
