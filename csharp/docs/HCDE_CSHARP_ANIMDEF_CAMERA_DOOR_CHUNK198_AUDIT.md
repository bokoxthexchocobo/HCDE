# Chunk 198 — ANIMDEFS camera/canvas and animated door

`VmAnimDefCameraParser`, `VmAnimDefDoorParser`, wiring in `VmAnimDefDocumentParser`.

Tests: `VmAnimDefExtendedClauseTests` (camera, door).

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
