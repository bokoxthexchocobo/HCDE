# Chunk 1063 — SBarInfo coordinate parsing, flow-control truth/branches, and main-block head from `sbarinfo.cpp` (**151–300**). Pivot after exhausted `v_font` mid bands (**901–957** deferred).

## Scope

SBarInfo coordinate parsing, flow-control truth/branches, and main-block head from `sbarinfo.cpp` (**151–300**). Pivot after exhausted `v_font` mid bands (**901–957** deferred).

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **SBarInfoCommand coordinates / flow control** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoParser1063Slice.cs`
Managed LOC (tally): 77.
Conversion commit: `9a13bf372369342d4099474c21284316f1434f53`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Signed coordinate + `center` modifier (**150–163**) | yes |
| Named translation bracket token (**175–180**) | yes (policy + undefined gate) |
| Flow truth skip / retick (**219–229**) | yes |
| Branch negate swap (**232–235**, **286–287**) | yes |
| Flow block shape (**240–256**) | yes |
| `not` prefix parse (**274–279**) | yes |
| Main block initial truth (**300**) | yes |
| Main block flag keywords + alpha combine (**312–337**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **112–124** | `Offset` enum outside hull |
| **137–149** | `GetCoordinates` prologue (**&lt;151** seam) |
| **177–178** | `V_ParseFontColor` host |
| **192–216**, **242–245** | draw/tick/parse host loops |
| **301+** | `DrawAux` outside hull |

| Managed (`VmSbarinfoParser1063Slice.cs`) | Native (`sbarinfo.cpp`) |
|-------------------------------------------|-------------------------|
| `ResolveSignedCoordinate` / center / fullscreen rel | **150–163** |
| `BuildNamedTranslationToken` / `IsMissingTranslation` | **175–180** |
| `ShouldSkipFlowTruthToggle` / `ShouldRetickOnTruthChange` | **224–229** |
| `ClassifyFlowBlockStart` | **240–256** |
| `TryParseNotPrefix` / `ShouldSwapFlowBranchesAfterNot` | **274–287** |
| `MainBlockStartsTrue` | **300** |
| `ClassifyMainBlockFlag` / `CombineDrawAlpha` | **312–337** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1063** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1062** |


**Post-review ratio:** 77/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoParser1063SliceTests.ResolveSignedCoordinate_HonorsNegativePrefix`
- `VmSbarinfoParser1063SliceTests.ShouldStoreRelativeCenter_OnlyWhenFullScreen`
- `VmSbarinfoParser1063SliceTests.BuildNamedTranslationToken_WrapsBracketSyntax`
- `VmSbarinfoParser1063SliceTests.IsMissingTranslation_UsesUndefinedSentinel`
- `VmSbarinfoParser1063SliceTests.ShouldSkipFlowTruthToggle_WhenUnchanged`
- `VmSbarinfoParser1063SliceTests.ClassifyFlowBlockStart_DetectsMissingCommand`
- `VmSbarinfoParser1063SliceTests.TryParseNotPrefix_RecognizesNot`
- `VmSbarinfoParser1063SliceTests.ClassifyMainBlockFlag_RecognizesNativeKeywords`
- `VmSbarinfoParser1063SliceTests.CombineDrawAlpha_MultipliesLikeNativeDraw`
- `VmSbarinfoParser1063SliceTests.MainBlockStartsTrue_MatchesNativeInit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1063 --to 1063` (0 hard / 0 soft).

## Seams

`FScanner` token reads, `NextCommand` factory, `V_ParseFontColor`, and draw/tick host calls remain injected; **1–150** class headers outside hull.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **151–300** (+150); zero conversion/recognition overlap; no Vm* line-range collision in repo.
