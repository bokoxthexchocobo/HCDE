# Chunk 847 — `FParser::SF_LevelNum` … `SF_SetColor` from `fragglescript/t_func.cpp` (3541–3698). Retired (2026-10-23): out-of-body credit removed; honest **127** < **150**.

## Scope

`FParser::SF_LevelNum` … `SF_SetColor` from `fragglescript/t_func.cpp` (3541–3698). Retired (2026-10-23): out-of-body credit removed; honest **127** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `86570d8157f3749683cd1fb7cb8f160505ee1f64`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_LevelNum` | yes |
| `SF_MobjRadius` | yes |
| `SF_MobjHeight` | yes |
| `SF_ThingCount` | yes |
| `SF_SetColor` | yes |

| Managed | Native (`t_func.cpp` 3541–3698) |
|---------|-------------------------|
| `SfLevelNum` | **3541–3545** |
| `SfMobjRadius` | **3554–3568** |
| `SfMobjHeight` | **3577–3591** |
| `SfThingCount` | **3600–3652** |
| `SfSetColor` | **3660–3698** |
| (separators) | **3546–3553**, **3569–3576**, **3592–3599**, **3653–3659** |

### Out-of-body uncredit (847)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **158** |
| **Out-of-body removed** | **31** |
| **Honest** | **127** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 847 --to 847` (0 hard / 0 soft).

## Seams

Uses `AuthoritySimulation`, `DoomActorCatalog`, `AcsThingCountReplacementPolicy`, and `LevelSector.ColormapLightColor` for script sector tint.

## Credit rationale

Portable GZDoom query band after **846** trig; **1381–2241** remains deferred.
