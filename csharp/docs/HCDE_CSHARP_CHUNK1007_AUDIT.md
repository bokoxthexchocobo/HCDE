# Chunk 1007 — `G_DeferedPlayDemo`, `playdemo`/`timedemo` CCMD gates, and `G_ProcessIFFDemo` header loop from `g_game.cpp` (**3350–3499**); follows chunk **1006** (**3200–3349**).

## Scope

`G_DeferedPlayDemo`, `playdemo`/`timedemo` CCMD gates, and `G_ProcessIFFDemo` header loop from `g_game.cpp` (**3350–3499**); follows chunk **1006** (**3200–3349**).

## Native ranges

- `src/g_game.cpp` **G_ProcessIFFDemo** 3350–3499 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameDemoPlayback1007Slice.cs`
Managed LOC (tally): 82.
Conversion commit: `e16e064d298410f8f944099d8eb712c74167915d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `G_DeferedPlayDemo` gameaction (**3350–3351**) | yes |
| `playdemo` reject/defer (**3356–3370**) | yes |
| `timedemo` argc gate (**3375–3379**) | yes |
| FORM length / ZDEM magic (**3399–3410**) | yes |
| Chunk bounds + ZDHD version/map (**3419–3456**) | yes |
| `UINF`/`NETD`/`BODY`/`COMP` cases (**3463–3488**) | yes |
| Missing header gate (**3495–3498**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3358–3364** | console print host |
| **3460–3461** | `C_ReadCVars` host |
| **3470** | `D_ReadUserInfoStrings` host |
| **3478** | `P_ReadDemoWeaponsChunk` host |
| **3500+** | no-players / no-body checks (next band) |

| Managed (`VmGGameDemoPlayback1007Slice.cs`) | Native (`g_game.cpp`) |
|---------------------------------------------|----------------------|
| `ResolveDeferredPlayDemoGameAction` | **3351** |
| `ResolvePlayDemoRejectReason` / argv gate | **3356–3370**, **3375–3379** |
| FORM/ZDEM/chunk layout helpers | **3399–3423** |
| ZDHD version/map/RNG gates | **3430–3456** |
| Player/multiplayer/body chunk predicates | **3463–3488** |
| `ResolveMissingDemoHeaderFailure` | **3495–3498** |
| Band hull (exclusive slice) | **3350–3499** |

### Chunk **1007** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1006** |


**Post-review ratio:** 82/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameDemoPlayback1007SliceTests.DeferredPlayDemoAction`
- `VmGGameDemoPlayback1007SliceTests.ProcessIffDemoHead`
- `VmGGameDemoPlayback1007SliceTests.DemoHeaderChunks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1007 --to 1007` (0 hard / 0 soft).

## Seams

Demo file I/O, `Printf`, `G_TimeDemo`, chunk readers, and userinfo hosts remain injected.

## Credit rationale

UNCredited **3350–3499** CLI band; disjoint from chunks **995–1006** and chunk **9** credited segments.
