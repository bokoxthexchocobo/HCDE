# Chunk 845 — `FParser::SF_ObjAwaken` … `SF_ObjDead` from `fragglescript/t_func.cpp` (2778–3047). Retired (2026-10-23): out-of-body credit removed; honest **122** < **150**.

## Scope

`FParser::SF_ObjAwaken` … `SF_ObjDead` from `fragglescript/t_func.cpp` (2778–3047). Retired (2026-10-23): out-of-body credit removed; honest **122** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `64dcca94eddb30a748baaa302df36754e2c32fef`.

## Padding review (2026-10-21)

Shared-file stubs credited in chunk 846 (padding allowlist): `SfSkinColor`, `SfPlayDemo`, `SfCheckCVar`, `SfObjType`.

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_ObjAwaken` | yes |
| `SF_AmbientSound` | yes |
| `SF_ExitSecret` | yes |
| `SF_MobjValue` | yes |
| `SF_StringValue` | yes |
| `SF_IntValue` | yes |
| `SF_FixedValue` | yes |
| `SF_SetObjPosition` | yes |
| `SF_ObjDead` | yes |

| Managed | Native (`t_func.cpp` 2778–3047) |
|---------|-------------------------|
| `SfObjAwaken` | **2778–2795** |
| `SfAmbientSound` | **2805–2811** |
| `SfExitSecret` | **2822–2825** |
| `SfMobjValue` | **2834–2841** |
| `SfStringValue` | **2843–2857** |
| `SfIntValue` | **2859–2866** |
| `SfFixedValue` | **2868–2875** |
| `SfSetObjPosition` | **2944–2959** |
| `SfObjDead` | **3029–3047** |
| (separators / deferred native) | **2796–2804**, **2812–2821**, **2826–2833**, **2960–3028**; **2876–2943** uncredited until combat band |

### Out-of-body uncredit (845)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **77** |
| **Honest** | **122** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 845 --to 845` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. Uses `Actor.Activate`, `ActorSoundActions.StartGlobalSound`, `AuthoritySimulation.MarkExited`, `Actor` placement/death, and `FsFixValue` from chunk 843.

## Credit rationale

Portable mobj/value band; **2884–3244** spawn/combat iterators and **3245+** stub/query tail land in **846**; **1381–2241** remains deferred.
