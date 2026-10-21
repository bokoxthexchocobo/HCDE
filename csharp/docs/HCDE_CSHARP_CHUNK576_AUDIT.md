# Chunk 576 — CheckRange, A_DropInventory, A_SetBlend, A_CountdownArg, A_Burst (1954–2123)

## Scope

CheckRange, A_DropInventory, A_SetBlend, A_CountdownArg, A_Burst (1954–2123)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CheckRange** 1954–1981 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DropInventory** 1989–2004 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetBlend** 2012–2032 (21 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CountdownArg** 2041–2065 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Burst** 2073–2132 (60 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmActionRangeInventory.cs`
Managed LOC (tally): 75.
Conversion commit: `45ee31e3f06ce3ebbd16ff02529855b97aa2fa73`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SquaredRange` / `CheckRangeShouldReturnFalse` / `ShouldCheckCamera` | `CheckRange` 1960–1980 |
| `ShouldDropInventory` | `A_DropInventory` 1995–2001 |
| `NormalizeBlendColor` / `BlendAlphaSeconds` | `A_SetBlend` 2021–2030 |
| `CountdownArgInRange` / `DecrementCountdownArg` / `ShouldJumpWhenCountdownZero` | `A_CountdownArg` 2047–2055 |
| `BurstChunkCount` / `BurstOffsetFromPr` | `A_Burst` 2094–2107 |

**Post-review ratio:** 75/150 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmActionRangeInventoryTests.SquaredRange_Multiplies`
- `VmActionRangeInventoryTests.ShouldCheckCamera_WhenNonPlayerCamera`
- `VmActionRangeInventoryTests.ShouldDropInventory_WhenFound`
- `VmActionRangeInventoryTests.NormalizeBlendColor_ClearsWhite`
- `VmActionRangeInventoryTests.BlendAlphaSeconds_ConvertsTics`
- `VmActionRangeInventoryTests.CountdownArgInRange_RejectsFive`
- `VmActionRangeInventoryTests.ShouldJumpWhenCountdownZero_AtZero`
- `VmActionRangeInventoryTests.DecrementCountdownArg_SubtractsOne`
- `VmActionRangeInventoryTests.BurstChunkCount_ScalesWithRadius`
- `VmActionRangeInventoryTests.BurstOffsetFromPr_ScalesRadius`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 576 --to 576` (0 hard / 0 soft).

## Seams

`DropInventory`, `CreateThinker`, and state jumps are not hosted.

## Credit rationale

Faithful managed mirrors for squared range checks, drop guards, blend color normalization, and countdown arg bounds.
