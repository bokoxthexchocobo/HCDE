# Chunk 1420 — Texture hacks, GC roots, capture actions, and file-system messages in d_main.cpp (3646-3848).

## Scope

Texture hacks, GC roots, capture actions, and file-system messages in d_main.cpp (3646-3848).

## Native ranges

- `src/d_main.cpp` **PatchTextures** 3646–3848 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmDMain1420Slice.cs`
Managed LOC (tally): 292.
Conversion commit: `4dd3c67692583236a91c9c15762cfee1032aef19`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| PatchTextures (**3646–3660**) | yes |
| CheckForHacks (**3668–3722**) | yes |
| FixWideStatusBar (**3724–3735**) | yes |
| Doom_CastSpriteIDToString (**3743–3746**) | yes |
| GC_MarkGameRoots (**3752–3769**) | yes |
| System_ToggleFullConsole (**3771–3774**) | yes |
| System_StartCutscene (**3776–3779**) | yes |
| System_SetTransition (**3781–3784**) | yes |
| System_HudScaleChanged (**3786–3796**) | yes |
| FileNameCheck (**3801–3818**) | yes |
| FileSystemPrintf (**3820–3848**) | yes |
| Band hull (**3646–3848**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| System_M_Dim | declared at line 3644 |
| D_InitGame | starts at line 3856 |
| Texture, image, GC, level, status-bar, automap, and rollback bodies | callees |

PatchTextures runs only for exact Hexen. It looks up BLANK as a wall with flags 0. An id below 0 is a miss. An id of 0 or more exists, and a missing game texture crashes. A hit sets the use type to null. CheckForHacks returns when there are no parts. Heretic SKY1, SKY2, or SKY3 at height 128 with one part copies the image height and sets the texture size, then returns. The sky letters are case sensitive. A missing image crashes. Doom without the MAPxx flag, one part, height 128, and origin -8 clears that origin for the name SKY1. The same Doom gate clears both origins on BIGDOOR7 when the name matches regardless of case, there are two parts, the height is 128, and both origins are -4. Chex does not take the Doom gate. A status bar wider than 320, with scale exactly 1 and zero offsets, stores half of width minus 320 on both offset slots. A null bar is skipped. Sprite cast writes TNT1 when the id is past the sprite list, and a null destination or a null list crashes. GC marks the first handler, the last handler, every level, each in-game player through 64 slots, the next thinker, then rollback objects. A released object stays. An object marked for destruction is cleared. A white object turns gray. A null level list or a null level crashes. Full console stores action 17. A blocking cutscene stores intro action 19, otherwise intermission action 20. Wipe type 0 leaves the wipe state. Burn stores -3, fade stores -2, and every other type stores -4. HUD scale updates the status bar and requests a size change when a bar exists, and updates the automap when the primary level has one. A file whose base begins with textures, compared for 8 characters without case, is rejected when the opened file starts with LZ4. A short read, a failed open, or a lowercase lz4 stays allowed. A null base crashes. A null path crashes only after the textures prefix matches. File-system error text is printed with the red prefix and that print count is returned. Warning, attention, and message print the yellow prefix, the blue prefix, or the plain text, then return the unprefixed length. Debug warning prints only when developer is at least 2. Debug notify prints only when developer is at least 3. Both still return the text length. A dead game or a missing console prints nothing. An empty error string still returns the two-character red prefix. Format supports percent, string, signed integer, and unsigned integer. A null format crashes.

| Managed (`VmDMain1420Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Texture hacks, roots, and file messages | **3646–3848** |
| Band hull (exclusive slice) | **3646–3848** |

### Chunk **1420** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **203** |
| **Net headline** | **+203** vs the prior chunk |


**Post-review ratio:** 292/203 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1420SliceTests.TextureHacksRootsAndNames`
- `VmDMain1420SliceTests.FileSystemPrintfRoutes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1420 --to 1420` (0 hard / 0 soft).

## Seams

Texture, image, GC, level, status-bar, automap, and rollback calls stay callees. System_M_Dim is declared at line 3644. D_InitGame starts at line 3856.

## Credit rationale

UNCredited d_main.cpp 3646-3848 (+203). The span runs from PatchTextures through FileSystemPrintf. The NextToThink extern, the rollback prototype, and the option-block prototype sit inside the span. The DoomButtons table at 3283-3316 stays uncredited. D_InitGame is larger than one chunk and starts at line 3856. The band is not sliced further.
