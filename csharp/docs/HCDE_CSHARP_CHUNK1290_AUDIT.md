# Chunk 1290 — Modal frame, tab order, and the launcher layout from launcherwindow.cpp (1-169).

## Scope

Modal frame, tab order, and the launcher layout from launcherwindow.cpp (1-169).

## Native ranges

- `src/widgets/launcherwindow.cpp` **LauncherWindow::ExecModal** 1–169 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmLauncherWindow1290Slice.cs`
Managed LOC (tally): 234.
Conversion commit: `ed2cf8e73d117ae6241aa1b5e3c778d373fc7157`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Modal frame (**34–47**) | yes |
| Constructor (**49–81**) | yes |
| Page commands (**83–130**) | yes |
| Language and layout (**132–169**) | yes |
| Band hull (**1–169**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| String table | host lookup returns the key when it is missing |
| Child pages | host language and value counts |
| Event loop | host calls start or exit |

The window title is HCDE. The changelog tab is added first only when both release flags are set, so the initial focus is that tab. Otherwise focus stays on Play. Multiplayer is the network page, and hosting also requires that page's host flag. Start always writes the settings, then the network page or the play page, and the changelog when it exists. Close clears the result. The modal frame is 615 by 700, centered on the screen. The constructor lays the children out before that frame is applied, so a zero client size leaves the button bar above the top and the page height at zero. A short client floors the page height at zero.

| Managed (`VmLauncherWindow1290Slice.cs`) | Native (`launcherwindow.cpp`) |
|------------------------------------------|-------------------------------|
| Frame, tabs, and the layout | **1–169** |
| Band hull (exclusive slice) | **1–169** |

### Chunk **1290** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **169** |
| **Net headline** | **+169** vs post-**1289** |


**Post-review ratio:** 234/169 managed/native ≈ 138% (src file LOC at HEAD; see tally).

## Tests

- `VmLauncherWindow1290SliceTests.Setup_PutsTheChangelogFirstAndStartsTheVisiblePage`
- `VmLauncherWindow1290SliceTests.Layout_StacksTheBannerAndFloorsThePages`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1290 --to 1290` (0 hard / 0 soft).

## Seams

String lookup and the page objects are host inputs. The event loop returns after the host calls start or exit.

## Credit rationale

UNCredited launcherwindow.cpp 1-169 (+169). A full tally line scan shows no overlap. The file ends at line 169.
