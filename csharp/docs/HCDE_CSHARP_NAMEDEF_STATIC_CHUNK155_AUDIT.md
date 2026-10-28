# Chunk 155 — `namedef.h` static table

`VmPredefinedNames` + generated `VmPredefinedNames.g.cs`: index lookup over the predefined `xx()` name list.

**Not credited:** `FindDuplicates` constexpr build, runtime `FName::NameManager` (chunk 150).

Tests: `VmPredefinedNamesTests.GetIndex_NoneIsZero`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
