# Chunk 796 — `FLevelLocals::CreateCeiling` (`mapthinkers/a_ceiling.cpp` 250–479). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

`FLevelLocals::CreateCeiling` (`mapthinkers/a_ceiling.cpp` 250–479). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `331c4d7cd2848c25ff68efebe530ca1cdb3566e7`.

## Padding review (2026-10-21)

| Managed | Native (`a_ceiling.cpp`) |
|---------|-------------------------|
| `CreateCeiling` | `FLevelLocals::CreateCeiling` **250–479** |

### Out-of-body uncredit (796)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **230** |
| **Out-of-body removed** | **230** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 796 --to 796` (0 hard / 0 soft).

## Seams

Surrounding-height finders, model sectors, `CreateThinker`, `StopInterpolation`, and `PlayCeilingSound` are injected on `CreateCeilingContext`.

## Credit rationale

Ceiling mover setup before tick/EV chunk797.
