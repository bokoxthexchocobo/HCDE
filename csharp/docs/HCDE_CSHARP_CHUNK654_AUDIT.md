# Chunk 654 — Whole `SoundEngine::StartSound` (`s_sound.cpp` 398–639). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Whole `SoundEngine::StartSound` (`s_sound.cpp` 398–639). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7142030b5546047a61b6949af10bcfaee56eae6a`.

## Padding review (2026-10-21)

| Managed | Native (`s_sound.cpp`) |
|---------|-------------------------|
| `StartSound` / `StartSoundContext` | `StartSound` 398–639 |
| `SfxHasLink` / `ResolveNextSfxLink` | 441–463 |

### Out-of-body uncredit (654)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **242** |
| **Out-of-body removed** | **242** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 654 --to 654` (0 hard / 0 soft).

## Seams

`GSnd`, `CalcPosVel`, `StopChannel`, and `Joy_Rumble` callbacks on `StartSoundContext`.

## Credit rationale

Full-body `StartSound` hosted with explicit context seams.
