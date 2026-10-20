# Chunk 251 — Re-audit: ParseReverbPropertyLine through equalized flags.

## Scope

Re-audit: ParseReverbPropertyLine through equalized flags.

## Native ranges

- `src/gamedata/hcde_edf.cpp` **ParseReverbPropertyLine** 776–841 (66 lines)

**Credited native LOC:** 66 (tally `nativeLoc` 66).

## Managed

`HcdeEdfReverbParser.cs`, `HcdeEdfReverbParserTests.cs`
Managed LOC (tally): 52.
Conversion commit: `8ca7aa91b72b0e6732a51568b621d01182b9c363`.

## Tests

- `HcdeEdfReverbParserTests.TryApplyPropertyLine_ParsesRoomsize`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 251 --to 251` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
