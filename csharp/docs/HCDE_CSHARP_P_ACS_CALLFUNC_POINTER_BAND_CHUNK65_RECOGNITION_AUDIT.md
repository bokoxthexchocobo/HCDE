# ACS CallFunction pointer/net/activator band — chunk 65 recognition audit

**Native:** `p_acs.cpp` `DLevelScript::CallFunction` uncredited lines 5492–5674 (GetNetID, SetActivator*, GetChar, armor getters).

**Managed:** Pre-existing `AcsNetId`, `AcsCallFunctions` activator/char/armor paths.

**Bucket:** recognition (`managedLoc` 0). Named Playsim tests only; no `conversionManagedCommit`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
