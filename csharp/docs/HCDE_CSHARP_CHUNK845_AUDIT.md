# Chunk 845 — `FParser::SF_ObjAwaken` … `SF_ObjDead` from `fragglescript/t_func.cpp` (2778–3047).

## Scope

`FParser::SF_ObjAwaken` … `SF_ObjDead` from `fragglescript/t_func.cpp` (2778–3047).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjAwaken** 2778–2795 (18 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_AmbientSound** 2805–2811 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ExitSecret** 2822–2825 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjValue** 2834–2841 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_StringValue** 2843–2857 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_IntValue** 2859–2866 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_FixedValue** 2868–2875 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetObjPosition** 2944–2959 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjDead** 3029–3047 (19 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF act separators** 2796–2804 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF act separators** 2812–2821 (10 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF act separators** 2826–2833 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF act separators** 2960–3028 (69 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmFScriptParserSfAct.cs`
Managed LOC (tally): 120.
Conversion commit: `d0da127a752c9dd05bcf35d557c9edacb0ccf9b5`.

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

**Post-review ratio:** 120/199 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfActTests.SfObjAwaken_ActivatesTrigger`
- `VmFScriptParserSfActTests.SfAmbientSound_QueuesGlobal`
- `VmFScriptParserSfActTests.SfExitSecret_MarksSecretExit`
- `VmFScriptParserSfActTests.SfMobjValue_ReturnsMobj`
- `VmFScriptParserSfActTests.SfStringValue_CoercesInt`
- `VmFScriptParserSfActTests.SfIntValue_FromInt`
- `VmFScriptParserSfActTests.SfFixedValue_FromInt`
- `VmFScriptParserSfActTests.SfSetObjPosition_MovesActor`
- `VmFScriptParserSfActTests.SfObjDead_DeadActor`
- `VmFScriptParserSfActTests.SfObjDead_CorpseFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 845 --to 845` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. Uses `Actor.Activate`, `ActorSoundActions.StartGlobalSound`, `AuthoritySimulation.MarkExited`, `Actor` placement/death, and `FsFixValue` from chunk 843.

## Credit rationale

Portable mobj/value band; **2884–3244** spawn/combat iterators and **3245+** stub/query tail land in **846**; **1381–2241** remains deferred.
