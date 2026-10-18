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


### Focused mutation-path finding (2026-10-18)

The direct creation path confirms that the shared list has multiple unsynchronized writers:

- Create.CreateInstance reads bucket.Instances.Count + 1, constructs a handle, then calls bucket.Instances.Add(handle) immediately.
- Internal.DestroyInstance calls bucket.Instances.Remove(handle) immediately.
- Internal.MoveInstance also removes/adds handles in those lists; other lifecycle operations clear lists or destroy whole buckets.
- FSMHandle does not override equality, so List<FSMHandle>.Remove uses ordinary reference equality in this type. The observed List<T>.RemoveAt out-of-range exception is therefore particularly consistent with a list being shortened concurrently after Remove found its index. This is a strong race hypothesis, not yet a proven reproduction.
- The Renderer coordinator's private _gate only serializes calls on that one coordinator. It does not protect the static FSM_API registry, nor does it coordinate separate coordinators or other consumers that share the Renderer.Representation bucket.

Next discriminating test: run the Renderer source-integration suite with test parallelization disabled, then enabled, and compare. Also add a focused FSM_API stress/regression test that concurrently creates and destroys handles in one shared bucket, if concurrent calls are intended to be supported. If the documented contract is main-thread-only, instead locate which test/runtime path violates that contract and align docs with the actual guarantee. Do not infer that IsValid deactivation is the cause; it does not mutate the instance list.

No runtime fix is made from this inspection alone. The correct fix depends on establishing whether concurrent registry access is supported and reproducing the interleaving; a lock around only DestroyInstance would leave the other list writers unsynchronized.


### Original failure vs successful rerun: lifecycle change identified (2026-10-18)

The exact source commits are now known from the Renderer Actions logs:

- Original failing source-integration job `117555449357` checked out FSM_API `da2e05ba41ff33aeb1429433b70593e271b1fd3b`.
- Successful rerun job `117593029298` checked out FSM_API `f5a89641eaca216f5466af59b2dc4a983fe9511c`.
- The successful source commit is 38 commits ahead of the failed source commit.

A relevant runtime change is present in `FSM_API.Internal.cs` within `ResetAPI`. At the failed commit, the non-hard-reset path automatically called `DestroyInstance(handle)` for invalid contexts. At the successful commit, that branch explicitly treats `IsValid == false` as deactivation: it continues without unregistering or destroying the handle. The successful version also adds a regression test for invalidating and then resuming the same handle. This is a materially relevant lifecycle change, not merely documentation drift.

This makes the user's hypothesis—that ongoing FSM_API work fixed the Renderer failure—a credible explanation. It is not yet a proven single-variable cause: the two tested commits differ by 38 commits and other changes are present, and the test did not provide a controlled before/after run isolating this change. The current `DestroyInstance` implementation remains a list removal with no visible registry-wide lock. Do not call the race hypothesis disproven, but update the investigation priority: first test the lifecycle change against the failure before assuming the fix must be a concurrency lock.

Evidence: [Renderer workflow and reruns](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875). No runtime change was made during this comparison.

### Renderer current-source verification (2026-10-18)

Renderer workflow run [39086967875](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875) has now completed successfully. Source-integration job `117594490093` checked out FSM_API `development` at `69949dc499c39f28d5b85d554d3f6006c7e9286d`; source restore and build succeeded and all 115 Renderer tests passed. Renderer’s independent build-and-test job also passed; NuGet publishing was skipped.

This confirms the current FSM_API/Renderer source combination passes the previously relevant suite. It does not establish a single-variable cause for the earlier failure or prove the registry is safe under arbitrary concurrent mutation. Keep the invalid-context suspend/resume contract and explicit destruction semantics intact. Do not add registry-wide locking solely from the earlier stack trace; first require a reproducible failure or a focused test justified by the documented concurrency contract.
