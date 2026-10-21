# Chunk 526 — AActor::A_Teleport

## Scope

AActor::A_Teleport

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::A_Teleport** 3255–3423 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmActionTeleport.cs`, `VmActionTeleportTests.cs`
Managed LOC (tally): 92.
Conversion commit: `70677c458e096466b7dd4d1d2109e8014412c9a4`.

## Tests

- `VmActionTeleportTests.BlockedByNoteleport_WithoutOverride`
- `VmActionTeleportTests.RandomDecideIndex_Clamps`
- `VmActionTeleportTests.ComputeFinalZ_CeilingClamp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 526 --to 526` (0 hard / 0 soft).

## Seams

`P_TeleportMove`, `GetSpotState`, `P_SpawnTeleportFog`, and state jumps are not hosted.

## Credit rationale

Credits A_Teleport spot selection, random-decide gate, sensitive-Z rejection, and fog/angle/velocity flags from `p_actionfunctions.cpp` (3255–3423).
