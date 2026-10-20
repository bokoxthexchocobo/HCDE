# Chunk 81 — `v_colortables.cpp`

## Scope

`BuildTransTable`: RGB555/RGB666 index tables, `Col2RGB8` / inverse rows, and less-precision pointer rows with `0x3feffbff` masking.

## Managed mapping

- `VmColorBlendTables.cs`

## Tests

- `VmWholeFilePortTests.ColorBlendTables_BuildSwizzledRows`
