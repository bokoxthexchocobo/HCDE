# MF5_NOINTERACTION tick — chunk 52 audit

**Native:** `p_mobj.cpp` `AActor::Tick` no-interaction branch 4587–4637.

**Managed:** `ActorNoInteractionTick.cs`, `Actor.NoInteraction`, `ActorNoInteractionTickTests`.

Co-op authority visual early-out and full time-freeze boss-cube paths are not converted.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
