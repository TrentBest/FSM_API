# FSM runtime — tick and mutation contract

**Status:** Implementation contract for integer-backed runtime scheduling and definition replacement.

## 1. The tick is a consistency boundary

A call to `FSMRuntime.Update(processingGroupID)` or `UpdateAll()` is one logical tick. A direct public `FSMHandle.Update()` call is wrapped in a single-handle runtime scope with the same deferred-registration finalization; it does not schedule sibling handles. Every definition and handle selected for that tick must execute against a stable runtime definition set.

User callbacks are arbitrary application code. They may register or replace definitions, create or remove handles, unregister definitions, invalidate contexts, throw exceptions, or attempt to re-enter the scheduler. These operations must not cause one tick to observe a mixture of old and replacement definitions.

The runtime snapshots eligible handles and the definition-array extent at tick start. A handle created during a callback is registered immediately but is not eligible until a later tick. A definition registered during a callback is queued and becomes active only after all work selected for the current tick has finished.

## 2. Definition replacement

Registering a definition whose identity is already registered replaces the blueprint; it does not destroy its live handles.

At the replacement commit boundary:

- Every still-registered handle with the matching FSM identity is rebound to the replacement definition.
- Handle identity, caller-owned context reference, destruction state, and current state identity are preserved.
- No old-state Exit or Destroy callback is synthesized merely because the blueprint was replaced.
- If the replacement no longer contains a handle's current state, the handle is marked as not having entered its current state. Normal recovery then selects the replacement's valid initial state and runs its Enter callback through the ordinary update lifecycle. If the replacement has no valid initial state, the handle enters a suspended waiting state rather than repeating the same failed recovery on every tick; a later valid replacement clears that suspension.
- The replacement definition's processing group and process rate govern subsequent scheduling.
- The definition-level error count and scheduler counter are reset for the replacement.

When several replacements for the same identity are requested in one tick, the last request wins. The pending replacement is not visible through definition lookup until the boundary commit. Code that needs the new definition must wait until the current tick has returned.

## 3. Callback re-entrancy

A callback may call `Register`; that request is deferred. This applies uniformly to requests made from Enter, Update, transition-condition, Exit, and teardown callbacks.

A callback must not recursively invoke `Update` or `UpdateAll` on the same runtime. A nested scheduler call is rejected with `InvalidOperationException`; it does not start a nested tick. A handle also cannot be updated re-entrantly from another handle callback or from lifecycle callbacks while the runtime is finalizing the tick; the nested call is rejected so a sibling cannot run early, run twice, or run during teardown. Hosts that need another tick must request it after the current update call returns.

This rule avoids recursive scheduler traversal, duplicate processing of the same handle, and ambiguous ordering between nested and outer snapshots.

## 4. Boundary finalization and failures

Tick finalization runs from a `finally` path. Pending lifecycle work is processed before queued registrations are committed. Therefore a callback failure does not strand an otherwise valid registration request.

A registration queued by a lifecycle callback during finalization joins the same boundary commit. An explicit unregister requested during a tick retains its teardown semantics and cancels any earlier queued registration for the same identity. If the identity is unregistered and then registered later in the same tick, that later registration remains queued and becomes active at the boundary; removed handles are never silently resurrected. If registration is requested first and an explicit unregistration later, the unregister wins. Automatic threshold teardown is distinct: it removes the failing handles, then allows a queued replacement to register without reviving those handles.

Context invalidation remains an execution eligibility gate, not a destruction request. Explicit removal/unregistration and error-threshold teardown remain distinct lifecycle operations.

## 5. Observable guarantees and regression matrix

The implementation must preserve these invariants:

| Scenario | Required result |
| --- | --- |
| Replace from Enter | Current and sibling handles finish the tick against the old definition; replacement applies at boundary |
| Replace from Update | No sibling handle switches definition midway through the tick |
| Replace from transition condition | Transition evaluation for the current tick remains on the old definition |
| Replace from Exit/teardown | Request is queued until lifecycle callbacks complete |
| Multiple replacements for one ID | Last request in the tick wins |
| Callback throws after requesting replacement | Finalization still commits the queued replacement |
| Nested Update/UpdateAll | Rejected; no nested scheduler pass occurs |
| New handle created during callback | Not processed from the current tick snapshot |
| Current state absent after replacement | No invalid old-state Exit; recover through the replacement initial state lifecycle |
| Invalid context | Handle is suspended, not implicitly destroyed |

The NUnit suites `FSMRuntime_TickBoundary_Tests` and `FSM_API_DefinitionReplacement_TickBoundary_Tests` are the executable specifications for integer-backed and string-backed replacement, respectively. The broader lifecycle suite remains responsible for context ownership, explicit teardown, processing-group movement, and cross-representation observable parity.

## 6. Cross-representation parity status

Both the string-backed and integer-backed runtimes now defer callback-originated definition replacement until the current tick boundary. Each runtime rebinds all live handles together, preserves lifecycle state when the current state still exists, recovers through the replacement initial state when it does not, and suspends safely when no valid initial state exists.

The two implementations use different internal schedulers, so the regression suites remain representation-specific. The shared observable contract is the same: no handle selected for one tick can observe a replacement halfway through that tick, and nested scheduler or handle updates are rejected.

## 7. Non-goals and concurrency

This contract governs synchronous callback re-entrancy within one runtime instance. It does not make `FSMRuntime` thread-safe and does not authorize concurrent calls from multiple threads. Hosts must serialize runtime interaction unless a future explicit concurrency contract is designed and validated.

The scheduler does not attempt to make arbitrary mutation of a registered `FSM` object thread-safe. Definitions should be fully constructed before registration and treated as immutable while registered; replacement is the supported way to publish a new blueprint.
