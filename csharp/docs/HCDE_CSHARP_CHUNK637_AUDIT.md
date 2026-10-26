# Chunk 637 — Whole `i_protocol.cpp` stream I/O (`i_protocol.cpp` 32–389)

## Scope

Whole `i_protocol.cpp` stream I/O (`i_protocol.cpp` 32–389)

## Native ranges

- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 32–38 (7 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 40–45 (6 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 47–52 (6 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 54–59 (6 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 61–66 (6 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 68–74 (7 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 76–85 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 87–96 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 98–108 (11 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 110–114 (5 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 116–121 (6 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 123–130 (8 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 132–143 (12 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 145–154 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 156–165 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 167–171 (5 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 175–185 (11 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 187–197 (11 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 199–210 (12 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 212–221 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 223–232 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 234–243 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 245–255 (11 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 257–266 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 268–277 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 279–298 (20 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 300–311 (12 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 313–316 (4 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 318–326 (9 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 328–337 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 339–350 (12 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 352–367 (16 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 369–378 (10 lines)
- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 380–389 (10 lines)

**Credited native LOC:** 323 (tally `nativeLoc` 323).

## Managed

`VmIProtocolUnchecked.cs`, `VmIProtocolChecked.cs`
Managed LOC (tally): 295.
Conversion commit: `5ee76215f7bd0c2fddd53b5a2a36d171e0221681`.

## Padding review (2026-10-21)

| Managed | Native (`i_protocol.cpp`) |
|---------|-------------------|
| `UncheckedReadStringNative` … `UncheckedReadDoubleNative` | 32–96 |
| `UncheckedWriteStringNative` … `UncheckedWriteDoubleNative` | 98–165 |
| Big-endian primitive helpers | shared read/write lanes |
| `AdvanceStreamNative` (unchecked + checked) | `AdvanceStream` 167–171 |
| `ReadStringNative` … `ReadDoubleNative` | 175–277 |
| `WriteStringNative` … `WriteDoubleNative` | 279–389 |
| `WriteFStringNative` | `WriteFString` 313–316 |

### Out-of-body uncredit (637)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **352** |
| **Out-of-body removed** | **29** |
| **Honest** | **323** |


**Post-review ratio:** 295/323 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmIProtocolStreamTests.UncheckedReadStringNative_NullTerminated`
- `VmIProtocolStreamTests.UncheckedReadInt16Native_BigEndian`
- `VmIProtocolStreamTests.ReadStringNative_Checked`
- `VmIProtocolStreamTests.ReadInt32Native_Checked`
- `VmIProtocolStreamTests.WriteInt16Native_Checked`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 637 --to 637` (0 hard / 0 soft).

## Seams

Unchecked string copy is inlined; checked paths use optional `fatal` instead of `I_Error`. `ProtocolStream` is uncredited on `VmIProtocolChecked.cs`.

## Credit rationale

Merged former chunks 637+638: unchecked primitives through `AdvanceStream`, then checked read/write through `WriteDouble` with boundary at `ReadString` (line 175).
