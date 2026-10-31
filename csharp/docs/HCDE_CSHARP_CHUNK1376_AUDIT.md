# Chunk 1376 — Canonical cvar change, run arguments, and event payload from d_net.cpp (3685-3901).

## Scope

Canonical cvar change, run arguments, and event payload from d_net.cpp (3685-3901).

## Native ranges

- `src/d_net.cpp` **HCDEAppendCanonicalEventPayload** 3685–3901 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

`VmCanonicalEvent1376Slice.cs`
Managed LOC (tally): 312.
Conversion commit: `1e6e891afc2e8c068ed9a0a4927a063cd9e14d0b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Blank line before the cvar change (**3685**) | yes |
| `HCDEAppendCanonicalCVarChange` (**3686–3719**) | yes |
| `HCDEAppendCanonicalRunArgs` (**3721–3745**) | yes |
| `HCDEAppendCanonicalEventPayload` (**3747–3900**) | yes |
| Blank line after the payload (**3901**) | yes |
| Band hull (**3685–3901**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `HCDEAppendCanonicalString` / null string / weapon index **3642–3684** | callees; already outside this hole |
| `HCDEAppendByte` / `BE16` / field bytes | earlier buffer primitives |
| `HCDEDemCommandName` **3902+** | next function, not this hole |

A cvar descriptor's high two bits are the type and the low six are the name length. A zero name, a type above string, or a short name fails after the descriptor byte is consumed. The type byte is written before the length-prefixed name. A failed name write can leave the big-endian length in place and does not consume the name. `DEM_SINFCHANGEDXOR` then copies one raw byte. Bool copies one byte, int and float copy four raw bytes, and string copies a null-terminated value. Named run arguments write the raw count byte, then mask that count with 127 before copying `count * 4` bytes. Unnamed arguments copy a two-byte script id and do not mask the count. A zero-length field copy still fails when the input cursor is already at the end of the buffer. Empty opcodes copy nothing. Say and taunt both take one byte plus a string. Slot writes read the slot and count before storing either byte. A weapon index keeps seven bits and, when the high bit is set, folds the next byte in at bit 7, then stores a big-endian ushort. `DEM_TAUNT` is 13, so later opcodes are one above the header comments (`DEM_SETSLOT` is 58, `DEM_USEFLECHETTE` is 80).

| Managed (`VmCanonicalEvent1376Slice.cs`) | Native (`d_net.cpp`) |
|------------------------------------------|---------------------|
| Cvar change, run arguments, and event payload | **3685–3901** |
| Band hull (exclusive slice) | **3685–3901** |

### Chunk **1376** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **217** |
| **Net headline** | **+217** vs the prior chunk |


**Post-review ratio:** 312/217 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmCanonicalEvent1376SliceTests.CVarChangeAndRunArgs`
- `VmCanonicalEvent1376SliceTests.EventPayloadFamilies`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1376 --to 1376` (0 hard / 0 soft).

## Seams

Append and read primitives, the canonical string, the null string, and the weapon index are callees just outside this hole. Their byte results are reproduced so the in-band calls keep native partial writes. EDemoCommand values are the compiler sequence; DEM_TAUNT shifts every later opcode by one from the header comments.

## Credit rationale

UNCredited d_net.cpp 3685-3901 (+217). After chunk 1375 the planner's largest 150-400 hole is this band, ahead of a_doors.cpp 343-547 (205). Gaps longer than 400 lines stay whole and are not sliced. A retired payload chunk no longer credits these lines.
