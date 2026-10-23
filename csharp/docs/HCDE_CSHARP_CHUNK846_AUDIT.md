# Chunk 846 — `FParser::SF_SkinColor` … `SF_SetCorona` from `fragglescript/t_func.cpp` (3245–3341, 3343–3533).

## Scope

`FParser::SF_SkinColor` … `SF_SetCorona` from `fragglescript/t_func.cpp` (3245–3341, 3343–3533).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SkinColor** 3245–3248 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayDemo** 3250–3253 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_CheckCVar** 3255–3258 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjType** 3317–3341 (25 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF stub separators** 3249–3249 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF stub separators** 3254–3254 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Sin** 3349–3355 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ASin** 3358–3364 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Cos** 3367–3373 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ACos** 3376–3382 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Tan** 3385–3391 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ATan** 3394–3400 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Exp** 3403–3409 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Log** 3412–3418 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Sqrt** 3421–3427 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Floor** 3429–3436 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Pow** 3439–3445 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_NewHUPic** 3454–3457 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_DeleteHUPic** 3459–3462 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ModifyHUPic** 3464–3467 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetHUPicDisplay** 3469–3472 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetCorona** 3481–3533 (53 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3343–3348 (6 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3356–3357 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3365–3366 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3374–3375 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3383–3384 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3392–3393 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3401–3402 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3410–3411 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3419–3420 (2 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3428–3428 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3446–3453 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3458–3458 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3463–3463 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3468–3468 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF trig separators** 3473–3480 (8 lines)

**Credited native LOC:** 228 (tally `nativeLoc` 228).

## Managed

`VmFScriptParserSfTrig.cs`
Managed LOC (tally): 118.
Conversion commit: `12a5b4b3d944a55f2c03785acd03a7a38764bd2b`.

## Padding review (2026-10-21)

HUD stub padding allowlist: `SfNewHuPic`, `SfDeleteHuPic`, `SfModifyHuPic`, `SfSetHuPicDisplay`.

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SkinColor` | yes (no-op) |
| `SF_PlayDemo` | yes (no-op) |
| `SF_CheckCVar` | yes (returns 0) |
| `SF_ObjType` | yes |
| `SF_Sin` | yes |
| `SF_ASin` | yes |
| `SF_Cos` | yes |
| `SF_ACos` | yes |
| `SF_Tan` | yes |
| `SF_ATan` | yes |
| `SF_Exp` | yes |
| `SF_Log` | yes |
| `SF_Sqrt` | yes |
| `SF_Floor` | yes |
| `SF_Pow` | yes |
| `SF_NewHUPic` | yes (no-op) |
| `SF_DeleteHUPic` | yes (no-op) |
| `SF_ModifyHUPic` | yes (no-op) |
| `SF_SetHUPicDisplay` | yes (no-op) |
| `SF_SetCorona` | yes (returns 0) |

| Managed | Native (`t_func.cpp` 3245–3533) |
|---------|-------------------------|
| `SfSkinColor` | **3245–3248** |
| `SfPlayDemo` | **3250–3253** |
| `SfCheckCVar` | **3255–3258** |
| `SfObjType` | **3317–3341** |
| `SfSin` | **3349–3355** |
| `SfAsin` | **3358–3364** |
| `SfCos` | **3367–3373** |
| `SfAcos` | **3376–3382** |
| `SfTan` | **3385–3391** |
| `SfAtan` | **3394–3400** |
| `SfExp` | **3403–3409** |
| `SfLog` | **3412–3418** |
| `SfSqrt` | **3421–3427** |
| `SfFloor` | **3429–3436** |
| `SfPow` | **3439–3445** |
| `SfNewHuPic` | **3454–3457** |
| `SfDeleteHuPic` | **3459–3462** |
| `SfModifyHuPic` | **3464–3467** |
| `SfSetHuPicDisplay` | **3469–3472** |
| `SfSetCorona` | **3481–3533** |
| (separators / deferred) | **3249**, **3254**, **3342–3348**, **3356–3357**, **3365–3366**, **3374–3375**, **3383–3384**, **3392–3393**, **3401–3402**, **3410–3411**, **3419–3420**, **3428**, **3446–3453**, **3458**, **3463**, **3468**, **3473–3480**; **3259–3316** uncredited combat |

**Post-review ratio:** 118/228 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfActTests.SfCheckCVar_ReturnsZero`
- `VmFScriptParserSfActTests.SfObjType_MatchesActorTypes`
- `VmFScriptParserSfTrigTests.SfSin_ReturnsSin`
- `VmFScriptParserSfTrigTests.SfPow_Squares`
- `VmFScriptParserSfTrigTests.SfFloor_TruncatesFixed`
- `VmFScriptParserSfTrigTests.SfSetCorona_ThreeArgsReturnsZero`
- `VmFScriptParserSfTrigTests.SfSetCorona_WrongArgCountErrors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 846 --to 846` (0 hard / 0 soft).

## Seams

Uses existing `FsFixValue`, `SetReturnDouble`, and `CheckArgs`; HUD corona bodies are faithful no-ops/stubs.

## Credit rationale

Portable trig/HUD band after **845**; **1381–2241** camera/sector band remains deferred.
