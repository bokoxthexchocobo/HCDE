# Chunk 458 — P_TouchSpecialThing, PronounMessage, ClientObituary gates

## Scope

P_TouchSpecialThing, PronounMessage, ClientObituary gates

## Native ranges

- `src/playsim/p_interaction.cpp` **P_TouchSpecialThing** 85–108 (24 lines)
- `src/playsim/p_interaction.cpp` **PronounMessage** 123–181 (59 lines)
- `src/playsim/p_interaction.cpp` **ClientObituary** 186–321 (136 lines)

**Credited native LOC:** 219 (tally `nativeLoc` 219).

## Managed

`VmPInteractionTouchObituary.cs`, `VmPInteractionTouchObituaryTests.cs`
Managed LOC (tally): 118.
Conversion commit: `a9eaca080f38357bab2c315f9d1c8ecb5b9b01f2`.

## Tests

- `VmPInteractionTouchObituaryTests.PickupOutOfReach_WhenAboveFeet`
- `VmPInteractionTouchObituaryTests.PronounCaseFromMarker_Grammar`
- `VmPInteractionTouchObituaryTests.ExpandPronoun_They`
- `VmPInteractionTouchObituaryTests.ShouldShowObituary_RequiresPlayerMo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 458 --to 458` (0 hard / 0 soft).

## Seams

`special->CallTouch`, `GStrings`, `AnnounceKill`, and `Printf` are not hosted.

## Credit rationale

Credits pickup Z reach, bot dest clear, pronoun expansion, and obituary message resolution from `p_interaction.cpp`.
