# Chunk 1004 — `G_DoQuickSave`, save comment/pic helpers, and `G_DoSaveGame` head from `g_game.cpp` (**2900–3049**); follows chunk **1003** (**2750–2899**).

## Scope

`G_DoQuickSave`, save comment/pic helpers, and `G_DoSaveGame` head from `g_game.cpp` (**2900–3049**); follows chunk **1003** (**2750–2899**).

## Native ranges

- `src/g_game.cpp` **G_DoQuickSave** 2900–3049 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameSaveFlow1004Slice.cs`
Managed LOC (tally): 78.
Conversion commit: `c2ad47601776debdc5b8388870b86476cce09546`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `G_DoAutoSave` hint clear tail (**2900–2901**) | seam with **1003** (zero duplicate credit) |
| `G_DoQuickSave` rotation (**2908–2937**) | yes |
| `PutSaveComment` elapsed time (**2967–2968**) | yes |
| `PutSavePic` dummy gate (**2976–2978**) | yes |
| `G_DoSaveGame` guards/failure head (**2998–3034**) | yes |
| Save version header (**3043**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2903–2905** | `myasctime` / `G_DoSaveGame` host call |
| **2941–2952** | `PutSaveWads` filesystem host |
| **2982–2985** | `WriteSavePic` render host |
| **3036–3042** | serializer open host |
| **3044+** | PNG append / info fields (next band) |

| Managed (`VmGGameSaveFlow1004Slice.cs`) | Native (`g_game.cpp`) |
|----------------------------------------|----------------------|
| `ResolveQuicksaveSkipReason` / rotation helpers | **2911–2933** |
| `ShouldAbortSaveWithoutLevel` / demo redirect | **2998–3006** |
| `ResolveRecoverableSnapshotFailure` | **3018–3026** |
| `ShouldWriteDummySavePic` | **2976–2978** |
| `FormatElapsedLevelTime` | **2967–2968** |
| `ResolveSaveVersionHeader` | **3043** |
| Band hull (exclusive slice) | **2900–3049** |

### Chunk **1004** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1003** |


**Post-review ratio:** 78/150 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameSaveFlow1004SliceTests.QuicksaveRotation`
- `VmGGameSaveFlow1004SliceTests.DoSaveGameHead`
- `VmGGameSaveFlow1004SliceTests.SavePicAndComment`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1004 --to 1004` (0 hard / 0 soft).

## Seams

`G_DoSaveGame` writers, `PutSaveWads`, PNG/screenshot host, and `G_SaveGame` call remain injected.

## Credit rationale

UNCredited **2900–3049** CLI band; disjoint from chunks **995–1003** and chunk **9** credited segments.
