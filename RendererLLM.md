# Renderer LLM — FSM_API source integration finding

## 2026-10-18: DestroyInstance throws while disposing Renderer computation machine

Please investigate this failure in FSM_API `development`:

- Consumer: [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer)
- Failing source-integration run: [Renderer workflow 39086967875](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875)
- Test: `TheSingularityWorkshop.Renderer.Tests.ObserverRepresentationCoordinatorTests.Evaluate_InvalidatesSelectedBucketWhenObserverRevisionChanges`
- Exception: `System.ArgumentOutOfRangeException` — index out of range.
- Stack: `System.Collections.Generic.List<T>.RemoveAt` -> `List<T>.Remove(T item)` -> `TheSingularityWorkshop.FSM_API.FSM_API.Internal.DestroyInstance(FSMHandle handle)` at `FSM_API.Internal.cs:254` -> `FSM_API.Interaction.DestroyInstance` at `FSM_API.Interaction.cs:522` -> Renderer `RendererComputationMachine.Dispose()` -> `ObserverRepresentationCoordinator.Dispose()`.

The Renderer source restore and build passed; only its source-integration test step failed. Renderer creates one FSM instance per computation machine and calls `DestroyInstance` once from idempotent disposal. Please inspect the internal instance-list / handle lookup / removal invariant and determine why `List<T>.Remove` reaches `RemoveAt` with an invalid index. The problem may involve a stale handle, a mismatched equality/index lookup, or registry mutation, but those are hypotheses until source inspection confirms the cause.

Please reproduce against FSM_API's own tests and/or the Renderer source-integration suite. Fix and test the defect in FSM_API if confirmed. Avoid a workaround in Renderer that suppresses disposal errors or changes the rendering API.

Renderer-side coordination note: https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/RendererLLM.md


## 2026-10-18: Concurrency contract needs explicit verification

Further source inspection shows that `Internal.DestroyInstance` immediately executes `bucket.Instances.Remove(handle)`; it does not use the deferred-mutation queue. The README currently makes broader claims that structural modifications such as destroying an FSM are queued until after the tick, while the public `Interaction.DestroyInstance` directly calls the internal method. By contrast, `Interaction.MoveInstance` explicitly queues its mutation via `QueueDeferredModification`.

The exception occurring inside `List<T>.Remove` at `List<T>.RemoveAt` is consistent with the list being concurrently mutated between index lookup and removal; it is not, by itself, proof of the exact competing operation or proof that the caller violated the contract. Please inspect every path that writes to `FsmBucket.Instances` (creation, tick/error cleanup, destroy-definition, move, and explicit destroy), inspect the deferred queue implementation and tick boundary, and establish the actual concurrency guarantees. The README contract and implementation must agree.

Do not assume this can be fixed by adding a lock only to `DestroyInstance`: all readers and writers of the shared registry/list must follow one coherent synchronization or deferred-mutation policy. Add a deterministic regression test for the demonstrated cause and run the FSM_API suite plus Renderer source integration before marking verified.


### Tick boundary and deferred-queue findings (2026-10-18)

Further source inspection on `development` adds important constraints to the investigation:

- `Interaction.Update(processingGroup)` runs `Internal.TickAll(processingGroup)` and then `Internal.ProcessDeferredModifications()`. That is a per-call boundary, not evidence of global synchronization against calls arriving from other threads or other processing-group ticks.
- `Internal.QueueDeferredModification` enqueues into a plain `Queue<Action>`; `ProcessDeferredModifications` checks `Count`, dequeues, and invokes actions without visible synchronization. Other code paths enqueue directly through `Internal.GetDeferred()`.
- `Create.CreateInstance` inserts immediately into `bucket.Instances`; public `Interaction.DestroyInstance` immediately calls `Internal.DestroyInstance`. Public `Interaction.MoveInstance` queues its mutation.
- `Interaction.DestroyFiniteStateMachine` and `RemoveProcessingGroup` iterate instance lists, call `DestroyHandle`, clear lists, and remove definitions/groups immediately. `DestroyFiniteStateMachine` snapshots its instance list first, but that does not synchronize against concurrent writers.
- `GetInstances` returns a list snapshot, but obtains the bucket through the live dictionaries before copying. `GetAllFsmHandles` and `GetProcessingGroupNames` expose deferred enumerations over live collections. Counts and other registry reads also traverse the live dictionaries/lists.
- The API's comments describe deferred structural mutation as a safe update boundary, but the observed implementation has immediate structural paths and a non-thread-safe deferred queue. This is a contract/implementation discrepancy worth resolving before calling the API thread-safe.

These observations do not yet prove the precise race that produced Renderer run `39086967875`'s `List<T>.RemoveAt` failure. Do not apply a narrow lock or assert concurrent API calls are supported until the intended threading contract is established. Next inspect `TickAll`, handle self-unregistration, and the Renderer disposal call graph; then create a deterministic regression test for the confirmed interleaving.


### Tick snapshot and lifecycle observations (2026-10-18)

The focused `TickAll` read adds two specific caveats:

- `TickAll` snapshots the processing group's bucket values and then calls `bucket.Instances.ToList()` before iterating handles. Those copies avoid iterator invalidation from mutations that occur after a successful copy, but the copy operation itself is not synchronized against a concurrent list writer. A copied handle can also remain in the tick snapshot after another caller destroys it; there is no visible destroyed/liveness check before state callbacks run.
- `ResetAPI` and `DestroyFiniteStateMachine` directly walk the live registry/list structures, invoke `DestroyHandle`, and clear/remove entries. They are not routed through the same deferred queue as `MoveInstance`.

This narrows the places to instrument, but still does not prove whether the Renderer exception comes from overlapping threads, overlapping update/disposal, re-entrant lifecycle callbacks, or a violated caller-side lifetime contract. Establish the intended owner/thread and whether dispose can overlap update before changing semantics. Regression coverage should test the confirmed interleaving and assert that no callback runs after teardown.


### IsValid deactivation semantics cross-check (2026-10-18)

The current `development` source reflects the recent semantic change and this distinction must be preserved during the Renderer investigation:

- `FSMHandle.IsValid` is a computed view of `Context?.IsValid ?? false`; it is an execution-eligibility gate, not a registry-membership flag.
- `Internal.TickAll` checks `handle.Context.IsValid`. An invalid context is skipped and the handle remains in its bucket; restoring validity can resume that same registered handle.
- Explicit `Interaction.DestroyInstance` / `Internal.DestroyInstance` is the destructive path. `Internal.DestroyInstance` removes the handle from `bucket.Instances` and then calls `handle.DestroyHandle()`; teardown runs the current state's Exit and sets `Context.IsValid = false`.
- Therefore, deactivation via `IsValid = false` must not be interpreted by consumers as disposal, unregistration, or permission to discard the handle/context. Conversely, explicit destruction is not a reusable pause.

This changes the framing of the Renderer failure: the IsValid change itself should not remove an instance from the registry. The reported exception occurs on the explicit destruction path at `bucket.Instances.Remove(handle)`. Investigation should now prioritize Renderer disposal ownership/order, repeated or re-entrant destroy calls, and any concurrent registry writer. Do not revert deactivation semantics or use `IsValid` as a substitute for explicit destruction.

One contract caveat remains visible in source: `DestroyHandle()` is teardown and always writes `Context.IsValid = false`, while `FSMHandle.IsValid` has no independent destroyed flag. Registry membership is the actual distinction between registered and unregistered; callers should not infer membership from `IsValid` alone.
