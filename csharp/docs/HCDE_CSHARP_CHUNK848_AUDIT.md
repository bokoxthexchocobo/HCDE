# Chunk 848 — `FParser::SF_SpawnMissile` … `SF_PlayerAddFrag` from `fragglescript/t_func.cpp` (3048–3237). Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Scope

`FParser::SF_SpawnMissile` … `SF_PlayerAddFrag` from `fragglescript/t_func.cpp` (3048–3237). Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8bbedef76ace0729b380d5ecabfea273d94fd130`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SpawnMissile` | yes |
| `SF_MapThingNumExist` | yes |
| `SF_MapThings` | yes |
| `SF_ObjState` | yes |
| `SF_LineFlag` | yes |
| `SF_PlayerAddFrag` | yes |

| Managed | Native (`t_func.cpp` 3048–3237) |
|---------|-------------------------|
| `SfSpawnMissile` | **3055–3069** |
| `SfMapThingNumExist` | **3077–3098** |
| `SfMapThings` | **3106–3112** |
| `SfObjState` | **3121–3156** |
| `SfLineFlag` | **3166–3200** |
| `SfPlayerAddFrag` | **3209–3237** |
| (separators) | **3048–3054**, **3070–3076**, **3099–3105**, **3113–3120**, **3157–3165**, **3201–3208** |

### Out-of-body uncredit (848)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **190** |
| **Out-of-body removed** | **46** |
| **Honest** | **144** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 848 --to 848` (0 hard / 0 soft).

## Seams

Uses `AuthoritySimulation.SpawnProjectile`, `FraggleScriptSpawnedThings`, `LevelLine.Flags`, and `PlayerPawn` frag fields. **3259–3316** combat deferred to **851**.

## Credit rationale

Legacy map/missile band between **845** act tail and **846** stub/trig; **1381–2241** and **3454–3472** remain deferred.
