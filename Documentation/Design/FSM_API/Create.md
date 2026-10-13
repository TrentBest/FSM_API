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

This complete example defines a two-state door. The application changes `OpenRequested`; the FSM evaluates that context value during updates and changes state when a transition condition becomes true.

```csharp
using TheSingularityWorkshop.FSM_API;

const string group = "Doors";
const string definitionName = "DoorFSM";

// Optional: FSM_API creates the group automatically when needed.
FSM_API.Create.CreateProcessingGroup(group);

// Define reusable behavior, then register it.
FSM_API.Create
    .CreateFiniteStateMachine(definitionName, processRate: 1, processingGroup: group)
    .State("Closed")
    .State("Open")
    .WithInitialState("Closed")
    .Transition("Closed", "Open", ctx => ((DoorContext)ctx).OpenRequested)
    .Transition("Open", "Closed", ctx => !((DoorContext)ctx).OpenRequested)
    .BuildDefinition();

// Create an instance bound to this door's application data.
var door = new DoorContext();
FSMHandle handle = FSM_API.Create.CreateInstance(definitionName, door, group);

// Your application controls the input and update cadence.
door.OpenRequested = true;
FSM_API.Interaction.Update(group);

// Context type can be declared after top-level statements.
public sealed class DoorContext : IStateContext
{
    public bool IsValid => true;
    public string Name { get; set; } = "FrontDoor";
    public bool OpenRequested { get; set; }
}
```

The builder creates the **definition**; `CreateInstance` creates the **context-bound instance**. The sample demonstrates the API shape; use the host application's actual input and update loop in production.

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
