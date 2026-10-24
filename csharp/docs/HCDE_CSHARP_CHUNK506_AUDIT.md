# Chunk 506 — AActor::Die part 2 (player death, diestate, CallDie)

## Scope

AActor::Die part 2 (player death, diestate, CallDie)

## Native ranges

- `src/playsim/p_interaction.cpp` **AActor::Die** 622–796 (175 lines)
- `src/playsim/p_interaction.cpp` **AActor::Die** 798–807 (10 lines)
- `src/playsim/p_interaction.cpp` **AActor::CallDie** 809–817 (9 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmPInteractionActorDieStates.cs`, `VmPInteractionActorDieStatesTests.cs`
Managed LOC (tally): 110.
Conversion commit: `96e267fbd6d3efd332e52a8a265e8249f2e476a4`.

## Tests

- `VmPInteractionActorDieStatesTests.Unmorph_Destroy`
- `VmPInteractionActorDieStatesTests.Extreme_FromGib`
- `VmPInteractionActorDieStatesTests.Ice_GenericFreeze`
- `VmPInteractionActorDieStatesTests.Tics_Randomized`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 506 --to 506` (0 hard / 0 soft).

## Seams

`ClientObituary`, `SetState`, `FindState`, `Destroy`, and `VMCall` are not hosted.

## Credit rationale

Split **AActor::Die** part 2: player/bot death setup, unmorph destroy, extreme/typed/generic death state selection, tics jitter, and `CallDie` dispatch from `p_interaction.cpp` (622–817). Continues chunk 505 at line 622.
