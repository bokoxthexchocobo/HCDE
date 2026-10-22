# Chunk 832 — `DEarthquake` damage/falloff/intensity and `P_StartQuake*` from `mapthinkers/a_quake.cpp` (174–463).

## Scope

`DEarthquake` damage/falloff/intensity and `P_StartQuake*` from `mapthinkers/a_quake.cpp` (174–463).

## Native ranges

- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::DoQuakeDamage** 174–208 (35 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::GetModWave** 220–224 (5 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::GetModIntensity** 234–296 (63 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::GetFalloff** 306–322 (17 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **P_StartQuakeXYZ** 425–458 (34 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **P_StartQuake** 460–463 (4 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmAQuake.cs`
Managed LOC (tally): 187.
Conversion commit: `1b2b15007c7d134963fa5ee29c5f08b781b4db94`.

## Padding review (2026-10-21)

| Managed | Native (`a_quake.cpp`) |
|---------|------------------------|
| `DoQuakeDamage` | **174–208** |
| `GetModWave` | **220–224** |
| `GetModIntensity` | **234–296** |
| `GetFalloff` | **306–322** |
| `P_StartQuakeXYZ` | **425–458** |
| `P_StartQuake` | **460–463** |

**Deferred:** `Construct`/`Serialize`/`Tick` **47–164**, `StaticGetQuakeIntensities` **335–417** (orphan avoidance deferred to follow-up row).

**Post-review ratio:** 187/158 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmAQuakeTests.DoQuakeDamage_InRadius_AppliesDamage`
- `VmAQuakeTests.GetModIntensity_DoublesBaseWhenNoScaleFlags`
- `VmAQuakeTests.GetModIntensity_ScaleDown_UsesCountdownRatio`
- `VmAQuakeTests.GetFalloff_InsideMinRange_ReturnsOne`
- `VmAQuakeTests.StartQuake_TidZero_SpawnsAtActivator`

## managedLoc correction (2026-10-22)

While landing **833**, lifecycle code briefly lived in `VmAQuake.cs` before the `VmAQuakeLifecycle.cs` split. A failed tally pass (`fix_chunk_tally` at commit `23f80da7`) counted **389** physical lines in `VmAQuake.cs` (832 + 833 combined) and wrote that into a transient matrix snapshot. **Authoritative row:** `managedSourceFiles` = `VmAQuake.cs` only; at **833** land HEAD the file is **187** lines → `managedLoc` **187**, ratio **187/158 ≈ 118%**. Padding map above is unchanged; reaudit **832** at **187** managed LOC: 0 hard / 0 soft.

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 832 --to 832` (0 hard / 0 soft).

## Seams

Actor distance, damage/thrust, thinker spawn, and RNG are injected; `StaticGetQuakeIntensities` and tick left for a later row.

## Credit rationale

Credits earthquake damage falloff and level start helpers without splitting `GetModIntensity`.
