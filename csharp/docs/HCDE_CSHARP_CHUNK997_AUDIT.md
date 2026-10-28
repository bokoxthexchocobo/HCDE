# Chunk 997 — `turbo` movement scale, save rotation clamps, `HCDE_InputSmoothMouseDelta`, and `turnspeeds` CCMD fill rules from `g_game.cpp` (**375–524**); follows chunk **996** (**225–374**).

## Scope

`turbo` movement scale, save rotation clamps, `HCDE_InputSmoothMouseDelta`, and `turnspeeds` CCMD fill rules from `g_game.cpp` (**375–524**); follows chunk **996** (**225–374**).

## Native ranges

- `src/g_game.cpp` **HCDE_InputSmoothMouseDelta** 375–524 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameInput997Slice.cs`
Managed LOC (tally): 136.
Conversion commit: `a5a392e3b27ffcf4b187c7bc58c5ea85d076f084`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `turbo` clamp + norm move scale (**388–407**) | yes |
| `autosavecount` clamp (**419–423**) | yes |
| `quicksaverotationcount` clamp (**428–432**) | yes |
| `HCDE_InputSmoothMouseDelta` (**451–466**) | yes |
| `turnspeeds` argv fill (**510–525**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **375–377** | `sv_gametype` close (chunk **996**) |
| **379–384**, **409–411** | MSVC pragma workaround |
| **413–417**, **425–426** | bare `CVAR` decls |
| **434–449**, **468–491** | static globals / constants |
| **495–505** | `Printf` host listing |
| **529+** | `CCMD(slot)` (next band) |

| Managed (`VmGGameInput997Slice.cs`) | Native (`g_game.cpp`) |
|-------------------------------------|----------------------|
| `ClampTurboPercent` / `ApplyTurboToMovementTables` | **390–406** |
| `NormalizeAutosaveCount` | **421–422** |
| `NormalizeQuicksaveRotationCount` | **430–431** |
| `SmoothMouseDelta` | **451–466** |
| `ApplyTurnspeedsConsoleArgs` | **510–525** |
| Band hull (exclusive slice) | **375–524** |

### Chunk **997** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**996** |


**Post-review ratio:** 136/150 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameInput997SliceTests.TurboClampAndScale`
- `VmGGameInput997SliceTests.SaveCountClamps`
- `VmGGameInput997SliceTests.SmoothMouseDelta`
- `VmGGameInput997SliceTests.TurnspeedsConsoleFillRules`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 997 --to 997` (0 hard / 0 soft).

## Seams

`CCMD(turnspeeds)` `Printf` listing (**497–504**) and bare `CVAR` decls remain host-injected.

## Credit rationale

UNCredited **375–524** CLI band; disjoint from chunks **995** / **996** and chunk **9** (**204–224**).
