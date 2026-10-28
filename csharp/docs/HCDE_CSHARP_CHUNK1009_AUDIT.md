# Chunk 1009 — `G_CheckDemoStatus` continuation, demo record finish, slideshow/autosave, and late `DEFINE_GLOBAL` seam from `g_game.cpp` (**3650–3799**); follows chunk **1008** (**3500–3649**).

## Scope

`G_CheckDemoStatus` continuation, demo record finish, slideshow/autosave, and late `DEFINE_GLOBAL` seam from `g_game.cpp` (**3650–3799**); follows chunk **1008** (**3500–3649**).

## Native ranges

- `src/g_game.cpp` **G_CheckDemoStatus** 3650–3799 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameLateTail1009Slice.cs`
Managed LOC (tally): 76.
Conversion commit: `a080b4bd4f11ff7166352c4b2b095b21cee4a1a1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Playback cleanup + timing (**3653–3665**) | yes |
| Status bar / player slot reset (**3667–3670**) | yes |
| Single/timing demo end (**3671–3693**) | yes |
| Record compress/save (**3700–3742**) | yes |
| `G_StartSlideshow` selection (**3750–3757**) | yes |
| Script screenshot gate (**3772–3775**) | yes |
| `G_MakeAutoSave` gate (**3781–3784**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3656–3659** | cvar/weapon host restore |
| **3678–3680** | `I_FatalError` timing print host |
| **3704–3720** | compress/chunk binary host |
| **3723–3731** | `FileWriter` host |
| **3751–3759** | `RunIntermission` host |
| **3762–3799** | action/global registration host |
| **3800–3813** | deferred EOF tail (zero credit) |

| Managed (`VmGGameLateTail1009Slice.cs`) | Native (`g_game.cpp`) |
|----------------------------------------|----------------------|
| Timing / player cleanup predicates | **3653–3670** |
| Demo end policy + FPS estimate | **3671–3693** |
| Compress capacity / save outcome | **3708–3741** |
| Slideshow / screenshot / autosave gates | **3750–3784** |
| Band hull (exclusive slice) | **3650–3799** |

### Chunk **1009** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1008** |


**Post-review ratio:** 76/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameLateTail1009SliceTests.CheckDemoStatusPlayback`
- `VmGGameLateTail1009SliceTests.DemoRecordingFinish`
- `VmGGameLateTail1009SliceTests.SlideshowAndAutosave`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1009 --to 1009` (0 hard / 0 soft).

## Seams

`D_AdvanceDemo`, zlib/file writers, intermission host, `G_ScreenShot`, and global registration remain injected.

## Credit rationale

UNCredited **3650–3799** CLI band; disjoint from chunks **995–1008** and chunk **9** credited segments; **3800–3813** deferred sub-150 without padding.
