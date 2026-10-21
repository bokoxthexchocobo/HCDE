# Chunk 653 — HCDEAppendLegacyRunArgs and legacy event payload sizing helpers (`d_net.cpp` 4111–4299)

## Scope

HCDEAppendLegacyRunArgs and legacy event payload sizing helpers (`d_net.cpp` 4111–4299)

## Native ranges

- `src/d_net.cpp` **HCDEAppendLegacyRunArgs** 4111–4136 (26 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4144–4161 (18 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4162–4177 (16 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4184–4204 (21 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4211–4254 (44 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4256–4299 (44 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmDNetLegacyEventPayload.cs`
Managed LOC (tally): 92.
Conversion commit: `f1b7d745ba0eb57450cb2524f8268b470ec404f9`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `TryPlanLegacyRunArgs` / `MaskRunArgCount` / `UsesNamedRunArgs` | 4111–4136 |
| `IsZeroPayloadEvent` | 4144–4161 |
| `SetInvTrailingBytes` | 4162–4171 |
| `GiveTakeCheatTrailingBytes` | 4173–4177 |
| `NetEventTrailingBytes` / `Summon2TrailingBytes` | 4184–4204 |
| `ChangeMap2LeadingBytes` / `WarpCheatFieldBytes` | 4211–4230 |
| `ConvReplyFieldBytes` | 4232–4254 |
| `UsesLegacyCVarChange` / `UsesRunArgs` | 4256–4269 |
| `SetSlotNeedsPnumPrefix` / `TryReadSetSlotHeader` | 4275–4299 |


**Post-review ratio:** 92/169 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetLegacyEventPayloadTests.TryPlanLegacyRunArgs_NamedMasksCount`
- `VmDNetLegacyEventPayloadTests.MaskRunArgCount_NamedClearsHighBit`
- `VmDNetLegacyEventPayloadTests.IsZeroPayloadEvent_Pause`
- `VmDNetLegacyEventPayloadTests.SetInvTrailingBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.GiveTakeCheatTrailingBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.NetEventTrailingBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.Summon2TrailingBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.ChangeMap2LeadingBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.ConvReplyFieldBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.UsesLegacyCVarChange_SinfchangedFamily`
- `VmDNetLegacyEventPayloadTests.TryReadSetSlotHeader_RoundTrip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 653 --to 653` (0 hard / 0 soft).

## Seams

Live `HCDEAppendLegacyEventPayload` buffer append and `.inl` tails are injected; constants mirror `DEM_*` sizes only.

## Credit rationale

Credits helper bands mirrored by `VmDNetLegacyEventPayload` and tests — not playsim `DoDamage` seams.
