# Chunk 449 — P_ActorInSpecialSector negative heal and secret discovery

## Scope

P_ActorInSpecialSector negative heal and secret discovery

## Native ranges

- `src/playsim/p_spec.cpp` **P_ActorInSpecialSector heal and secret** 478–501 (24 lines)

**Credited native LOC:** 24 (tally `nativeLoc` 24).

## Managed

`VmPSpecActorSpecialFinish.cs`, `VmPSpecActorSpecialFinishTests.cs`
Managed LOC (tally): 19.
Conversion commit: `8eee7ab9bd6127fa76a281a87041e2ec06449e2e`.

## Tests

- `VmPSpecActorSpecialFinishTests.NegativeDamageHealTick`
- `VmPSpecActorSpecialFinishTests.PlayerTouchesSecret`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 449 --to 449` (0 hard / 0 soft).

## Seams

`P_GiveBody`, `P_GiveSecret`, and sector secret flags are not hosted.

## Credit rationale

Credits negative-damage heal ticks, secret-sector touch policy, and clear-secret wiring from `p_spec.cpp`.
