# Chunk 1431 — soundlinks through MarkSound in s_advsound.cpp (1761-2062).

## Scope

soundlinks through MarkSound in s_advsound.cpp (1761-2062).

## Native ranges

- `src/sound/s_advsound.cpp` **soundlinks** 1761–2062 (302 lines)

**Credited native LOC:** 302 (tally `nativeLoc` 302).

## Managed

`VmAdvSound1431Slice.cs`
Managed LOC (tally): 360.
Conversion commit: `a016cd991e46de61aa9e3900cbf4f9f4b4e6bacd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| soundlinks and playersounds (**1767–1824**) | yes |
| Ambient ticker and tick (**1850–1943**) | yes |
| Activate and deactivate (**1953–2013**) | yes |
| MUSINFO and mark sound (**2022–2062**) | yes |
| Band hull (**1761–2062**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Player lookup | ends at line 1759 |
| Scanner and actor virtuals | callees |

soundlinks prints a plain alias, and skips a random header or a player reserve. playersounds records the first reserved names by link index, then prints each defined gender with a 16-wide label. A missing label crashes. A continuous ticker returns 1. A random ticker truncates rand over RAND_MAX times the span, then adds the minimum. Any other type returns the minimum. Tick always runs the actor tick. A positive countdown returns while the value stays positive. A clear special2 or a missing ambient returns. Volume scale is 1 when argument 1 is 0, otherwise that argument over 100, and the product is clamped to 0 through 1. A zero distance pair, or a minimum above the maximum, uses the standard sound. Otherwise the distances are scaled when argument 4 is positive. A loop stores the maximum int. A one-shot adds the ticker. The empty sound destroys the actor. Activate always calls the actor, destroys a missing ambient, and on the first arming fills a zero period from the length when the low type bits are clear. An invalid sound then destroys the actor. Random, periodic, and continuous add one ticker to a cleared countdown and set special2. Deactivate clears special2 and stops a continuous body channel. MUSINFO reads every lump. A full token is a number, including octal, hex, and MAXINT. Anything else starts a map. Unknown names use the default level, which is initialized once, and still consume music. Index 0 and negative indexes consume the music name and do not store it. A missing music string is fatal. Marking an ambient or a sound id returns 0.

| Managed (`VmAdvSound1431Slice.cs`) | Native (`s_advsound.cpp`) |
|------------------------------------|--------------------------|
| Ambient commands and MUSINFO | **1761–2062** |
| Band hull (exclusive slice) | **1761–2062** |

### Chunk **1431** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **302** |
| **Net headline** | **+302** vs the prior chunk |


**Post-review ratio:** 360/302 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmAdvSound1431SliceTests.LinksTickerAndPlayers`
- `VmAdvSound1431SliceTests.AmbientLifecycleAndMusic`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1431 --to 1431` (0 hard / 0 soft).

## Seams

Actor tick, activation, length, and lump tokenization stay callees. Player lookup ends at line 1759.

## Credit rationale

UNCredited s_advsound.cpp 1761-2062 (+302). This is the command printout, ambient actor, and MUSINFO tail. The band is shorter than 400 lines and is not sliced. The following remainder of the file is under 150 lines.
