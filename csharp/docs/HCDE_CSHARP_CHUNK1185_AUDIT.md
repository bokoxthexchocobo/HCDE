# Chunk 1185 — Scoreboard cvars, player order, draw, and toggle from hu_scores.cpp (1–156).

## Scope

Scoreboard cvars, player order, draw, and toggle from hu_scores.cpp (1–156).

## Native ranges

- `src/hu_scores.cpp` **HU_DrawScores** 1–156 (156 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmHuScores1185Slice.cs`
Managed LOC (tally): 81.
Conversion commit: `9a5d3c506efc4c24aae738a8e4f991ac2f2041c7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| scoreboard cvars (**68–79**) | yes |
| comparepoints (**81–94**) | yes |
| compareteams (**96–113**) | yes |
| HU_DrawScores (**138–151**) | yes |
| togglescoreboard (**153–156**) | yes |
| Band hull (**1–156**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Deathmatch order uses frag count. Otherwise it uses kill count. Equal counts fall through to a case-insensitive name compare. Team order uses the team number first, then frags, then the name. Draw leaves the toggle set only when a status bar exists and its draw returns nonzero. A missing bar does not call draw and clears the toggle. The console command flips the toggle. The commented sort helper is not live code.

| Managed (`VmHuScores1185Slice.cs`) | Native (`hu_scores.cpp`) |
|-------------------------------------|--------------------------|
| Order, draw, and toggle | **1–156** |
| Band hull (exclusive slice) | **1–156** |

### Chunk **1185** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **156** |
| **Net headline** | **+156** vs post-**1184** |


**Post-review ratio:** 81/156 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmHuScores1185SliceTests.ComparePoints_UsesFragsInDeathmatchAndKillsOtherwise`
- `VmHuScores1185SliceTests.CompareTeams_OrdersByTeamThenFragsThenName`
- `VmHuScores1185SliceTests.DrawAndToggle_ClearTheFlagUnlessTheBarDraws`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1185 --to 1185` (0 hard / 0 soft).

## Seams

DrawScores asks the host status bar to draw. The virtual call's integer result is nonzero-as-true. Name order uses ordinal case-insensitive compare, matching stricmp.

## Credit rationale

UNCredited hu_scores.cpp 1–156 (+156). The file ends at 156. Playsim has no remaining clean hole of at least 150 lines outside the standing skips. A full tally line scan shows no overlap.
