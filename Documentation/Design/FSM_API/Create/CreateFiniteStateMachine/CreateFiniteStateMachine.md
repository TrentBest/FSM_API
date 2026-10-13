# CreateFiniteStateMachine: describe reusable behavior

`CreateFiniteStateMachine` is the starting point for a definition. It returns an `FSMBuilder`; it does not, by itself, create a context-bound runtime instance.

<p align="center">
  <a href="../../Visuals/Create_Journey.svg">
    <img src="../../Visuals/Create_Journey.svg" alt="Animated workflow from FSM builder to registered definition and context-bound runtime instance" width="1000" />
  </a>
</p>

## Signature

```csharp
public static FSMBuilder CreateFiniteStateMachine(
    string fsmName = "UnNamedFSM",
    int processRate = 0,
    string processingGroup = "Update")
```

### Parameters

- **`fsmName`** — the name used to identify this definition when building it and creating instances. Must not be null, empty, or whitespace.
- **`processRate`** — the default processing mode/rate for instances of the definition:
  - `0`: event-driven; it is not processed by the ordinary tick loop.
  - A positive value: tick/scheduled processing according to FSM_API's processing-rate rules.
  - `-1`: manual processing; invoke processing explicitly.
  - A value below `-1` is reported through the API error path and coerced to `0`.
- **`processingGroup`** — the case-sensitive group name associated with the definition. Defaults to `"Update"`. The group is created when needed.

### Returns

An `FSMBuilder` used to describe the definition's states, transition conditions, state actions, and initial state. Finish the definition with `BuildDefinition()` before asking `CreateInstance` to create a runtime instance.

## Example

The exact state-builder API is demonstrated in the [Getting Started with C# guide](../../../../User%20Guide/03_Getting_Started_CSharp.md). At a high level, the flow is:

```csharp
const string group = "PlayerSession";
const string name = "GameStateManager";

var builder = FSM_API.Create.CreateFiniteStateMachine(
    name,
    processRate: 1,
    processingGroup: group);

// Describe the states, transitions, actions, and initial state here.
// Use the fluent methods shown in the getting-started guide.

builder.BuildDefinition();

// A runtime instance is a separate operation:
var handle = FSM_API.Create.CreateInstance(name, playerContext, group);
```

The middle section is intentionally a placeholder: a useful definition must have its state structure and initial state configured before it is built.

## Design principle: definition first, instance second

The definition is the reusable behavior plan. Each call to `CreateInstance` binds that plan to a particular `IStateContext` and produces an `FSMHandle`. This separation lets many objects share one behavior definition while maintaining independent runtime state.

If the requested name already exists in the selected group, the current implementation returns a builder initialized from the existing definition path; treat this API as both a creation entry point and a route for definition modification. Review the implementation and tests when changing an existing definition at runtime.

## Related reading

- [Create workflow overview](../../Create.md)
- [CreateInstance](../CreateInstance/CreateInstance.md)
- [What Are FSMs? — theory](../../../../User%20Guide/Theory/01_What_Are_FSMs.md)
