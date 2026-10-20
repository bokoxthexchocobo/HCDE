# Chunk 524 — DLevelScript::DoSetActorProperty

## Scope

DLevelScript::DoSetActorProperty

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::DoSetActorProperty** 4166–4406 (241 lines)

**Credited native LOC:** 241 (tally `nativeLoc` 241).

## Managed

`VmAcsDoSetActorProperty.cs`, `VmAcsDoSetActorPropertyTests.cs`
Managed LOC (tally): 197.
Conversion commit: `3ddd69fd85a6f749cce13481c8f8153bcc7c8ed4`.

## Tests

- `VmAcsDoSetActorPropertyTests.ApplyHealth_KillsWhenNonPositive`
- `VmAcsDoSetActorPropertyTests.TryMapLegacyRenderStyle_FindsIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 524 --to 524` (0 hard / 0 soft).

## Seams

`actor->Die`, `Level->Behaviors.LookupString`, and full `APROP_*` switch are not hosted.

## Credit rationale

Credits ACS actor property health/damage/style/scale dispatch guards from `p_acs.cpp` (4166–4406).
