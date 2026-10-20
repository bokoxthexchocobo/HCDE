# Chunk 85 — `r_memory.cpp`

## Scope

16-byte aligned bump allocation across 1 MiB blocks with free-list recycle on `Clear`.

## Faithful reproduction

`AllocBytes`, `Clear`, block rollover (aligned platform alloc stubbed as `byte[]`).

## Tests

`VmRenderMemoryArenaTests`
