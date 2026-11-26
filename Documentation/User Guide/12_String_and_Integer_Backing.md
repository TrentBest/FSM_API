![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# String and integer backing in FSM_API 2.0

FSM_API supports two distinct representations. Choose the one that best fits the boundary you are working at; neither representation is mandatory for every application.

> **Version note:** The unified `FsmApi` facade and definition-transfer API described here are part of the unreleased 2.0 development work. They are not a claim about the currently published NuGet package.

## Choose a backing

| Concern | String-backed | Integer-backed |
|---|---|---|
| Identity | Meaningful names for FSMs, states, and processing groups | Integer IDs for FSMs, states, and processing groups |
| Best fit | Authoring, readable domain logic, user-authored workflows, diagnostics | Runtime execution where integer identity and direct-indexed storage are useful |
| State names | Kept as part of the definition | Supplied through an explicit mapping when translating for diagnostics or authoring |
| Registry | Independent string-backed registry | Independent integer-backed registry |
| Conversion | Explicitly compile to integer IDs | Explicitly reconstruct names using a caller-provided mapping |

The two backings are separate. Passing a string identity does not implicitly look up an integer identity, and converting a definition does not move live handles or merge the two registries.

This is an intentional representation boundary, not an old/new compatibility ladder. The string-backed API remains a first-class option for readable, name-oriented workflows. The integer-backed API remains a first-class option for callers that already own integer identities and want direct-indexed definition/state storage. An application can choose either boundary; the runtime does not silently mix them.

## Three identity layers

Keep these identities distinct:

1. **Authored names** — FSM names, state names, and processing-group names used by the string-backed API.
2. **Compiled state IDs** — dense integer indexes assigned by `FSMCompiler` to the states in one compiled definition.
3. **Runtime definition/group IDs** — the caller-supplied integer FSM ID and processing-group ID used when registering and scheduling an integer definition.

The compiler sorts the source state's names using ordinal comparison and assigns dense IDs beginning at zero. That is deterministic for the same state-name set and compiler rules, but it is **not a durable identity guarantee across edits**: adding or removing a state can shift IDs assigned to other names. If IDs are persisted, replicated, or referenced by saved data, persist/version the name-to-ID mapping with the definition or supply an explicit stable mapping strategy. Do not assume a freshly compiled mapping is interchangeable with an older one.

The integer runtime uses direct-indexed arrays for FSM definitions, process counters, and states. FSM and state IDs are bounded by `FSM.MaximumSupportedArrayIndex` (1,048,575). Dense IDs are the normal compiled shape. Sparse IDs are accepted for compatibility, but capacity grows to accommodate the highest index, so a very large sparse ID can waste memory and make registration more expensive. Processing-group IDs are integer metadata; they do not carry a group name.

## What direct indexing does — and does not — promise

A successful integer definition lookup can use the FSM ID as an array index. A state lookup can use the state ID as an array index. Those are direct-indexed operations after the relevant arrays have been sized.

That does **not** make every integer-runtime operation constant-time. The current integer scheduler scans the definition array for a group tick and, for each eligible definition, scans the tick's handle snapshot to find matching handles. Inspection methods such as handle counting also scan live handles. Scheduler traversal, handle population, process rates, and allocations therefore remain separate performance dimensions. The paired benchmark matrix is designed to expose these costs rather than hiding them behind a single lookup number.

## The possible string-to-integer execution adapter

A future optimization could preserve the string-facing API while routing its identities through an integer execution kernel:

```text
String-facing API                         Native integer API
(names, diagnostics, authoring)            (integer IDs)
          │                                      │
          ▼                                      ▼
 collision-safe name registry              integer identity
          │                                      │
          └──────── assigned integer IDs ────────┘
                             │
                             ▼
                   shared integer execution
```

This is a **future architecture option**, not the current implementation. Today, the two runtime registries and execution paths are independent, and `FSMCompiler` transfers definitions explicitly. A future adapter must preserve the string contract, diagnostics, and observable behavior while keeping the integer runtime free of string lookups on its hot path.

Do not use `string.GetHashCode()` as a persistent FSM or state ID. Hash codes can collide and are not a durable cross-process identity. Use a collision-safe name-to-ID registry that assigns and retains explicit integer IDs, with a reverse mapping for diagnostics and authoring.

## Build a named definition

```csharp
using TheSingularityWorkshop.FSM_API;

var namedDefinition = FsmApi.Create
    .CreateFiniteStateMachine("Worker", processRate: -1, processingGroup: "Update")
    .State("Idle", onEnter: null, onUpdate: null, onExit: null)
    .State("Working", onEnter: null, onUpdate: null, onExit: null)
    .WithInitialState("Idle")
    .Transition("Idle", "Working", context => true)
    .BuildDefinition();
```

The string-backed form is convenient when state names communicate domain meaning.

## Transfer a string definition to integer backing

```csharp
var compiled = FsmApi.Transfer.ToInteger(
    namedDefinition,
    fsmID: 100,
    processingGroupID: 2);

var integerDefinition = compiled.Definition;
var stateNameToID = compiled.StateNameToID;
var stateIDToName = compiled.StateIDToName;

FsmApi.Create.RegisterDefinition(integerDefinition);
```

Compilation assigns dense state IDs in ordinal state-name order and returns read-only mappings in both directions. The mapping is repeatable for the same exact set of names, but adding or removing a state can renumber other states; it is not automatically a persistent ID scheme. The FSM ID and processing-group ID are provided by the caller. State actions and transition conditions are carried across; the transfer does not move existing runtime instances.

## Transfer an integer definition back to string backing

Integer definitions do not contain human-readable state names or processing-group names. Supply those names explicitly rather than relying on guessed names:

```csharp
var namedAgain = FsmApi.Transfer.ToString(
    integerDefinition,
    fsmName: "Worker",
    processingGroup: "Update",
    stateIDToName: stateIDToName);
```

The mapping must include every defined integer state ID and must use unique, non-empty names. The integer FSM's initial state and transitions must refer to defined states. The returned value is a string-backed definition; it does not automatically register it or transfer live instances.

## Transfer boundaries and limitations

- **Definitions transfer; live handles do not.** Recreate instances in the destination runtime when needed.
- **Identity mappings are explicit.** String-to-integer compilation creates a dense, compile-local state mapping. Preserve/version that mapping if IDs cross a persistence or network boundary; integer-to-string reconstruction requires the mapping and human-facing FSM/group names.
- **Processing groups are representation-specific.** An integer group ID does not intrinsically identify a string group name.
- **Behavior is preserved, not magically made equivalent.** Callbacks and transition predicates are retained, but their context assumptions remain the application author's responsibility.
- **Measure before claiming a speedup.** Use equivalent workloads and the paired FSM_API 2.0 benchmark project to compare throughput, allocation, and scaling for the two backings. The current paired suite has smoke-test coverage but no calibrated result yet.

## Related documentation

- [Getting started with C#](03_Getting_Started_CSharp.md)
- [Core concepts](01_Core_Concepts.md)
- [Performance tips](08_Performance_Tips.md)
- [FSM_API 2.0 readiness checklist](../FSM_API_2.0.0_READINESS.md)

---

<p align="center">
  <img src="../Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="180">
</p>
