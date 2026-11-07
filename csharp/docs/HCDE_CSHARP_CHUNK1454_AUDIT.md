# Chunk 1454 — Ready, invasion console commands, pings, and kick in d_net.cpp (10127-10385).

## Scope

Ready, invasion console commands, pings, and kick in d_net.cpp (10127-10385).

## Native ranges

- `src/d_net.cpp` **ReadyPlayer** 10127–10385 (259 lines)

**Credited native LOC:** 259 (tally `nativeLoc` 259).

## Managed

`VmNet1454Slice.cs`
Managed LOC (tally): 239.
Conversion commit: `cc1fb1754ac4f2f8715279d9c218aee5fd88bd94`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Ready, preset, pings, and kick (**10127–10385**) | yes |
| Band hull (**10127–10385**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Invasion accessors | lines 9679-9911 are already credited |
| ZScript export wrappers | lines 9914-10125 stay for later |
| Mute commands | start at line 10387 |

Ready writes DEM_READIED only during a live net game. The invasion preset requires the local service authority and clears fast monsters. Kick ignores the console player and duplicate client numbers, and it refuses the command away from the authority.

| Managed (`VmNet1454Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| Ready through Kick | **10127–10385** |
| Band hull (exclusive slice) | **10127–10385** |

### Chunk **1454** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **259** |
| **Net headline** | **+259** vs the prior chunk |


**Post-review ratio:** 239/259 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1454SliceTests.ReadyPresetAndStatus`
- `VmNet1454SliceTests.SpotsPingsAndKick`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1454 --to 1454` (0 hard / 0 soft).

## Seams

ReadyPlayer through CCMD kick. Net_ControlInvasion ends at 9911. The ZScript wrappers occupy 9914-10125. CCMD mute starts at 10387.

## Credit rationale

UNCredited d_net.cpp 10127-10385 (+259). One contiguous span of the ready command, the invasion preset, and kick. The span steps over the small export macros inside it and does not cross credited code.
