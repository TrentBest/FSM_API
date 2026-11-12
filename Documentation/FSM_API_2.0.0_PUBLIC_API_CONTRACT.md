# FSM_API 2.0.0 — Public API and Documentation Contract

**Status:** target contract for 2.0.0. This document describes the intended public surface; it does not claim that the facade or namespace migration is implemented yet.

## 1. Consumer-facing entry point

The 2.0.0 public namespace is `FSM_API`. Consumers import it once:

```csharp
using FSM_API;
```

The public facade is `FsmApi`, with three deliberately named modules:

- `FsmApi.Create` — define FSMs, compile definitions, and create instances.
- `FsmApi.Interact` — update, inspect, transition, move, reset, and explicitly destroy live FSMs.
- `FsmApi.Error` — configure error policy and observe/report diagnostics.

The canonical creation call is:

```csharp
using FSM_API;

var builder = FsmApi.Create.CreateFiniteStateMachine(
    "Door",
    processRate: 1,
    processingGroup: "Update");
```

The desired public shape is intentionally not `FSM_API.Create...`. `FSM_API` is the namespace; `FsmApi` is the stable, readable facade type. The module names are nouns/verbs chosen to make call sites discoverable and consistent.

## 2. Module responsibilities

### Create

Owns the construction boundary:
- FSM definition builders and validation.
- Definition registration and lookup needed during creation.
- String-authored definition compilation.
- Deterministic name-to-ID mapping and reverse diagnostic mapping.
- Creation of live handles bound to caller-owned contexts.
- Explicit identity and compatibility validation.

Creation must not silently start host scheduling. The host remains responsible for deciding when interaction/update work occurs.

### Interact

Owns the runtime interaction boundary:
- Processing-group updates and manual stepping.
- State and transition inspection/evaluation.
- Explicit transition requests.
- Instance movement between compatible processing groups.
- Reset, pause/resume where supported, and explicit instance/definition destruction.
- Lifecycle and deferred-mutation behavior.

The same observable behavior must be available through both string-backed and integer-backed representations. Representation may change lookup/storage mechanics; it must not silently change callback order, transition precedence, entry timing, invalidation, exception handling, or destruction semantics.

### Error

Owns the diagnostic and error-policy boundary:
- Internal API diagnostics and subscriptions.
- Instance- and definition-level error accounting.
- Configurable thresholds and explicit shutdown policy.
- Error context sufficient to identify the definition, instance, state, processing group, and callback phase.
- Deterministic behavior when callbacks both invalidate/destroy an instance and throw.

An invalid context is an execution eligibility gate, not an implicit destruction request. Error policy may explicitly schedule shutdown; validity alone must not.

## 3. String and integer parity is a first-class release gate

2.0.0 is not complete merely because an integer-backed implementation exists. Every supported capability in the string API must have an equivalent integer-backed capability, or a documented intentional difference.

Parity evidence must compare observable traces/results, including:
- definition creation and validation;
- initial state and callback timing;
- ordinary and Any-State transition selection and precedence;
- Enter, Update, Exit ordering;
- manual, event-driven, and scheduled execution;
- process rates and processing groups;
- invalidation, suspension, resumption, and explicit destruction;
- mutation during callbacks and deferred operations;
- errors, exceptions, thresholds, and diagnostics;
- identity, lookup, and state-name reconstruction.

Tests should use equivalent inputs and assert equivalent traces. A test that merely proves both implementations do not throw is insufficient.

## 4. Identity contract

- Authored names remain available for readable construction, diagnostics, and tooling.
- Integer identifiers used for execution must be deterministic within their declared identity scope.
- `GetHashCode()` is not a persistent identity or cross-process identifier.
- The contract must distinguish runtime-local IDs from IDs intended for serialization, storage, or wire protocols.
- Forward and reverse mappings must be explicit, immutable to consumers, and versioned wherever persisted data depends on them.
- Integer hot paths must not repeatedly fall back to string hashing or lookup to simulate integer execution.

## 5. Documentation quality gate — Workshop standard

Existing documentation is treated as a substantial first draft, not as finished 2.0 documentation. The 2.0 documentation pass must raise it to a coherent, implementation-linked reference system:

1. **Conceptual model:** explain the domain, invariants, ownership, lifecycle, and why the architecture is shaped this way.
2. **Public API reference:** document every public type/member, parameters, defaults, exceptions, side effects, and examples.
3. **Implementation correspondence:** connect concepts to source files, tests, and executable examples. Claims must be traceable to implementation.
4. **Learning progression:** provide a deliberate path from first FSM to advanced lifecycle, identity, mutation, diagnostics, and performance.
5. **Operational truth:** explain host scheduling, thread-safety boundaries, deferred mutation, error policy, and failure modes without overstating guarantees.
6. **Visual explanation:** use purposeful diagrams and living SVGs that expose relationships and behavior, not decoration.
7. **Validation:** examples compile; links resolve; generated API docs match source; no stale 1.x claims are presented as 2.0 behavior.
8. **Maintenance:** documentation has an owner, update triggers, version scope, and a clear distinction between implemented, planned, and experimental behavior.

The standard is unusually high by design: documentation should function as a precise learning environment and an auditable companion to the implementation, not merely a collection of prose pages.

## 6. Progress accounting

The development graphic groups work by Create, Interact, and Error, while the overall percentage also accounts for cross-cutting parity, identity, verification, performance, documentation, and release governance.

A workstream earns completion only from committed implementation and relevant evidence. Documentation earns credit for accuracy, completeness, executable examples, source correspondence, and learning quality—not page count. Integer-backed parity earns credit only where the matching string capability is implemented and tested.

The overall score is a weighted engineering estimate, not code coverage or a release date. Reassess weights when the agreed 2.0.0 scope changes, record the reason, and update the README and SVG together. 100% means the release gates are evidenced and ready for owner review. It never authorizes publication by itself.

## 7. Compatibility and migration

The published 1.x package remains a separate compatibility line. Its namespace and call sites must not be silently rewritten in examples that claim to target 1.0.13. The 2.0 documentation must label version-specific examples clearly and provide a migration guide from the 1.x surface to:

```csharp
using FSM_API;

FsmApi.Create.CreateFiniteStateMachine();
FsmApi.Interact.Update("Update");
```

No package publication, merge to `master`, or release is authorized by this contract.
