# Chunk 975 — `HandleReply` reply lookup, item-check failure, inventory give/take, log/terminal hooks from `p_conversation.cpp` (**451–600**); follows chunk **974** (**≤450**).

## Scope

`HandleReply` core through terminal response gate; `NextNode` body (**602+**) deferred to a follow-on band.

## Native ranges

- `src/p_conversation.cpp` **HandleReply** hull 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationHandleReply975Slice.cs`

## Tests

Listed in `chunk975.json` nativeRanges tests array.

## Seams

Spawn/pickup/special/slideshow/terminal/clear and `NextNode` continuation remain host-injected.

## Credit rationale

UNCredited continuation on `p_conversation.cpp`; item-check mirrors **973** without re-crediting that band.
