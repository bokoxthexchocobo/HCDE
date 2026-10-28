# Chunk 935 — `LineSpecials[]` opcodes **58–207** (`p_lnspec.cpp` **3605–3754**). Follows **934** (through opcode **57** at **≤3604**); table continues at **3755+** deferred.

## Scope

`LineSpecials[]` opcodes **58–207** (`p_lnspec.cpp` **3605–3754**). Follows **934** (through opcode **57** at **≤3604**); table continues at **3755+** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LineSpecials** 3605–3754 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecLineSpecialDispatch935Slice.cs`
Managed LOC (tally): 193.
Conversion commit: `12a4eab6d9b8cea0edc9a03c46a33086496ac1be`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `DispatchOpcodes58Through207` | `LineSpecials[]` **3605–3754** |
| `ContinuedDispatchName` | opcode→name lookup **3605–3754** |
| `ContinuedDispatchIsNop` | NOP slots **3605**, **3647–3650**, **3655**, **3665**, **3668–3671**, etc. |
| `ContinuedDispatchIsPlat` | plat cluster **3607–3612**, **3719**, **3753–3754** |
| `ContinuedDispatchIsTeleport` | **3617–3625**, **3701** |
| `ContinuedDispatchIsAcs` | **3627–3632** |
| `ContinuedDispatchIsLight` | **3656–3664** |
| `ContinuedDispatchIsThingSpawn` | **3677–3684**, **3686** |
| polyobj movers | **3606**, **3633–3639** |
| floor/ceiling instant | **3613–3616** |
| ACS / FS / fog table slots | **3697–3699**, **3704–3706** |
| align/pan/scale slots | **3730–3736**, **3738–3739** |
| generic movers | **3747–3752** |
| `LS_SetPlayerProperty` table entry only | **3738** (handler body credited elsewhere) |

### Chunk **935** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**934** |


**Post-review ratio:** 193/150 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchEntryCount_Is150`
- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchName_MapsTeleportOpcode`
- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchIsAcs_RecognizesExecute`
- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchIsNop_ScrollTextureSlot`
- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchName_TableSlotForSetPlayerProperty`
- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchName_EndsAtPerpetualRaiseLip`
- `VmPLnspecLineSpecialDispatch935SliceTests.ContinuedDispatchIsLight_IncludesStrobe`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 935 --to 935` (0 hard / 0 soft).

## Seams

`lnSpecFunc` binding and per-special handlers remain host seams; dispatch table credits names only (not handler bodies).

## Credit rationale

Honest **150**-line UNCredited band toward **3982**; exclusive `VmPLnspecLineSpecialDispatch935Slice.cs`.
