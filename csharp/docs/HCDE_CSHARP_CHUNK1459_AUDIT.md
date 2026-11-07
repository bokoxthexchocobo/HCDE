# Chunk 1459 — Bandwidth reset, lane budget end, and live control capabilities in d_net.cpp (2253-2402).

## Scope

Bandwidth reset, lane budget end, and live control capabilities in d_net.cpp (2253-2402).

## Native ranges

- `src/d_net.cpp` **Net_ResetHCDEBandwidthAutoState** 2253–2402 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmNet1459Slice.cs`
Managed LOC (tally): 184.
Conversion commit: `5a7c4e034a04df4eb299753ad023b56a4974eca0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Reset through report interval (**2253–2402**) | yes |
| Band hull (**2253–2402**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Bandwidth tick | lines 1993-2251 are already credited |
| Passive resend | lines 2418 onward are already credited |
| Comment banner | lines 2404-2417 stay uncredited |

A lane budget of zero, or a cursor already at the end, returns the output capacity and does not record a clamp. A short or unrecognized capability block is a legacy control. A bad version or flag clears the negotiated mask. Unknown bits are logged on every accepted control, and the session id is read only when the flag is set and the payload is the long form.

| Managed (`VmNet1459Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| Reset through ReportElapsed | **2253–2402** |
| Band hull (exclusive slice) | **2253–2402** |

### Chunk **1459** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **150** |
| **Net headline** | **+150** vs the prior chunk |


**Post-review ratio:** 184/150 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1459SliceTests.ResetBudgetShapeAndInterval`
- `VmNet1459SliceTests.ApplyAcceptsExtensionAndRejectsLegacy`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1459 --to 1459` (0 hard / 0 soft).

## Seams

Net_ResetHCDEBandwidthAutoState through HCDELiveReportIntervalElapsed. The bandwidth tick ends at 2251. The passive-resend comment starts at 2404 and that function is already credited at 2418.

## Credit rationale

UNCredited d_net.cpp 2253-2402 (+150). One contiguous span of the bandwidth reset, lane budget end, capability advertisement, and the report interval. The span does not cross credited code.
