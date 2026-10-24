# Chunk 361 — CUSTOM_CVAR wipetype clamp

## Scope

CUSTOM_CVAR wipetype clamp

## Native ranges

- `src/d_main.cpp` **CUSTOM_CVAR wipetype** 485–488 (4 lines)

**Credited native LOC:** 4 (tally `nativeLoc` 4).

## Managed

`VmDMainWipePolicy.cs`, `VmDMainWipePolicyTests.cs`
Managed LOC (tally): 14.
Conversion commit: `fb7bd2727f8d1986707e79976195a56cb5d232b6`.

## Tests

- `VmDMainWipePolicyTests.Clamp_InvalidReturnsMelt`
- `VmDMainWipePolicyTests.IsValid_AcceptsInRange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 361 --to 361` (0 hard / 0 soft).

## Seams

Wipe drawer selection stays native; only invalid wipe index clamp is modeled.

## Credit rationale

Credits `CUSTOM_CVAR(wipetype)` bounds check against `wipe_None` and `wipe_NUMWIPES`.
