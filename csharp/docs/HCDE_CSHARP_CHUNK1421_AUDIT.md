# Chunk 1421 — The first phase of D_InitGame, through the startup flags, in d_main.cpp (3856-4018).

## Scope

The first phase of D_InitGame, through the startup flags, in d_main.cpp (3856-4018).

## Native ranges

- `src/d_main.cpp` **D_InitGame** 3856–4018 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmDMain1421Slice.cs`
Managed LOC (tally): 244.
Conversion commit: `c936073f59c541ff0616846910fef5f158a104de`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| IWAD check and server init (**3856–3876**) | yes |
| Dummy screen and gameinfo (**3878–3896**) | yes |
| RNG seed (**3898–3912**) | yes |
| Config, autoload, and exec (**3914–3938**) | yes |
| Wad scan and lump filters (**3940–3967**) | yes |
| Filesystem init and resource parse (**3969–3994**) | yes |
| Sound init and startup flags (**3996–4018**) | yes |
| Band hull (**3856–4018**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| D_InitGame comment banner | ends at line 3854 |
| Startup type selection | starts at line 4020 |
| Server, screen, RNG, config, exec, filesystem, and sound bodies | callees |

A missing IWAD stops with the game-IWAD message before any other work. Server init stores the IWAD name. A debug value or vm_debug clears both JIT flags. Unless this is a restart, a client builds the dummy framebuffer and a dedicated server clears the screen. The save folder and game info are copied from the IWAD. A seed argument is parsed with atoi, stored as the static seed, and printed unless this is a batch run. Otherwise the made seed is used and the static flag stays clear. The seed is passed to srand and the random table is cleared. Callbacks are disabled, the game config is set up, and autoload files are added. Autoexec is skipped when noautoexec is set. Exec files and plus commands build the exec list, and a non-null list pulls wads. Batch runs skip the wad and sound prints. Lump filters add Raven, DoomStrifeChex, and DoomChex when the game type bits match, then the game name and the IWAD filter. A filter containing doom.id.doom also adds doom.doom plus the tail after 12 characters. A null game name crashes. Pwad names move onto the wad list and the source slots are left empty. Duplicate names are removed unless duplicates are allowed. No mounted entries stops with the no-files message and does not run the postprocess. A successful mount runs the postprocess, clears and shrinks the wad list, then marks mapxx, eternity, EDF, DEFCVARS, and the palette. The sound trace always runs. A texture guess above 0 becomes the greater of four times that guess and 4096. A guess of 0 or less uses 4096. The per-shader progress is 0. The startup screen is skipped for batch, restart, join, dedicated join, host, dedicated, or norun. Video init and the first shader stay false.

| Managed (`VmDMain1421Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| D_InitGame boot through startup flags | **3856–4018** |
| Band hull (exclusive slice) | **3856–4018** |

### Chunk **1421** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **163** |
| **Net headline** | **+163** vs the prior chunk |


**Post-review ratio:** 244/163 managed/native ≈ 150% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1421SliceTests.FatalDebugScreenAndRng`
- `VmDMain1421SliceTests.FilesFiltersAndProgress`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1421 --to 1421` (0 hard / 0 soft).

## Seams

Server mode, screen init, RNG, config, exec, filesystem, and sound calls stay callees. Startup type selection starts at line 4020. D_InitGame closes at line 4499.

## Credit rationale

UNCredited d_main.cpp 3856-4018 (+163). D_InitGame runs from line 3856 through line 4499, which is 644 lines, so this slice stops after the startup flags. The startup screen and the launch tail stay for the following slices. The band is not sliced further.
