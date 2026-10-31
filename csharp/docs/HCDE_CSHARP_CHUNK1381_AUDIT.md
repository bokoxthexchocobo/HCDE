# Chunk 1381 — Team validity, team change, getters, the team-list command, and the VM natives from teaminfo.cpp (232-434).

## Scope

Team validity, team change, getters, the team-list command, and the VM natives from teaminfo.cpp (232-434).

## Native ranges

- `src/gamedata/teaminfo.cpp` **FTeam::IsValid** 232–434 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmTeamNative1381Slice.cs`
Managed LOC (tally): 158.
Conversion commit: `60ab2ac2b4214551803264beeac85db82c73e8b6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Band open (**232–237**) | yes |
| `FTeam::IsValid` (**239–245**) | yes |
| `FTeam::ChangeTeam` (**247–261**) | yes |
| `GetName` (**269–272**) | yes |
| `GetPlayerColor` (**280–283**) | yes |
| `GetTextColor` (**291–306**) | yes |
| `GetLogo` (**314–317**) | yes |
| `GetAllowCustomPlayerColor` (**325–328**) | yes |
| `teamlist` (**336–344**) | yes |
| Global and field descriptors (**347–348**) | yes |
| `IsValid` native (**350–361**) | yes |
| `ChangeTeam` native (**363–375**) | yes |
| `GetPlayerColor` native (**377–386**) | yes |
| `GetTextColor` native (**388–397**) | yes |
| `GetLogo` native (**399–412**) | yes |
| `GetLogoName` native (**414–423**) | yes |
| `AllowsCustomPlayerColor` native (**425–434**) | yes |
| Band hull (**232–434**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `ClearTeams` **228–231** | already credited |
| `userinfo_t::TeamChanged` | callee |
| `R_BuildPlayerTranslation` | callee |
| `V_ParseFontColor` | callee |
| `TexMan.CheckForTexture` | callee |

`IsValid` is true only when the unsigned id is below `Teams.Size()`. `ChangeTeam` returns false unless multiplayer and teamplay are set, the player is below `MAXPLAYERS` (64), that player is in game, the new team is valid, and the unsigned cast of the current team differs from the new team. A current team of -1 does not match team 1. Success records `TeamChanged` with the int conversion of the new team, stores that value, then records the translation build. `GetName`, `GetPlayerColor`, `GetLogo`, and `GetAllowCustomPlayerColor` return the fields. An empty text color returns `CR_UNTRANSLATED` (11) without parsing. Otherwise `V_ParseFontColor` is called with normal and bold color 0. `CR_UNDEFINED` (-1) prints `GetTextColor: Undefined color` and returns 11. An empty logo index is -1 and does not look up a texture. Any other logo calls `CheckForTexture` with `ETextureType::Any` (0) and returns that index. `teamlist` prints the header, `index : name` for each team, and the footer. The command name is `teamlist`. `DEFINE_GLOBAL(Teams)` publishes script name `Teams` with an empty class and bit value 0. `DEFINE_FIELD_NAMED` publishes class `FTeam`, script name `mName`, native `m_Name`, bit value 0. Each action evaluates its value even when `numret` is 0, then returns 0 without a result. A positive `numret` stores the int or string and returns 1. Bool results are 0 or 1. A null struct self aborts with `In function parameter self` before the body.

| Managed (`VmTeamNative1381Slice.cs`) | Native (`teaminfo.cpp`) |
|--------------------------------------|------------------------|
| Validity, change, getters, list, and natives | **232–434** |
| Band hull (exclusive slice) | **232–434** |

### Chunk **1381** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **203** |
| **Net headline** | **+203** vs the prior chunk |


**Post-review ratio:** 158/203 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmTeamNative1381SliceTests.ValidityChangeAndGetters`
- `VmTeamNative1381SliceTests.NativesEvaluateBeforeTheReturnCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1381 --to 1381` (0 hard / 0 soft).

## Seams

ClearTeams at 228-231 stays outside this band. TeamChanged, R_BuildPlayerTranslation, V_ParseFontColor, and TexMan.CheckForTexture are callees. MAXPLAYERS is 64. CR_UNDEFINED is -1 and CR_UNTRANSLATED is 11. ETextureType::Any is 0. ACTION_RETURN evaluates the value before the numret check and returns 0 when numret is 0. A null struct self aborts with NullParam. DEFINE_GLOBAL and DEFINE_FIELD_NAMED publish script names with bit value 0. The older VmTeamInfoRuntime gap filler is not this band and is not edited.

## Credit rationale

UNCredited teaminfo.cpp 232-434 (+203). After chunk 1380 this ties compatibility.cpp 1-203. Path order takes this file. Gaps longer than 400 lines stay whole and are not sliced.
