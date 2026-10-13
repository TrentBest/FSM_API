# Create: turn a behavior design into a running FSM

The `FSM_API.Create` methods form the entry point from an intended behavior to an executable state machine. The steps are deliberately separate: organize processing, describe a reusable definition, register that definition, then create an instance bound to application data.

<p align="center">
  <a href="Visuals/Create_Journey.svg">
    <img src="Visuals/Create_Journey.svg" alt="Animated FSM_API Create workflow: processing group, builder, registered definition, context-bound runtime instance, and application update cycle" width="1100" />
  </a>
</p>

*This is an explanatory animation, not a live runtime monitor. The actual implementation and API contracts are the authority.*

## The creation path

| Step | API | What exists afterward |
|---|---|---|
| 1. Organize | `CreateProcessingGroup(name)` | The named processing group exists. This step is optional because FSM_API creates a group when needed. |
| 2. Describe | `CreateFiniteStateMachine(name, processRate, processingGroup)` | An `FSMBuilder` lets you describe states, transitions, actions, and the initial state. |
| 3. Register | `BuildDefinition()` | The definition is available for instances to use. |
| 4. Instantiate | `CreateInstance(name, context, processingGroup)` | An `FSMHandle` connects the registered definition to one `IStateContext`. |
| 5. Run | `Interaction.Update(processingGroup)` or an appropriate explicit/manual trigger | The host application determines when the machine is processed. |

## A minimal C# example

This example focuses on the creation boundary. The builder calls used to define states and transitions are shown in the getting-started guide.

```csharp
using TheSingularityWorkshop.FSM_API;

public sealed class DoorContext : IStateContext
{
    public bool IsValid => true;
    public string Name { get; set; } = "FrontDoor";
    public bool IsOpen { get; set; }
}

const string group = "Doors";
const string definitionName = "DoorFSM";

// Optional: groups are also created automatically when needed.
FSM_API.Create.CreateProcessingGroup(group);

// Define the reusable behavior and register it.
FSM_API.Create
    .CreateFiniteStateMachine(definitionName, processRate: 1, processingGroup: group)
    // Add states, transitions, actions, and an initial state here.
    .BuildDefinition();

// Create an independent runtime instance for this context.
var door = new DoorContext();
FSMHandle handle = FSM_API.Create.CreateInstance(definitionName, door, group);

// Call from the application's own update loop when appropriate.
FSM_API.Interaction.Update(group);
```

> **Important:** The snippet's builder body is intentionally a placeholder; a definition must include its required states and initial state before it can represent useful behavior. For a complete compiling example, continue to [Getting Started with C#](../../User%20Guide/03_Getting_Started_CSharp.md).

## Four concepts to keep separate

- **Processing group — organization.** A named bucket for definitions and instances that are processed together when that group is updated. Group names are case-sensitive.
- **FSM definition — the recipe.** The reusable arrangement of states, transitions, and state actions. It is not itself one particular character, door, document, or session.
- **FSM instance / handle — the running machine.** An instance uses a definition while maintaining its own runtime state and binding to a context.
- **Context — the domain data.** Your application owns the object implementing `IStateContext`; the FSM operates against it rather than requiring the FSM library to own your domain model.

That separation is what lets one definition be reused for multiple independent objects without giving each object a separate hand-written state machine.

## Processing-rate note

The `processRate` argument configures how instances of a definition are processed. The current implementation documents `0` as event-driven, positive values as scheduled/tick-based processing, and `-1` as manual processing. Values below `-1` are reported and coerced to `0`. Consult the API XML documentation and tests when choosing a rate; don't infer wall-clock timing from the integer alone.

## Related documentation

- [CreateFiniteStateMachine — start a definition](Create/CreateFiniteStateMachine/CreateFiniteStateMachine.md)
- [CreateInstance — bind a definition to a context](Create/CreateInstance/CreateInstance.md)
- [CreateProcessingGroup — organize update work](CreateProcessGroup/CreateProcessGroup.md)
- [Getting Started with C#](../../User%20Guide/03_Getting_Started_CSharp.md)
- [Theory: What Are FSMs?](../../User%20Guide/Theory/01_What_Are_FSMs.md)
- [Theory: Types of FSMs](../../User%20Guide/Theory/02_Types_Of_FSMs.md)
