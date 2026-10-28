# Chunk 162 — typed CVars and latch

`VmBoolCvar`, `VmIntCvar`, `VmCvarLatchBuffer` (`SetGenericRep` latch + `Unlatch`).

**Not credited:** `FFloatCVar`, `FStringCVar`, serverinfo latch gate, `C_CreateCVar` type factory.

Tests: `VmTypedCvarTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
