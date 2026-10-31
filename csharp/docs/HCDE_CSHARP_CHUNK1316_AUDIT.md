# Chunk 1316 — Counted malloc, the size prefix, and the free report from m_alloc.cpp (1-201).

## Scope

Counted malloc, the size prefix, and the free report from m_alloc.cpp (1-201).

## Native ranges

- `src/common/utility/m_alloc.cpp` **M_Malloc** 1–201 (201 lines)

**Credited native LOC:** 201 (tally `nativeLoc` 201).

## Managed

`VmCountedAlloc1316Slice.cs`
Managed LOC (tally): 235.
Conversion commit: `370c6fa94ca2b96348446ae261508cd2167e83e4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Release malloc and realloc (**54–114**) | yes |
| Release calloc (**116–121**) | yes |
| Debug malloc and realloc (**123–187**) | yes |
| Free and the prefix free (**190–201**) | yes |
| Band hull (**1–201**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The libc heap | the host returns the block and its measured size |

The release path fatals with the byte count and no file. The debug path appends the file and the line, and on a non-Microsoft build the debug allocator is ordinary malloc, so the file and the line do not change the heap size. A null realloc on the ordinary path still calls realloc and reports a realloc from a zero old size. A null realloc on the size-prefix path calls malloc instead and reports an alloc. The prefix is one size_t stored in front of the user pointer. Reports use _msize of the user pointer, which the sun stub forces to zero. Free of null does nothing. Free of a live block reports that measured size, then frees the user pointer or the prefix base. Calloc multiplies with size_t wrap and clears that many user bytes. A failed realloc leaves the old block in place and writes no report.

| Managed (`VmCountedAlloc1316Slice.cs`) | Native (`m_alloc.cpp`) |
|----------------------------------------|------------------------|
| Counted malloc and the free report | **1–201** |
| Band hull (exclusive slice) | **1–201** |

### Chunk **1316** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **201** |
| **Net headline** | **+201** vs post-**1315** |


**Post-review ratio:** 235/201 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmCountedAlloc1316SliceTests.Release_ReportsMsizeAndLeavesTheBlockOnFailure`
- `VmCountedAlloc1316SliceTests.Debug_StoresThePrefixAndNamesTheFile`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1316 --to 1316` (0 hard / 0 soft).

## Seams

malloc, realloc, free, and _msize are host operations. The sun stub returns zero from _msize.

## Credit rationale

UNCredited m_alloc.cpp 1-201 (+201). A full tally line scan shows no overlap. The file ends at line 201.
