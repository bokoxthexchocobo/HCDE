# Chunk 542 — HCDEAppendLegacyRunArgs and HCDEAppendLegacyEventPayload switch (pre-ADDSLOT)

## Scope

HCDEAppendLegacyRunArgs and HCDEAppendLegacyEventPayload switch (pre-ADDSLOT)

## Native ranges

- `src/d_net.cpp` **HCDEAppendLegacyRunArgs** 4111–4135 (25 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4144–4163 (20 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4162–4171 (10 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4173–4177 (5 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4184–4187 (4 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4199–4204 (6 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4211–4226 (16 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4232–4254 (23 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4228–4230 (3 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4256–4269 (14 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4275–4298 (24 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmDNetLegacyEventPayload.cs`, `VmDNetLegacyEventPayloadTests.cs`
Managed LOC (tally): 92.
Conversion commit: `7fc36df86ac4321a17d61a452ebc908f241948ce`.

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
- `VmDNetLegacyEventPayloadTests.WarpCheatFieldBytes_MatchesNative`
- `VmDNetLegacyEventPayloadTests.UsesLegacyCVarChange_SinfchangedFamily`
- `VmDNetLegacyEventPayloadTests.UsesRunArgs_RunscriptVariants`
- `VmDNetLegacyEventPayloadTests.SetSlotNeedsPnumPrefix_OnlyPnumVariant`
- `VmDNetLegacyEventPayloadTests.TryReadSetSlotHeader_RoundTrip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 542 --to 542` (0 hard / 0 soft).

## Seams

`HCDEAppendFieldBytes`, `HCDEAppendLegacyStringFromCanonical`, `HCDEAppendByte`, and `HCDEAppendLegacyCVarChange` I/O are injected; `DEM_ADDSLOT` continuation in `.inl` files is not hosted.

## Credit rationale

Credits native bands mirrored by `VmDNetLegacyEventPayload` helpers and tests — not live buffer append or invasion snapshot tail in `d_net_snapshot_part2.inl`.

## Port fidelity review

| Native symbol | Approach | Notes |
|---------------|----------|-------|
| `HCDEAppendLegacyRunArgs` | Whole-function control flow (named vs unnamed arg header, arg-count mask, `argCount * 4` tail) | Field append hosted at seams |
| `HCDEAppendLegacyEventPayload` | Per-`case` field-size and routing helpers only through `DEM_SETSLOT` / `DEM_SETSLOTPNUM` | Stops before `DEM_ADDSLOT` `.inl` continuation; no buffer I/O in managed code |

**Totals:** 150 native / 92 managed. Reaudit 542: 0 hard / 0 soft.
