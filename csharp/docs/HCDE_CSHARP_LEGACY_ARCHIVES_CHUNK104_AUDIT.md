# Chunk 104 — `file_grp.cpp`, `file_hog.cpp`, `file_rff.cpp` (recognition)

GRP/HOG/RFF directory layout and RFF XOR decrypt already live in managed code since `b051701e` (`VmGrpArchiveReader`, `VmHogArchiveReader`, `VmRffArchiveReader` — ~104 LOC combined). Chunk `71ea4f56` only made types public and added `VmLegacyArchiveReaderTests`; that is **recognition**, not new conversion (0 managed LOC credited here).

Tests: `VmLegacyArchiveReaderTests`.

## Addendum (re-audit)

Prior entry incorrectly bucketed this chunk as **conversion** with **12** managed LOC against **179** native LOC. Twelve lines cannot represent RFF decrypt plus full GRP/HOG parsers; the behavior was pre-existing. Reclassified to **recognition** with native ranges unchanged and tests cited as parity evidence.
