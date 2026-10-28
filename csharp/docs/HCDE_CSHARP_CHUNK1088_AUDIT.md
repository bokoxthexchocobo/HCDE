# Chunk 1088 — `CommandDrawInventoryBar` spacing tail + counter tick layout and `CommandDrawKeyBar` draw/parse head from `sbarinfo_commands.cpp` (**2251–2400**); follows **1087** spacing fallback (**2250**).

## Scope

`CommandDrawInventoryBar` spacing tail + counter tick layout and `CommandDrawKeyBar` draw/parse head from `sbarinfo_commands.cpp` (**2251–2400**); follows **1087** spacing fallback (**2250**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawInventoryBar::GetCounterSpacing / Tick / CommandDrawKeyBar** 2251–2400 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawInventoryBar1088Slice.cs`
Managed LOC (tally): 111.
Conversion commit: `c3b2a2d02a0de61ab53ebd6535a1ab4691e9118b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Live box width/height spacing (**2256–2264**) | yes (delegates **1087**) |
| Counter slot coordinates (**2280–2281**) | yes |
| Counter draw-number init (**2285–2287**) | yes |
| Counter shadow copy (**2288–2290**) | yes |
| Counter tick delegation loop (**2294–2295**) | yes (predicate) |
| Key bar ctor defaults (**2335–2338**) | yes |
| Key icon eligibility scan (**2351–2356**) | yes |
| Key draw offset gate (**2357**) | yes |
| Auto row width / slot stride (**2363–2377**) | yes |
| Row wrap + `reverseRows` (**2382–2389**) | yes |
| Key bar orientation parse head (**2403–2407**) | yes |
| Key bar `number` token (**2400–2401**) | yes (constant default context) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2248–2250** | **1087** null-box fallback seam |
| **2274–2278**, **2295** | host `new` / `Tick` delegate |
| **2297–2328** | inventory bar member layout |
| **2341–2362**, **2393–2396** | host `DrawGraphic` / inventory walk |
| **2401+** | parse flag loop outside hull |

| Managed (`VmSbarinfoCommandDrawInventoryBar1088Slice.cs`, `VmSbarinfoCommandDrawKeyBar1088Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------------------------------------------------|----------------------------------|
| Spacing + counter layout | **2252–2291** |
| Key bar draw/parse head | **2335–2400** |
| Band hull (exclusive slice) | **2251–2400** |

### Chunk **1088** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1087** |


**Post-review ratio:** 111/150 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.ComputeLiveInventoryBarCounterSpacing_UsesWidthWhenHorizontal`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.ResolveInventoryBarCounterSlotCoordinates_AdvancesVerticalSlots`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.ResolveInventoryBarCounterDrawNumberInit_MatchesNativeCounterFields`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.CreateDrawKeyBarCtorDefaults_MatchesNativeInit`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.IsKeyBarInventoryItemDrawable_RequiresValidKeyIcon`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.AdvanceKeyBarSlotOffset_ReversesStrideWhenRequested`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.ShouldWrapKeyBarRow_DetectsRowBoundary`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.AdvanceKeyBarRowOffset_SubtractsWhenReverseRows`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.ResolveKeyBarAutoSlotStride_AddsTwoPixelPadding`
- `VmSbarinfoCommandDrawInventoryBar1088SliceTests.ClassifyKeyBarOrientationKeyword_MapsVertical`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1088 --to 1088` (0 hard / 0 soft).

## Seams

**1087** owns null-box spacing **32** and parse/draw layout; **1088** completes live box spacing and counter slot math. Host `new CommandDrawNumber`, `DrawGraphic`, and inventory chain walks excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **2251–2400** (+150); GetCounterSpacing dimension branch, counter array layout flags, key bar ctor defaults + draw stride/row wrap; hull ends at key bar `number` parse (**2400**).
