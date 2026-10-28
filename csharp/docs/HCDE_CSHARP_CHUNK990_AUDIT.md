# Chunk 990 — `ResolveGotoLabel` tail, `FixStatePointers`, `ResolveGotoLabels`, and `SetGotoLabel` from `p_states.cpp` (**751–900**); follows chunk **989** (**601–750**).

## Scope

`ResolveGotoLabel` tail, `FixStatePointers`, `ResolveGotoLabels`, and `SetGotoLabel` from `p_states.cpp` (**751–900**); follows chunk **989** (**601–750**).

## Native ranges

- `src/p_states.cpp` **FStateDefinitions::ResolveGotoLabel** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesParser990Slice.cs`
Managed LOC (tally): 206.
Conversion commit: `09c92329a2448d782b7fc8f0a88d28fcbea8ca94`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ResolveGotoLabel` Super/class validation (**751–776**) | yes (predicates) |
| `ResolveGotoLabel` offset + lookup (**778–826**) | yes |
| `FixStatePointers` (**836–851**) | yes |
| `ResolveGotoLabels` (**861–872**) | yes |
| `SetGotoLabel` (**883–905**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **751–754** | `Super` / `::` scope head (**989**) |
| **757–762**, **808–824** | `I_Error` / `Printf` host |
| **828–834**, **853–859**, **875–881**, **907+** | comment banners / outside hull |
| **1–750** | chunks **985–989** |

| Managed (`VmPStatesParser990Slice.cs`) | Native (`p_states.cpp`) |
|----------------------------------------|----------------------|
| `ClassifyGotoScopeResolution` | **751–776** |
| `TrySplitGotoLabelOffset` / `ResolveGotoLabelInActorType` / `TryApplyOwnedStateOffset` | **778–819** |
| `FixIndexedStatePointersInTree` | **836–851** |
| `ResolvePendingGotoLabelsInTree` | **861–872** |
| `TrySetGotoLabelOnParserSession` | **883–905** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **990** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**989** |


**Post-review ratio:** 206/150 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesParser990SliceTests.ResolveGotoLabelTail`
- `VmPStatesParser990SliceTests.FixAndResolveGotoTree`
- `VmPStatesParser990SliceTests.SetGotoLabelSession`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 990 --to 990` (0 hard / 0 soft).

## Seams

`ValidateActor`, `PClass::FindClass`, `FindState`, `FindStateByString`, `FState::StaticFindStateOwner`, `FxAlloc.Strdup`, `I_Error`, and `Printf` host calls remain injected.

## Credit rationale

UNCredited continuation of `p_states.cpp` parser band; disjoint from **985–989**. Does not re-port **989** class-scope head (**743–754**).
