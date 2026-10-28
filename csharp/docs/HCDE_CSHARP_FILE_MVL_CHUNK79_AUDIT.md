# Chunk 79 — `file_mvl.cpp`

## Scope

Full DMVL archive signature probe, 17-byte directory entries, payload offset accumulation, and payload extraction wired through `VmMvlArchiveReader`.

## Managed mapping

- `VmMvlResourceProbe.cs`
- `VmMvlArchiveReader.cs` (delegates directory layout to the probe)

## Tests

- `VmWholeFilePortTests.MvlResourceProbe_ReadsDirectoryOffsets`
- `VmArchiveTests.MvlScriptsPreserveDirectoryOrderAndResolveIncludes`
