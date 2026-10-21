# Chunk 541 — HCDE DEM naming, event-record conversion loop, canonical string + legacy weapon/CVar append

## Scope

HCDE DEM naming, event-record conversion loop, canonical string + legacy weapon/CVar append

## Native ranges

- `src/d_net.cpp` **HCDEDemCommandName** 3902–3923 (22 lines)
- `src/d_net.cpp` **HCDEAppendEventRecords** 3932–4026 (95 lines)
- `src/d_net.cpp` **HCDEReadCanonicalString** 4028–4051 (24 lines)
- `src/d_net.cpp` **HCDEAppendLegacyWeaponIndex** 4062–4072 (11 lines)
- `src/d_net.cpp` **HCDEAppendLegacyCVarChange** 4074–4109 (36 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmDNetAppendEventRecordsTests.cs`
Managed LOC (tally): 263.
Conversion commit: `57abde1f65e7ff2f889f130e97cbf23a97ca3780`.

## Tests

- `VmDNetAppendEventRecordsTests.DemCommandName_KnownPause`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_ClientInputFilterRewinds`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_RejectsDisallowedTicEvent`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_RejectsCursorAtCapacity`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_WritesTwoEventPayloadSizes`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_RejectsPayloadTooLarge`
- `VmDNetAppendEventRecordsTests.TryReadCanonicalString_RoundTrip`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyWeaponIndex_TwoByteForm`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyCVarChange_BoolPayload`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 541 --to 541` (0 hard / 0 soft).

## Seams

`HCDEAppendCanonicalEventPayload`, `HCDEReadByteField`, `HCDEAppendByte`, and live `HCDELiveWriteBE16` buffer mutation are injected at seams; debug trace logging is not hosted.

## Credit rationale

Credits whole `HCDEDemCommandName`, `HCDEAppendEventRecords`, `HCDEReadCanonicalString`, `HCDEAppendLegacyWeaponIndex`, and `HCDEAppendLegacyCVarChange` bodies mirrored in managed code (not helper-only extractions).

## Port fidelity review

| Native symbol | Prior credit | Finding | Correction |
|---------------|-------------|---------|------------|
| `HCDEAppendEventRecords` | Full loop credited while managed code only exposed precondition/rewind helpers | Loop control flow (count header, per-event payload size, client-input rewind, BE16 count write) was not hosted | Added `VmDNetAppendEventRecords.AppendEventRecords` mirroring 3932–4026 with injectable read/append seams; five named loop tests |
| `HCDEAppendLegacyCVarChange` | Full function credited with header helper only | Managed code did not walk type switch / field-byte append | Added `TryAppendLegacyCVarChange` mirroring 4074–4109 |
| Other credited symbols | Matched | `DemCommandName`, canonical string read, and legacy weapon index already aligned | Ranges unchanged |

**Corrected totals:** 188 native / 263 managed (tally). Reaudit 538–541: 0 hard / 0 soft.
