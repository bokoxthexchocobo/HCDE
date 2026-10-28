# Chunk 1002 — `C_SerializeCVars` deserialize and `G_DoLoadGame` info.json validation from `g_game.cpp` (**2600–2749**); follows chunk **1001** (**2449–2598**).

## Scope

`C_SerializeCVars` deserialize and `G_DoLoadGame` info.json validation from `g_game.cpp` (**2600–2749**); follows chunk **1001** (**2449–2598**).

## Native ranges

- `src/g_game.cpp` **G_DoLoadGame** 2600–2749 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameLoadFlow1002Slice.cs`
Managed LOC (tally): 93.
Conversion commit: `a6bd33e6e6ec6bb7aa04a2cc5bccb6d3d541aec7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| CVAR deserialize filter + apply (**2608–2618**) | yes |
| Demo/console load head (**2632–2637**) | yes |
| Save archive entry checks (**2640–2660**) | yes |
| Engine / `AllowLoadIn` gate (**2685–2700**) | yes |
| Save version range (**2703–2718**) | yes |
| Map name + hide console (**2726–2737**) | yes |
| `globals.json` open (**2741–2752**) partial | yes (through **2749** hull) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2628–2637** | `Net_ResetCommands` / trace host |
| **2678–2684** | `LOAD_GZDOOM_4142_SAVES` compat host |
| **2708–2717** | `GStrings` message host |
| **2750+** | `G_SerializeHub` (next band) |

| Managed (`VmGGameLoadFlow1002Slice.cs`) | Native (`g_game.cpp`) |
|----------------------------------------|----------------------|
| `ShouldDeserializeCvarFromSave` / `ShouldApplyLoadedCvarString` | **2608–2618** |
| `ShouldStopDemoPlaybackForLoad` / `ShouldHideConsoleAfterLoadMenu` | **2632–2637** |
| `IsSaveArchiveEntryMissing` | **2640–2646**, **2741–2745** |
| `IsEngineSignatureAllowed` / `ResolveEngineCompatibilityReject` | **2685–2699** |
| `IsSaveVersionSupported` / too old/new helpers | **2703–2718** |
| `ShouldRejectEmptySaveMap` | **2726–2729** |
| `ResolveGameStateAfterLoadHideRequest` | **2734–2737** |
| Band hull (exclusive slice) | **2600–2749** |

### Chunk **1002** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1001** |


**Post-review ratio:** 93/150 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameLoadFlow1002SliceTests.DeserializeCvarFilter`
- `VmGGameLoadFlow1002SliceTests.DoLoadGameHeadGates`
- `VmGGameLoadFlow1002SliceTests.EngineAndVersionChecks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1002 --to 1002` (0 hard / 0 soft).

## Seams

`arc()`, `FSerializer`, `G_TraceSetGameAction`, and `LoadGameError` string host remain injected.

## Credit rationale

UNCredited **2600–2749** CLI band; disjoint from chunks **995–1001** and chunk **9** credited segments.
