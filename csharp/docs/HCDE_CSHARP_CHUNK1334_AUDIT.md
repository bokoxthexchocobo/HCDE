# Chunk 1334 — MAPINFO redirect, sound, exit, environment, hazard, fog, light, and shadow options from g_mapinfo.cpp (1491-1826), through the notexturefill opening brace.

## Scope

MAPINFO redirect, sound, exit, environment, hazard, fog, light, and shadow options from g_mapinfo.cpp (1491-1826), through the notexturefill opening brace.

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **redirect** 1491–1826 (336 lines)

**Credited native LOC:** 336 (tally `nativeLoc` 336).

## Managed

`VmMapInfoOptions1334Slice.cs`
Managed LOC (tally): 1424.
Conversion commit: `22ca174c2de8863f66ab26b35a6c0fe752fc7af5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| redirect body (**1491–1496**) | yes |
| cvar_redirect (**1498–1505**) | yes |
| sndseq, sndinfo, soundinfo, translator (**1507–1533**) | yes |
| deathsequence and slideshow (**1535–1547**) | yes |
| bordertexture, f1, mapbackground (**1549–1572**) | yes |
| teamdamage (**1561–1566**) | yes |
| exittext, textmusic, textflat, textpic (**1574–1608**) | yes |
| defaultenvironment (**1610–1638**) | yes |
| hazardcolor and hazardflash (**1640–1652**) | yes |
| fog densities, sky fog, thick fog, pixel ratio (**1654–1696**) | yes |
| brightfog (**1699–1704**) | yes |
| lightmode (**1706–1720**) | yes |
| lightblendmode (**1722–1771**) | yes |
| shadow profile, quality cap, max lights (**1773–1823**) | yes |
| notexturefill macro and opening brace (**1825–1826**) | yes |
| Band hull (**1491–1826**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| redirect macro and brace | chunk 1057, lines 1489-1490 |
| notexturefill body | starts at line 1827 |
| ParseNextMap | callee; number formats MAP or Hexen wt, end words become enDSeQ plus the name index |
| CheckEndSequence and ParseEndGame | callees of ParseNextMap |
| ParseExitText, ParseExitMusic, ParseExitBackdrop | callees; new backdrop also sets the music bit |
| V_GetColor | callee; X11 name, then #RGB, #RRGGBB, RRGGBB, or spaced hex |
| NormalizeHcdeShadowQualityCap | callee; tiers 128 through 8192 |
| S_FindEnvironment | callee; miss prints the unknown-environment line and stores 0 |

New format requires '=' and ','. Old format does not. Redirect and cvar redirect store the type string and the next map. sndinfo and soundinfo both store SoundInfo. Exit text joins new-format strings with newlines. lookup and a leading '$' set the lookup bit. A trailing-comma compat warning ungets a closing brace. Music order is the text after ':' via atoi, or the next number when there is no colon. A new backdrop sets the music bit; an update does not. Numeric environments shift the first number by 8 and or the optional second number. 8 and 16 clear light mode. 0 through 4 store the mode. Any other number prints and leaves the mode. Unclamped may take a tonemap name. A negative shadow quality or max-light cap stores -1. Profile clamps to -1..7. Bright fog narrows to 8 bits. Thick fog multiplier stores only a positive float. The notexturefill handler body is not in this band.

| Managed (`VmMapInfoOptions1334Slice.cs`) | Native (`g_mapinfo.cpp`) |
|------------------------------------------|-------------------------|
| Redirect, exit, light, fog, and shadow options | **1491–1826** |
| Band hull (exclusive slice) | **1491–1826** |

### Chunk **1334** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **336** |
| **Net headline** | **+336** vs post-**1333** |


**Post-review ratio:** 1424/336 managed/native ≈ 424% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoOptions1334SliceTests.StringsFogLightAndShadow`
- `VmMapInfoOptions1334SliceTests.RedirectExitBlendAndEndgame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1334 --to 1334` (0 hard / 0 soft).

## Seams

ParseAssign, ParseComma, and CheckNumber stay the format-sensitive parser. ParseNextMap, CheckEndSequence, ParseExitText, ParseExitMusic, ParseExitBackdrop, V_GetColor, S_FindEnvironment, and NormalizeHcdeShadowQualityCap stay callees. The notexturefill body stays the next credited band.

## Credit rationale

UNCredited g_mapinfo.cpp 1491-1826 (+336). A full tally line scan shows no overlap. This is tied for the largest remaining hole of at most 400 lines with a_ceiling.cpp 216-551. g_mapinfo sorts first by path, ahead of whole-file voxeldef.cpp 1-331. The redirect macro at 1489-1490 stays chunk 1057. The notexturefill body from 1827 stays later credit.
