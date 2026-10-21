# Chunk 522 — HCDEAppendCanonicalEventPayload

## Scope

HCDEAppendCanonicalEventPayload

## Native ranges

- `src/d_net.cpp` **HCDEAppendCanonicalEventPayload** 3747–3900 (154 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

(see conversion commit file list)
Managed LOC (tally): 218.
Conversion commit: `be84d76402af8dff8033b04233f8782e44a11aba`.

## Tests

- `CanonicalEventPayloadCodecTests.GiveCheat_StringPlusInt`
- `CanonicalEventPayloadCodecTests.SetSlot_CanonicalizesSlotCountAndWeaponIndices`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 522 --to 522` (0 hard / 0 soft).

## Seams

`HCDEAppendFieldBytes`, `HCDEAppendCanonicalNullString`, and `HCDEReadBE16Field` are not hosted.

## Credit rationale

Credits canonical demo-event payload translation for SAY/cheat/ZSC/slot families from `d_net.cpp` (3747–3900).
