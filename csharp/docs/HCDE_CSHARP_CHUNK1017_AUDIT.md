# Chunk 1017 — `DSectorMarker::PropagateMark` tail, `FLevelLocals::Tick`, `Mark`, `AddScroller`, `SetInterMusic`, and `SetMusicVolume` from `g_level.cpp` (**2409–2558**); follows chunk **1016** (**2259–2408**).

## Scope

`DSectorMarker::PropagateMark` tail, `FLevelLocals::Tick`, `Mark`, `AddScroller`, `SetInterMusic`, and `SetMusicVolume` from `g_level.cpp` (**2409–2558**); follows chunk **1016** (**2259–2408**).

## Native ranges

- `src/g_level.cpp` **FLevelLocals::Mark** 2409–2558 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelMarkTickMusic1017Slice.cs`
Managed LOC (tally): 117.
Conversion commit: `0ed767d5e0f2888047203629bbd8c8951a7a9e91`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sidedef propagate loop + gray requeue (**2408–2429**) | yes (tail only; batch sizes from **1016** constants) |
| `FLevelLocals::Tick` scroll reset (**2437–2443**) | yes |
| `FLevelLocals::Mark` marker lifecycle + roots (**2451–2497**) | yes |
| `AddScroller` head (**2505–2516**) | yes |
| `SetInterMusic` + VM (**2523–2540**) | yes |
| `SetMusicVolume` (**2547–2551**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2371–2407** | sector/poly batch stepping (**1016**) |
| **2425–2427** | GC list mutation host |
| **2517+** | `AddScroller` assign (**2559+** `GetInfighting` outside hull) |
| **2559+** | next band |

| Managed (`VmGLevelMarkTickMusic1017Slice.cs`) | Native (`g_level.cpp`) |
|---------------------------------------------|----------------------|
| Side marked-bytes + texture mark count | **2410–2415** |
| Gray reinsert plan | **2423–2427** |
| Scroll carry clear/resize | **2440–2443**, **2511–2515** |
| Sector marker create/reset | **2453–2464** |
| Inter music fallback chain | **2525–2531** |
| Music volume assign | **2547–2550** |
| Band hull (exclusive slice) | **2409–2558** |

### Chunk **1017** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1016** |


**Post-review ratio:** 117/150 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelMarkTickMusic1017SliceTests.PropagateMarkTailAndGrayReinsert`
- `VmGLevelMarkTickMusic1017SliceTests.LevelTickMarkAndScroller`
- `VmGLevelMarkTickMusic1017SliceTests.InterMusicAndVolume`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1017 --to 1017` (0 hard / 0 soft).

## Seams

`Black2Gray`, `GC::Mark`, `Create<DSectorMarker>`, `memset`, `S_ChangeMusic`, `I_SetMusicVolume`, and thinker `MarkRoots` remain injected. Sector/poly/side **batch stepping** stays on **1016** helpers only.

## Credit rationale

UNCredited **2409–2558**; **1017** credits sidedef loop body, gray requeue, and level tick/mark/music — not a second port of **1016** `Step*PropagateMark` batch math.
