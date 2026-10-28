# Chunk 905 — `FParser::SF_PlayerName` … `SF_KillObj` from `fragglescript/t_func.cpp` (**722–918**). Follows **904** (**≤721**); `SF_ObjX` deferred at **927**.

## Scope

`FParser::SF_PlayerName` … `SF_KillObj` from `fragglescript/t_func.cpp` (**722–918**). Follows **904** (**≤721**); `SF_ObjX` deferred at **927**.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerName** 722–918 (197 lines)

**Credited native LOC:** 197 (tally `nativeLoc` 197).

## Managed

`VmFScriptParserSfPlayer.cs`, `VmFScriptParserSfRemoveKill905Slice.cs`
Managed LOC (tally): 138.
Conversion commit: `bb6fd4dfea6474d7c4a6d9a13b41f8e5b306caf3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_PlayerName` | yes |
| `SF_PlayerObj` | yes |
| `SF_Player` | yes |
| `SF_Spawn` | yes |
| `SF_RemoveObj` | yes |
| `SF_KillObj` | yes |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfPlayerName` | **722–745** |
| `SfPlayerObj` | **753–776** |
| `SfPlayer` | **784–807** |
| `SfSpawn` | **817–869** |
| `SfRemoveObj` | **877–888** |
| `SfKillObj` | **896–918** |
| (separator) | **746–752** |
| (separator) | **777–783** |
| (separator) | **808–816** |
| (separator) | **870–876** |
| (separator) | **889–895** |

### Chunk **905** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **197** |
| **Net headline** | **+197** vs post-**904** |


**Post-review ratio:** 138/197 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfPlayerTests.SfPlayerName_NoTrigger_ScriptError`
- `VmFScriptParserSfPlayerTests.SfPlayerName_ValidPlayer_ReturnsName`
- `VmFScriptParserSfPlayerTests.SfPlayer_FromMobj_ReturnsPlayerNum`
- `VmFScriptParserSfPlayerTests.SfSpawn_FailedLocation_ClearsReturn`
- `VmFScriptParserSfPlayerTests.SfRemoveObj_CallsDestroy`
- `VmFScriptParserSfPlayerTests.SfKillObj_DefaultTrigger_MassacreDamage`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 905 --to 905` (0 hard / 0 soft).

## Seams

Player resolve, spawn, placement test, counter clear, massacre damage, and `Destroy` are host callbacks on `ParserContext`; `Spawn`/`P_TestMobjLocation`/`P_DamageMobj` are not hosted.

## Credit rationale

Honest **197**-line UNCredited band after **904**; `SfRemoveObj`/`SfKillObj` live in exclusive `VmFScriptParserSfRemoveKill905Slice.cs` so `VmFScriptParserSfObj.cs` remains free for **927+**.
