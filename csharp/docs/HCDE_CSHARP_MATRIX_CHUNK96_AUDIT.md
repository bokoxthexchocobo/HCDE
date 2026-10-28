# Chunk 96 — `matrix.cpp` / `VSMatrix`

## Faithful

Column-major multiply, translate/scale/rotate, lookAt, perspective/ortho/frustum, transpose, quaternion multiply, `multMatrixPoint`, `computeNormalMatrix`, `inverseMatrix`.

## Seams

- `USE_DOUBLE` overloads and unused `setIdentityMatrix` helper not exposed.
- Vector helpers (`dotProduct`, `add`, `subtract`, `length`) inlined privately for lookAt only.

## Tests

- `VmVsMatrixTests`.
