![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# String and integer backing in FSM_API 2.0

FSM_API supports two distinct representations. Choose the one that best fits the boundary you are working at; neither representation is mandatory for every application.

## Choose a backing

| Concern | String-backed | Integer-backed |
|---|---|---|
| Identity | Meaningful names for FSMs, states, and processing groups | Integer IDs for FSMs, states, and processing groups |
| Best fit | Authoring, readable domain logic, user-authored workflows, diagnostics | Runtime execution where integer identity and direct-indexed storage are useful |
| State names | Kept as part of the definition | Supplied through an explicit mapping when translating for diagnostics or authoring |
| Registry | Independent string-backed registry | Independent integer-backed registry |
| Conversion | Explicitly compile to integer IDs | Explicitly reconstruct names using a caller-provided mapping |

The two backings are separate. Passing a string identity does not implicitly look up an integer identity, and converting a definition does not move live handles or merge the two registries.

## Build a named definition

```csharp
using TheSingularityWorkshop.FSM_API;

var namedDefinition = FsmApi.Create
    .CreateFiniteStateMachine("Worker", processRate: -1, processingGroup: "Update")
    .State("Idle")
    .State("Working")
    .WithInitialState("Idle")
    .Transition("Idle", "Working", context => IsWorkAvailable(context))
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

Compilation assigns dense, deterministic state IDs based on ordinal state-name order and returns read-only mappings in both directions. The FSM ID and processing-group ID are provided by the caller. State actions and transition conditions are carried across; the transfer does not move existing runtime instances.

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
- **Identity mappings are explicit.** String-to-integer compilation creates a deterministic state mapping. Integer-to-string reconstruction requires the mapping and human-facing FSM/group names.
- **Processing groups are representation-specific.** An integer group ID does not intrinsically identify a string group name.
- **Behavior is preserved, not magically made equivalent.** Callbacks and transition predicates are retained, but their context assumptions remain the application author's responsibility.
- **Measure before claiming a speedup.** Use equivalent workloads and the FSM_API benchmark project to compare throughput, allocation, and scaling for the two backings.

## Related documentation

- [Getting started with C#](03_Getting_Started_CSharp.md)
- [Core concepts](01_Core_Concepts.md)
- [Performance tips](08_Performance_Tips.md)
- [FSM_API 2.0 readiness checklist](../FSM_API_2.0.0_READINESS.md)

---

<p align="center">
  <img src="../Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="180">
</p>
