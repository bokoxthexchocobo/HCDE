# Chunk 468 — DamageMobj early filters through spectral/special damage

## Scope

DamageMobj early filters through spectral/special damage

## Native ranges

- `src/playsim/p_interaction.cpp` **DamageMobj** 1098–1280 (183 lines)

**Credited native LOC:** 183 (tally `nativeLoc` 183).

## Managed

`VmPEnemyLookEx.cs`, `VmPEnemyLookExTests.cs`, `VmPInteractionDamageEarly.cs`, `VmPInteractionDamageEarlyTests.cs`, `VmPMapBounceTraverse.cs`, `VmPMapBounceTraverseTests.cs`, `VmPMobjShadePitch.cs`, `VmPMobjShadePitchTests.cs`
Managed LOC (tally): 100.
Conversion commit: `4d6fead63aeae227fdcfd7c8e0f87d411e66042a`.

## Tests

- `VmPInteractionDamageEarlyTests.DormantBlocks_ReturnsMinusOne`
- `VmPInteractionDamageEarlyTests.SpectralPlayerBlock_ReturnsMinusOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 468 --to 468` (0 hard / 0 soft).

## Seams

`CallDoSpecialDamage`, `P_AutoUseHealth`, and full `P_DamageMobj` tail are not hosted.

## Credit rationale

Credits telefrag/dormant gates, skill scaling branches, and spectral hit rules from `p_interaction.cpp` (1098–1280).
