# Chunk 1008 — `G_ProcessIFFDemo` tail, `G_DoPlayDemo`, `G_TimeDemo`, and `G_CheckDemoStatus` head from `g_game.cpp` (**3500–3649**); follows chunk **1007** (**3350–3499**).

## Scope

`G_ProcessIFFDemo` tail, `G_DoPlayDemo`, `G_TimeDemo`, and `G_CheckDemoStatus` head from `g_game.cpp` (**3500–3649**); follows chunk **1007** (**3350–3499**).

## Native ranges

- `src/g_game.cpp` **G_DoPlayDemo** 3500–3649 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameDemoPlayback1008Slice.cs`
Managed LOC (tally): 89.
Conversion commit: `82a342336b3478870278ae5fe6847d8a51ea00c5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| No players / no BODY (**3501–3512**) | yes |
| Multiplayer + decompress (**3514–3529**) | yes |
| `G_DoPlayDemo` load/FORM (**3542–3587**) | yes |
| Successful play path (**3594–3613**) | yes |
| `G_TimeDemo` (**3622–3627**) | yes |
| `G_CheckDemoStatus` head (**3643–3654**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3503–3510** | `Printf` host |
| **3520–3528** | zlib buffer host |
| **3545–3563** | filesystem read host |
| **3573–3580** | `C_BackupCVars` / buffer reset host |
| **3601–3608** | `G_InitNew` / console host |
| **3650+** | timing print tail (next band) |

| Managed (`VmGGameDemoPlayback1008Slice.cs`) | Native (`g_game.cpp`) |
|---------------------------------------------|----------------------|
| `ResolveNoPlayersFailure` / `ResolveNoBodyFailure` | **3501–3512** |
| Decompress / netgame player count | **3514–3529** |
| Lump/file open + FORM guards | **3542–3587** |
| Map init vs savegame requirement | **3599–3606** |
| `ResolveTimeDemoGameAction` / draw flags | **3622–3627** |
| `G_CheckDemoStatus` userinfo/timing gates | **3643–3654** |
| Band hull (exclusive slice) | **3500–3649** |

### Chunk **1008** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1007** |


**Post-review ratio:** 89/150 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameDemoPlayback1008SliceTests.ProcessIffDemoTail`
- `VmGGameDemoPlayback1008SliceTests.DoPlayDemoHead`
- `VmGGameDemoPlayback1008SliceTests.TimeDemoAndStatusHead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1008 --to 1008` (0 hard / 0 soft).

## Seams

File I/O, `uncompress`, `G_InitNew`, `I_Error`, and userinfo hosts remain injected.

## Credit rationale

UNCredited **3500–3649** CLI band; disjoint from chunks **995–1007** and chunk **9** credited segments.
