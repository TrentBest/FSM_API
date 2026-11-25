# FSM_API theory — behavior as an explicit runtime model

**Scope:** FSM_API 2.0 development line. This document describes the candidate architecture, not the published 1.0.13 package.

## 1. The central separation

FSM_API separates *what behavior means* from *who owns the data* and *who advances time*.

- **Definition (blueprint):** states, transition conditions, callback delegates, initial-state identity, and scheduling metadata.
- **Context (application data):** the caller-owned object implementing `IStateContext`. The runtime retains its reference; it does not copy the domain object into an FSM-owned data model.
- **Handle (live instance):** the runtime object that binds one definition to one context and tracks current state, lifecycle flags, registration, and error state.
- **Host (clock and owner):** the application that decides when to call Update and remains responsible for its own data, threading, I/O, and main loop.

This is a deliberate ownership boundary. A definition can be reused by many handles; each handle can act on a different context; and the host remains in control of when behavior advances.

## 2. State is a description; a handle is an execution

A definition is not itself an executing entity. It describes the legal state space and the callbacks that implement behavior. A handle is the live execution bound to a specific context.

The first eligible update enters the initial state and then runs that state's update callback. When a transition condition succeeds, the current state's exit callback runs and the handle's current-state identity changes. The destination state's enter callback is deferred until the next eligible update. This one-step boundary makes callback order observable and testable.

A transition is therefore not merely an assignment. It is a lifecycle protocol:

1. evaluate the current state's update behavior;
2. evaluate applicable transition conditions in the defined precedence order;
3. run Exit for the state being left;
4. change the current-state identity;
5. run Enter for the destination on the next eligible update.

Any-State transitions have defined precedence over state-specific transitions. That precedence must remain the same regardless of whether state identity is represented by a string or an integer.

## 3. Context validity is an execution gate

The context belongs to the caller. A handle retains that exact reference. Successful handle construction initializes `IsValid` to true; the caller may subsequently set it false.

An invalid context suspends callback execution. It does **not** implicitly destroy the handle, unregister its definition, or discard its current lifecycle position. When validity returns, execution resumes without replaying Enter for a state that was already entered.

This distinction is foundational: *not currently eligible to execute* is not the same thing as *no longer exists*. Explicit destruction and error-threshold teardown are separate lifecycle operations.

## 4. A host update is a consistency boundary

The host owns the clock. Calling Update advances the selected processing group; FSM_API does not start a background loop.

A scheduled update is a logical tick. The runtime selects eligible definitions and handles for that tick, then keeps the selected work on a stable definition view. Scheduling rates have the same meaning in both representations: `-1` processes on every host update, `0` is manual/event-driven, and a positive N processes every Nth host update. Values below `-1` normalize to `0` with a diagnostic; they must not accidentally become every-tick execution. A definition replacement requested from Enter, Update, a transition condition, Exit, or teardown is queued until the tick boundary. The last replacement requested for an identity during the tick wins.

This prevents a sibling handle from seeing a new blueprint halfway through a tick while another sibling is still executing the old one. It also makes failures manageable: finalization runs through a `finally` path, pending lifecycle work is processed, and queued registrations are then committed.

Nested scheduler updates and re-entrant handle updates are rejected. A callback that needs another tick must request it from its host after the current Update call returns. These rules govern synchronous callback re-entrancy; they do not make the runtime thread-safe. Hosts must serialize runtime interaction.

See [the runtime tick and mutation contract](FSM_RUNTIME_TICK_AND_MUTATION_CONTRACT.md) for the normative operation ordering and regression matrix.

## 5. Two representations, a shared behavioral target

The string-backed and integer-backed runtimes are separate execution paths with independent registries. The integer runtime is not a string lookup hidden behind an integer-shaped API. Explicit conversion belongs at the Transfer boundary.

| Concern | String-backed | Integer-backed |
| --- | --- | --- |
| Definition identity | Name within a processing group | Integer FSM ID |
| State identity | State name | Integer state ID |
| Handle | `FSMHandle` | `FSMHandleInt` |
| Processing group | String | Integer ID |
| Runtime registry | String runtime | Integer runtime |
| Update | Named group | Integer group ID |

Integer-backed direct-indexed storage currently supports FSM and state IDs from 0 through 1,048,575 inclusive. Out-of-range IDs must be rejected before capacity growth. Sparse IDs inside that range are supported; the limit is a resource boundary, not a recommendation to allocate every slot.

