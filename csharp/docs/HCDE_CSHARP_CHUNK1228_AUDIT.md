# Chunk 1228 — DEFCVARS version gate, legacy renames, blacklist, and archive defaults from d_defcvars.cpp (1-163).

## Scope

DEFCVARS version gate, legacy renames, blacklist, and archive defaults from d_defcvars.cpp (1-163).

## Native ranges

- `src/d_defcvars.cpp` **D_GrabCVarDefaults** 1–163 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmDefCVars1228Slice.cs`
Managed LOC (tally): 216.
Conversion commit: `98b6ba1d121286fa24def8a80119a332b8155b1d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Supported version (**34–60**) | yes |
| Legacy rename and early stop (**65–101**) | yes |
| Blacklist and archive default (**103–161**) | yes |
| Band hull (**1–163**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Version 219 renames GAMMA before the lookup and leaves the tokens after cd_drive unread. A lump at the IWAD's last index is refused. A bad version number does not change a default.

| Managed (`VmDefCVars1228Slice.cs`) | Native (`d_defcvars.cpp`) |
|------------------------------------|---------------------------|
| Version gate, renames, and defaults | **1–163** |
| Band hull (exclusive slice) | **1–163** |

### Chunk **1228** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **163** |
| **Net headline** | **+163** vs post-**1227** |


**Post-review ratio:** 216/163 managed/native ≈ 133% (src file LOC at HEAD; see tally).

## Tests

- `VmDefCVars1228SliceTests.Grab_RenamesLegacyNamesAndStopsTheLump`
- `VmDefCVars1228SliceTests.Grab_RejectsAWadLumpAndABadVersion`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1228 --to 1228` (0 hard / 0 soft).

## Seams

atoi of LASTRUNVERSION is 234. A lump whose index plus one is past the IWAD's last entry prints the red wad warning and stops the scan. The first token must be version, compared without case. The version number is the scanner's base-0 integer, and MAXINT is the signed maximum. A version above 234 or below 219 is fatal and later lumps are not read. Below 220 the name is folded and gamma, fullscreen, and cd_drive are special: the first two are renamed and cd_drive abandons the rest of that lump without reading its value. Below 221, mouse_sensitivity and m_noprescale do the same. The blacklist is an exact name match. A system-only cvar is also refused. The message quotes the original token. An archived cvar stores the next token as its default. A known cvar without the archive bit, and an unknown name, skip one value and continue. A fatal missing token stops the whole grab.

## Credit rationale

UNCredited d_defcvars.cpp 1-163 (+163). The file ends at 163. A full tally line scan shows no overlap.
