# Chunk 927 — `PCD_TERMINATE` tail through `PCD_LSPEC5` stack path from `p_acs.cpp` (**7216–7365**). Follows **926** (**≤7215**); `PCD_LSPEC5` pop at **7366** and direct forms deferred.

## Scope

`PCD_TERMINATE` tail through `PCD_LSPEC5` stack path from `p_acs.cpp` (**7216–7365**). Follows **926** (**≤7215**); `PCD_LSPEC5` pop at **7366** and direct forms deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 7216–7365 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsRunscript927Slice.cs`
Managed LOC (tally): 93.
Conversion commit: `6da21097aad7564af7dfd311d5b04a247aa2e70c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PCD_TERMINATE` finish (**7216–7219**) | yes |
| `PCD_NOP` / `PCD_SUSPEND` (**7221–7226**) | yes (state seam) |
| `PCD_TAGSTRING` (**7228–7231**) | yes |
| `PCD_PUSHNUMBER` / `PUSHBYTE`…`PUSH5BYTES` (**7233–7275**) | yes |
| `PCD_PUSHBYTES` (**7277–7284**) | yes |
| `PCD_DUP` / `PCD_SWAP` (**7286–7293**) | yes |
| `PCD_LSPEC1`…`PCD_LSPEC5` (**7295–7365**) | yes (stack/mask) |

| Managed (`VmPAcsRunscript927Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------|---------------------|
| `MarkScriptFinished` | **7216–7219** |
| `TryInternTagStringOnStack` | **7228–7231** |
| `TryPushNumber` | **7233–7236** |
| `TryPushInlineBytes` | **7238–7275** |
| `TryPushCountedBytes` | **7277–7284** |
| `TryDup` / `TrySwap` | **7286–7293** |
| `MaskLineSpecialArg` / `TryPopLspecOperands` | **7295–7365** |
| `ReadEmbeddedLspecNumber` | **7298**, **7312**, etc. |
| `ResetStackOnMalformedLspec` | **7304–7306**, **7318–7320**, etc. |

### Chunk **927** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**926** |


**Post-review ratio:** 93/150 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsRunscript927SliceTests.TryInternTagStringOnStack_ReplacesTopWithPoolId`
- `VmPAcsRunscript927SliceTests.TryPushNumber_ReadsLittleEndianWord`
- `VmPAcsRunscript927SliceTests.TryPushCountedBytes_MatchesNativeLoopOrder`
- `VmPAcsRunscript927SliceTests.TryDup_AndSwap_ManipulateStackTop`
- `VmPAcsRunscript927SliceTests.TryPopLspecOperands_AppliesMaskAndPops`
- `VmPAcsRunscript927SliceTests.TryPopLspecOperands_FiveWide_FillsArgs`
- `VmPAcsRunscript927SliceTests.ResetStackOnMalformedLspec_ClearsSp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 927 --to 927` (0 hard / 0 soft).

## Seams

`P_ExecuteSpecial`, `GlobalACSStrings`, `Printf`, and full `RunScript` dispatch remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **6614–7509** corridor; exclusive `VmPAcsRunscript927Slice.cs`.
