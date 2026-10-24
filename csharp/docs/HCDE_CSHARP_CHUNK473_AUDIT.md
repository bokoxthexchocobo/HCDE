# Chunk 473 — DamageMobj reflection prelude, DoDamageMobj/P_DamageMobj wrappers, PoisonMobj and CallOkayToSwitchTarget glue

## Scope

DamageMobj reflection prelude, DoDamageMobj/P_DamageMobj wrappers, PoisonMobj and CallOkayToSwitchTarget glue

## Native ranges

- `src/playsim/p_interaction.cpp` **DamageMobj** 1281–1319 (39 lines)
- `src/playsim/p_interaction.cpp` **DoDamageMobj** 1573–1603 (31 lines)
- `src/playsim/p_interaction.cpp` **AActor::DamageMobj** 1605–1615 (11 lines)
- `src/playsim/p_interaction.cpp` **P_DamageMobj** 1617–1657 (41 lines)
- `src/playsim/p_interaction.cpp` **AActor::PoisonMobj** 1715–1726 (12 lines)
- `src/playsim/p_interaction.cpp` **AActor::OkayToSwitchTarget** 1803–1808 (6 lines)
- `src/playsim/p_interaction.cpp` **AActor::CallOkayToSwitchTarget** 1810–1821 (12 lines)
- `src/playsim/p_interaction.cpp` **P_PoisonDamage** 1982–1990 (9 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmPInteractionVirtualDamage.cs`, `VmPInteractionVirtualDamageTests.cs`
Managed LOC (tally): 123.
Conversion commit: `8389c958b4db9d7b07bc172f81cc5d0428da0e9b`.

## Tests

- `VmPInteractionVirtualDamageTests.ReflectionScan_PicksHigherReflect`
- `VmPInteractionVirtualDamageTests.Kickback_BlocksThrustlessFlag`
- `VmPInteractionVirtualDamageTests.DoDamageMobj_MirrorSkips`
- `VmPInteractionVirtualDamageTests.ClampDoDamageReturn_NegativeToZero`
- `VmPInteractionVirtualDamageTests.PDamageMobj_RecordsPositiveDamage`
- `VmPInteractionVirtualDamageTests.PoisonMobjRequiresInflictor`
- `VmPInteractionVirtualDamageTests.CallOkaySwitch_FallsBackWhenNoVirtual`
- `VmPInteractionVirtualDamageTests.CallOkaySwitch_UsesVirtualReturn`
- `VmPInteractionVirtualDamageTests.PoisonDamagePlaysound`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 473 --to 473` (0 hard / 0 soft).

## Seams

`VMCall`, `P_DamageMobj` body, `HCDERewind_LagComp`, and net record hooks are not hosted.

## Credit rationale

Credits power-reflection scan, kickback eligibility, mirror/coop early outs, lag-comp record gates, and virtual OkayToSwitch/Poison action wrappers from `p_interaction.cpp` (1281–1319, 1573–1657, 1715–1726, 1803–1821, 1982–1990).
