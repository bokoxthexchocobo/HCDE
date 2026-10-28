# p_mobj.cpp P_PlaySpawnSound — chunk 19 audit

## Scope

Credit existing managed `ActorSoundActions.PlaySpawnSound` against native `P_PlaySpawnSound` (**7880–7900**, 21 lines): `SeeSound` gate, default missile voice channel, spawner weapon channel when `MF_SPAWNSOUNDSOURCE`, and silent-sector position fallback.

**Managed LOC:** 24 (`ActorSoundActions.PlaySpawnSound` body).

## Tests

- `ProjectileSpawnSoundRoutingTests`
- Wired from `AuthoritySimulation.SpawnProjectileCore` (existing)

## Verified

`python3 csharp/tools/generate_native_managed_matrix.py`; `dotnet build -warnaserror`; full `dotnet test`.
