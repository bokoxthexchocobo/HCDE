# Chunk 1204 — setdate, getdate, and SystemTime format from date.cpp (1–234).

## Scope

setdate, getdate, and SystemTime format from date.cpp (1–234).

## Native ranges

- `src/common/engine/date.cpp` **setdate** 1–234 (234 lines)

**Credited native LOC:** 234 (tally `nativeLoc` 234).

## Managed

`VmDate1204Slice.cs`
Managed LOC (tally): 320.
Conversion commit: `c7b8573421c0ee6a93d27f25ead7b6f65c08ad2d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| epochoffset (**29–29**) | yes |
| setdate (**40–136**) | yes |
| getdate (**138–156**) | yes |
| GetEpochTime (**166–171**) | yes |
| CheckStrfString (**174–202**) | yes |
| FormatTime (**204–218**) | yes |
| SystemTime exports (**220–234**) | yes |
| Band hull (**1–234**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

setdate requires the command plus a clock and a date. Each field must pass IsInt. A lone 0 is an integer, a leading zero is octal, and 0x is hex, so 08 is rejected. ToLong uses base 0. The broken-down fields are written onto the current local time and converted back. Out-of-range fields roll over. If either conversion fails, the offset stays put. Otherwise the offset is the new instant minus the old one, then the shown hour is compared with the requested hour. A difference of -1 or 23 adds 3600 seconds. A difference of 1 or -23 subtracts 3600. getdate prints the shifted local time with a trailing newline from the format and another from the print. GetEpochTime returns -1 when the clock fails. CheckStrfString rewrites %% to %a, prepends a, and rejects an empty token or a first character outside the valid set. FormatTime throws the VM format abort for a bad specifier. A failed local conversion or a result that does not fit in 1023 characters leaves the result empty.

| Managed (`VmDate1204Slice.cs`) | Native (`date.cpp`) |
|---------------------------------|---------------------|
| Date commands | **1–234** |
| Band hull (exclusive slice) | **1–234** |

### Chunk **1204** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **234** |
| **Net headline** | **+234** vs post-**1203** |


**Post-review ratio:** 320/234 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmDate1204SliceTests.SetDate_MovesTheEpochAndRejectsBadFields`
- `VmDate1204SliceTests.Format_MatchesCLocaleAndRejectsBadSpecifiers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1204 --to 1204` (0 hard / 0 soft).

## Seams

ReadClock and Zone stand in for time and localtime. Tests pin UTC. Integer fields use the FString IsInt predicate and strtoll base 0. Wall fields normalize the way mktime does, and the GCC hour nudge is applied after that. strftime names are the C locale. %Z for the UTC zone is UTC. Other zones use the host daylight or standard name. Years outside 1..9999 fail the conversion.

## Credit rationale

UNCredited date.cpp 1–234 (+234). The file ends at 234. A full tally line scan shows no overlap.
