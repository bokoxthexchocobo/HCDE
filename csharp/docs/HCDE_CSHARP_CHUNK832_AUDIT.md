# Chunk 832 — `DEarthquake` damage/falloff/intensity and `P_StartQuake*` from `mapthinkers/a_quake.cpp` (174–463). Retired (2026-10-23): out-of-body credit removed; honest **124** < **150**.

## Scope

`DEarthquake` damage/falloff/intensity and `P_StartQuake*` from `mapthinkers/a_quake.cpp` (174–463). Retired (2026-10-23): out-of-body credit removed; honest **124** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5e69c3005a45d343185dce4ed8d24b98a47db8b1`.

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

### Out-of-body uncredit (832)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **158** |
| **Out-of-body removed** | **34** |
| **Honest** | **124** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 832 --to 832` (0 hard / 0 soft).

## Seams

Actor distance, damage/thrust, thinker spawn, and RNG are injected; `StaticGetQuakeIntensities` and tick left for a later row.

## Credit rationale

Credits earthquake damage falloff and level start helpers without splitting `GetModIntensity`.
