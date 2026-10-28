# Chunk 996 — `displaynametags`, session globals, and `sv_gametype` rule mapping from `g_game.cpp` (**225–374**); follows chunk **995** (**1–150**). Starts after chunk **9** (**204–224**).

## Scope

`displaynametags`, session globals, and `sv_gametype` rule mapping from `g_game.cpp` (**225–374**); follows chunk **995** (**1–150**). Starts after chunk **9** (**204–224**).

## Native ranges

- `src/g_game.cpp` **CUSTOM_CVAR(sv_gametype)** 225–374 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameCvars996Slice.cs`
Managed LOC (tally): 82.
Conversion commit: `e3d6932738c404d075bd6d9f1151b061b1c563f2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `displaynametags` clamp (**314–319**) | yes |
| `sv_gametype` validation (**341–345**) | yes |
| `sv_gametype` DM/team/CTF/invasion mapping (**347–375**) | yes |
| Session flag globals (**244–248**) | yes (aggregate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **204–224** | chunk **9** `G_TraceSetGameState` |
| **225–313** | `EXTERN_CVAR` / globals / `FARG` banners |
| **360–363** | `Printf` host (CTF placeholder message) |
| **322–337**, **290–298** | bare `CVAR` decls |
| **1–224** | chunks **995** / **9** |

| Managed (`VmGGameCvars996Slice.cs`) | Native (`g_game.cpp`) |
|-------------------------------------|----------------------|
| `NormalizeDisplayNameTags` / `ShouldClampDisplayNameTags` | **316–318** |
| `ClampSvGameType` / `ShouldRejectSvGameType` | **341–345** |
| `ResolveMultiplayerRulesFromSvGameType` / helpers | **347–375** |
| `CollectGameSessionFlags` | **244–248** |
| Band hull (exclusive slice) | **225–374** |

### Chunk **996** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**995** |


**Post-review ratio:** 82/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameCvars996SliceTests.DisplayNameTagsClamp`
- `VmGGameCvars996SliceTests.SvGameTypeRules`
- `VmGGameCvars996SliceTests.SessionFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 996 --to 996` (0 hard / 0 soft).

## Seams

`Printf`, `CVAR` registration macros, and `FARG` help text remain host-injected.

## Credit rationale

UNCredited **225–374** CLI band; disjoint from chunk **9** trace (**204–224**) and chunk **995** head. Do not take **151–300** (overlaps **204–224**).