The current `FSMCompiler` assigns dense state IDs in ordinal state-name order. That mapping is deterministic for an unchanged state-name set, not stable across arbitrary edits: adding or removing a state can renumber existing states. Persisted or network-visible state identity therefore requires a preserved/versioned mapping or an explicit stable-ID policy. FSM IDs are caller-assigned; processing-group IDs are representation-specific metadata.

Direct indexing makes individual definition/state resolution array-based, but it does not make the entire scheduler O(1). The current integer group update scans the definition array and then scans the tick's handle snapshot for each eligible definition. Scheduler cost therefore depends on definition capacity and handle population as well as the number of handles actually updated. The paired performance matrix must measure these costs separately from narrow lookup benchmarks.

For the implementation-level examples and explicit string-to-integer boundary, see [String and integer backing in FSM_API 2.0](User%20Guide/12_String_and_Integer_Backing.md).

The unified `FSM_API.FsmApi.Interact` facade also exposes representation-matched per-handle operations: `UpdateInstance`, `EvaluateConditions`, `TransitionTo`, and `ResetInstance`. The goal is not identical internal code. The goal is equivalent observable behavior: matching callback traces, transition selection, scheduling cadence, invalidation/resumption, mutation boundaries, destruction, and diagnostic outcomes for equivalent inputs. Tests must compare results and traces rather than merely asserting that both paths execute without throwing.

The two representations expose compatible per-handle movement with representation-appropriate identities. The string runtime moves a handle to the compatible definition with the same name in another processing group; the integer runtime moves a handle to a registered target FSM ID whose definition contains the handle's current state. Integer moves requested inside callbacks are committed only after the entire update tick, preventing a handle from being processed twice when the destination definition has not yet been visited. The move preserves the caller-owned context, current state, and state-entry bookkeeping. This closes the previously identified group-movement surface gap; broader parity still requires comparison of diagnostics, thresholds, exceptions, and scheduling edge cases.

## 6. Error handling is lifecycle policy

An exception in user callback code is a runtime event that must be attributed to the affected handle and definition. Instance thresholds determine when a failing handle is scheduled for removal; definition thresholds determine when repeated failing instances cause definition-level teardown.

Error events, threshold counters, cleanup, and deferred destruction are part of the public behavioral contract. They must be equivalent across representations or any intentional difference must be explicit. In particular, callback failures during teardown, multiple failing handles, repeated threshold crossings, and diagnostic subscribers that themselves throw require deliberate tests.

The current integration candidate has a direct cross-runtime threshold regression, but multi-handle threshold interactions and diagnostic-event semantics remain review items. Do not describe error parity as complete until those cases are evidenced.

## 7. Identity and persistence are different problems

Runtime identity is useful for efficient lookup and execution. It is not automatically a persistence or wire-format identity. A process-local integer, an authored name, and a stable serialized identifier have different lifetimes and compatibility requirements.

Never use `GetHashCode()` as a persistent identifier. Any mapping intended to survive process restarts, package versions, or network boundaries must be explicit, deterministic, reversible where needed, and versioned. The runtime's integer storage index should not silently become a serialization contract.

## 8. Evidence, not adjectives

A claim is complete only when its implementation and evidence agree:

- implementation establishes the behavior;
- tests compare the expected observable contract;
- documentation explains ownership, ordering, limits, and failure modes;
- visuals make those relationships easier to inspect without inventing behavior;
- CI validates the relevant source, tests, links, and generated documentation.

The 2.0 candidate remains unreleased. A passing build, a percentage estimate, or a generated package artifact is not publication authorization. The published 1.x package remains a separate compatibility line until an explicitly authorized release.

## Implementation map

- Public facade: `FSM_API/FsmApi.Namespace.cs`
- Established string-backed facade: `FSM_API/FSM_API.Create.cs`, `FSM_API/FSM_API.Interaction.cs`, and `FSM_API/FSM_API.Error.cs`
- Integer-backed runtime: `FSM_API/IntegerBacked/FSMRuntime.cs`
- Integer-backed handle and definition: `FSM_API/IntegerBacked/FSMHandle.cs`, `FSM_API/IntegerBacked/FSM.cs`
- Shared tick contract: [FSM_RUNTIME_TICK_AND_MUTATION_CONTRACT.md](FSM_RUNTIME_TICK_AND_MUTATION_CONTRACT.md)
- Public surface and representation boundary: [FSM_API_2.0.0_PUBLIC_API_CONTRACT.md](FSM_API_2.0.0_PUBLIC_API_CONTRACT.md)
- Readiness and remaining gates: [FSM_API_2.0.0_READINESS.md](FSM_API_2.0.0_READINESS.md)
