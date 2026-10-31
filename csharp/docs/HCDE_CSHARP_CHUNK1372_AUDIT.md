# Chunk 1372 — Thing-header dehacked tail, reverb properties and header, and the thing-block scan through the nested opener from hcde_edf.cpp (757-976).

## Scope

Thing-header dehacked tail, reverb properties and header, and the thing-block scan through the nested opener from hcde_edf.cpp (757-976).

## Native ranges

- `src/gamedata/hcde_edf.cpp` **EdfReverbThing** 757–976 (220 lines)

**Credited native LOC:** 220 (tally `nativeLoc` 220).

## Managed

`VmEdfBand1372Slice.cs`
Managed LOC (tally): 577.
Conversion commit: `337ee6811fb82382c8cb5d30f353722fe1f348b4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dehacked loops and the header return (**757–773**) | yes |
| Closing braces of `ParseThingHeader` (**774–775**) | yes |
| `ParseReverbPropertyLine` (**776–883**) | yes |
| `ParseReverbHeader` (**885–925**) | yes |
| `ParseThingBlock` through the stat counters (**927–949**) | yes |
| Heredoc exit and the nested-brace cut (**951–976**) | yes |
| Band hull (**757–976**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| hcde_edf.cpp **729–756** | header name, base, and doomednum, chunk 18 |
| hcde_edf.cpp **977–980** | `ParseThingPropertyLine` call, chunk 18 |
| hcde_edf.cpp **149–384** | trim, lower, tokenize, and strict int; callees, not this hole |
| hcde_edf.cpp **981–1001** | heredoc enter, brace depth, and the definition push; the next gap |

`ParseIntStrict` is `strtol` base 0. The whole token must be consumed. `0x2A` is 42 and `010` is 8. A trailing character or a value outside `int` is rejected. After the doomednum is stored, the next strict integer in the header is `DehackedNum`. The search then returns, so a later colon is not read. One integer leaves `DehackedNum` at -1.

Reverb keys are compared after ASCII lower. `roomsize`, `damping`, `wetscale`, `dryscale`, `width`, and the five `eq.*` keys store `std::stof` of the next token, which accepts a numeric prefix and throws when no conversion is possible. `predelay` stores the next strict integer. `+equalized` sets `DecayHFLimit` to 1 and `-equalized` sets it to 0. Any other key increments the unsupported count and records `property 'key' is not supported`. Empty text does not increment the property count. The issue list stops at 20. A negative lump is source `<unknown>`. A non-negative lump asks `GetFileFullName`.

The reverb header stores `tokens[1]` as the name, then the first two strict integers after the colon as `Id1` and `Id2`. Both start at -1. A thing block with an empty name records `missing thing name` under `thingdelta` or `thingtype` and does not count a type. Otherwise it counts a delta or a type. The scan trims with the C space set, skips a blank line, and while a heredoc is open skips until `"@`. Other lines cut at the first `{` and keep the untrimmed prefix. `nestedDepth` is set to 0 here; the property call that reads it is chunk 18.

| Managed (`VmEdfBand1372Slice.cs`) | Native (`hcde_edf.cpp`) |
|-----------------------------------|------------------------|
| Dehacked tail, reverb, thing-block scan | **757–976** |
| Band hull (exclusive slice) | **757–976** |

### Chunk **1372** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **220** |
| **Net headline** | **+220** vs the prior chunk |


**Post-review ratio:** 577/220 managed/native ≈ 262% (src file LOC at HEAD; see tally).

## Tests

- `VmEdfBand1372SliceTests.HeaderDehackedAndThingScan`
- `VmEdfBand1372SliceTests.ReverbHeaderAndProperties`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1372 --to 1372` (0 hard / 0 soft).

## Seams

TokenizeSimple, TrimCopy, LowerCopy, ParseIntStrict, and std::stof stay outside this band and are invoked from it. The thing-header prefix at 729-756 and the property call at 977-980 stay chunk 18.

## Credit rationale

UNCredited hcde_edf.cpp 757-976 (+220). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines after decallib.cpp 1-222. It is not a tie. hcde_servermode.cpp 1-218 and p_switch.cpp 235-452 are 218. Gaps longer than 400 lines stay whole and are not sliced. The band sits between chunk 18 ranges 729-756 and 977-980.
