# Chunk 522 — HCDEAppendCanonicalEventPayload

## Scope

HCDEAppendCanonicalEventPayload

## Native ranges

- `src/d_net.cpp` **HCDEAppendCanonicalEventPayload** 3747–3900 (154 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmDNetCanonicalEventPayload.cs`, `VmDNetCanonicalEventPayloadTests.cs`
Managed LOC (tally): 97.
Conversion commit: `83f1b91a920ec297625b4f91f7bb162d4628f27f`.

## Tests

- `VmDNetCanonicalEventPayloadTests.Summon2FamilyExtra_Is25`
- `VmDNetCanonicalEventPayloadTests.ZscCmdChain_AllTrue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 522 --to 522` (0 hard / 0 soft).

## Seams

`HCDEAppendFieldBytes`, `HCDEAppendCanonicalNullString`, and `HCDEReadBE16Field` are not hosted.

## Credit rationale

Credits canonical demo-event payload translation for SAY/cheat/ZSC/slot families from `d_net.cpp` (3747–3900).
