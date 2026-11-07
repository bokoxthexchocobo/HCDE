# Chunk 1460 — ACS call cases UniqueTID through StopSound in p_acs.cpp (5967-6148).

## Scope

ACS call cases UniqueTID through StopSound in p_acs.cpp (5967-6148).

## Native ranges

- `src/playsim/p_acs.cpp` **ACSF_UniqueTID** 5967–6148 (182 lines)

**Credited native LOC:** 182 (tally `nativeLoc` 182).

## Managed

`VmAcsCall1460Slice.cs`
Managed LOC (tally): 204.
Conversion commit: `39d418db1185cb8e2f941919359f90d26d56ffee`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| UniqueTID through StopSound (**5967–6148**) | yes |
| Band hull (**5967–6148**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Previous case break | lines 5965-5966 stay outside this case run |
| SoundVolume | line 6150 is already credited |

A short argument list returns 0 and records the required count unless the compat flag skips the check. Sqrt floors the integer square root. FixedSqrt and VectorLength convert through 16.16. PlaySound on tid 0 plays the activator and then the tid iterator. StopSound on tid 0 stops only the activator.

| Managed (`VmAcsCall1460Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------|---------------------|
| Call through ToAcs | **5967–6148** |
| Band hull (exclusive slice) | **5967–6148** |

### Chunk **1460** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **182** |
| **Net headline** | **+182** vs the prior chunk |


**Post-review ratio:** 204/182 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsCall1460SliceTests.MathHudAndCvarsFollowTheCaseRules`
- `VmAcsCall1460SliceTests.LineAttackAndSoundWalkEveryTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1460 --to 1460` (0 hard / 0 soft).

## Seams

case ACSF_UniqueTID through the StopSound break. The previous case break at 5965-5966 stays outside. ACSF_SoundVolume at 6150 is already credited. The late-join body in d_net.cpp is already partially credited and is not a contiguous 150-line hole.

## Credit rationale

UNCredited p_acs.cpp 5967-6148 (+182). One contiguous run of call-function cases. The span does not cross credited code.
