# Chunk 1006 — Demo net dispatch tail, `CCMD (stop)`, `G_WriteDemoTiccmd`, `G_RecordDemo`, and `G_BeginRecording` from `g_game.cpp` (**3200–3349**); follows chunk **1005** (**3050–3199**).

## Scope

Demo net dispatch tail, `CCMD (stop)`, `G_WriteDemoTiccmd`, `G_RecordDemo`, and `G_BeginRecording` from `g_game.cpp` (**3200–3349**); follows chunk **1005** (**3050–3199**).

## Native ranges

- `src/g_game.cpp` **G_WriteDemoTiccmd** 3200–3349 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameDemoRecord1006Slice.cs`
Managed LOC (tally): 84.
Conversion commit: `e520d2543e569368efd08e82d1081b5d6f380e9f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `G_ReadDemoTiccmd` default (**3201–3203**) | yes |
| `CCMD (stop)` / write stop gate (**3208–3225**) | yes |
| Special ticcmd + buffer growth (**3229–3248**) | yes |
| `G_RecordDemo` setup (**3257–3265**) | yes |
| `G_BeginRecording` header (**3273–3338**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3204–3206** | loop close host |
| **3230–3233** | `WriteUserCmdMessage` host |
| **3283–3296** | FORM chunk binary host |
| **3303–3307** | userinfo string host |
| **3321–3328** | cvar/weapon chunk host |
| **3342+** | `G_DeferedPlayDemo` (next band) |

| Managed (`VmGGameDemoRecord1006Slice.cs`) | Native (`g_game.cpp`) |
|------------------------------------------|----------------------|
| `RoutesUnhandledDemoCommandToNet` | **3201–3203** |
| `ResolveDemoRecordingWriteGate` / full console trace | **3218–3224** |
| `ShouldWriteSpecialDemoTics` / buffer expansion | **3229–3248** |
| `AppendDefaultDemoExtensionIfMissing` | **3260–3263** |
| `ResolveRecordingStartMap` / header chunk gates | **3277–3338** |
| Band hull (exclusive slice) | **3200–3349** |

### Chunk **1006** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1005** |


**Post-review ratio:** 84/150 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameDemoRecord1006SliceTests.ReadDemoDefaultDispatch`
- `VmGGameDemoRecord1006SliceTests.WriteDemoTiccmd`
- `VmGGameDemoRecord1006SliceTests.RecordAndBeginDemo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1006 --to 1006` (0 hard / 0 soft).

## Seams

`Net_DoCommand`, demo buffer host I/O, chunk writers, and `C_WriteCVars` remain injected.

## Credit rationale

UNCredited **3200–3349** CLI band; disjoint from chunks **995–1005** and chunk **9** credited segments.
