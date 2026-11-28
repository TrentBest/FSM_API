# 06\. FSMModifier Deep Dive: Modifying Your FSMs at Runtime

> The `FSMModifier` provides advanced capabilities to **dynamically alter the blueprint (definition) of your Finite State Machines at runtime**. This allows for highly adaptive behaviors, hot-swappable logic, or configuration changes without rebuilding your application.

While the `FSMBuilder` is used to **initially design and build** an FSM's definition, the `FSMModifier` allows you to **change that definition after it has been created and registered** with `FSM_API`. This is a powerful feature for scenarios requiring:

  * **Adaptive AI:** Changing an enemy's behavior patterns on the fly (e.g., learning new attacks, adapting to player strategies).
  * **Dynamic UI:** Adding or removing navigation paths or UI states based on user unlocks or content downloads.
  * **Hot Reloading:** Modifying FSM logic in a live environment (e.g., development tools for rapid iteration).
  * **A/B Testing:** Dynamically switching between different FSM behaviors for experimentation.

-----

## 📚 Table of Contents

[00. Introduction to FSM_API](00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](01_Core_Concepts.md)

[03. Getting Started with C# ](03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](07_Error_Handling.md)

[08. Performance Tips & Best Practices](08_Performance_Tips.md)

[09. Common Use Cases & Examples](09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](11_FAQ.md)


-----

## 06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime

The FSMModifier provides advanced capabilities to dynamically alter the blueprint (definition) of your Finite State Machines at runtime. This allows for highly adaptive behaviors, hot-swappable logic, or configuration changes without rebuilding your application.

While the FSMBuilder is used to initially design and build an FSM's definition, the FSMModifier is the tool you use to change that definition after it has been created and registered.

This is a powerful feature for scenarios requiring:

A) Adaptive AI: Changing an enemy's behavior patterns on the fly (e.g., learning new attacks, adapting to player strategies).

B) Dynamic UI: Adding or removing navigation paths or UI states based on user unlocks or content downloads.

C)  Hot Reloading: Modifying FSM logic in a live environment (e.g., development tools for rapid iteration).

D)  A/B Testing: Dynamically switching between different FSM behaviors for experimentation.

## 🔑 Accessing and using FSMModifier

`FSMModifier` is a **public** type. Create it from an existing string-backed `FSM` definition, stage the changes you want, and call `ModifyDefinition()` to apply them.

The public `FsmApi.Interaction` module also provides convenience methods for common add/remove operations. Those wrappers apply their changes immediately through the modifier. For changing existing state actions or transition conditions, use `FSMModifier` directly.

## 🛠 Stage and apply definition changes

Retrieve the registered definition, then create a modifier:

```csharp
using System;
using TheSingularityWorkshop.FSM_API;

var definition = FsmApi.Interaction.GetFSMDefinition("PlayerFSM", "Update");
var modifier = new FSMModifier(definition);

modifier
    .WithState(
        "SpecialAttack",
        onEnter: ctx => Console.WriteLine("Prepare special attack!"),
        onUpdate: ctx => Console.WriteLine("Execute special attack."),
        onExit: ctx => Console.WriteLine("Finish special attack."))
    .WithModifiedState(
        "Idle",
        onEnter: ctx => Console.WriteLine("Entered the revised Idle state."),
        onUpdate: null,
        onExit: ctx => Console.WriteLine("Leaving revised Idle state."))
    .WithTransition(
        "Idle",
        "SpecialAttack",
        ctx => ctx is PlayerContext player && player.HasSpecialAttack)
    .WithModifiedTransition(
        "Damaged",
        "CriticallyInjured",
        ctx => ctx is PlayerContext player && player.Health <= 10)
    .WithoutTransition("Running", "Jumping")
    .WithoutState("Dead", "Idle")
    .WithProcessRate(2)
    .ModifyDefinition();

public sealed class PlayerContext : IStateContext
{
    public string Name { get; set; } = "Player";
    public bool IsValid { get; set; } = true;
    public bool HasSpecialAttack { get; set; }
    public int Health { get; set; } = 100;
}
```

The available staging methods are:

| Method | Purpose |
|---|---|
| `WithState` | Add a state if it does not already exist |
| `WithModifiedState` | Replace the actions on an existing state |
| `WithTransition` | Add a transition if that endpoint pair does not already exist |
| `WithModifiedTransition` | Replace the condition on an existing transition |
| `WithoutState` | Remove a state and provide a preferred fallback |
| `WithoutTransition` | Remove a transition by source and destination |
| `WithProcessGroup` | Move the definition and its instances to another group |
| `WithProcessRate` | Change the definition's processing rate |
| `ModifyDefinition` | Apply the staged operations |

The application order is sequential: process-group/rate changes, state removals, state additions, state modifications, transition removals, transition additions, then transition-condition modifications. This is **not an atomic transaction**. Do not assume all-or-nothing rollback if an operation or callback fails.

When removing a state, active handles in that state are moved to the requested fallback when it exists and is not the removed state. If the fallback is blank, missing, or invalid, the FSM's initial state is used. Design that fallback deliberately.

## Convenience methods on FsmApi.Interaction

For common add/remove operations, the facade offers direct methods:

```csharp
FsmApi.Interaction.AddStateToFSM(
    "PlayerFSM",
    "SpecialAttack",
    onEnter: ctx => Console.WriteLine("Prepare special attack!"),
    onUpdate: ctx => Console.WriteLine("Execute special attack."),
    onExit: ctx => Console.WriteLine("Finish special attack."));

FsmApi.Interaction.AddTransition(
    "PlayerFSM",
    "Idle",
    "SpecialAttack",
    ctx => ctx is PlayerContext player && player.HasSpecialAttack);

FsmApi.Interaction.RemoveTransition("PlayerFSM", "Running", "Jumping");
FsmApi.Interaction.RemoveStateFromFSM("PlayerFSM", "Dead", "Idle");
```

These wrappers cover adding/removing states and transitions. They do **not** expose `ModifyState` or `ModifyTransition` methods; use `FSMModifier.WithModifiedState` and `FSMModifier.WithModifiedTransition` to change existing definitions.

## 🔄 Manipulating live FSM instances (FSMHandle)

Definition changes affect the shared blueprint. A live `FSMHandle` also exposes instance-level controls that do not change the definition:

- `handle.TransitionTo("StateName")` forces that instance to a target state, bypassing transition conditions.
- `handle.ResetFSMInstance()` returns the instance to its initial state.
- `handle.EvaluateConditions()` explicitly evaluates transition conditions for that instance.
- `FsmApi.Interaction.DestroyInstance(handle)` explicitly destroys the live instance. Destruction is an interaction operation, not a `DestroyHandle()` method on the handle.

Use instance controls deliberately: they change the lifecycle of one running instance, while `FSMModifier` changes the shared definition used by all instances.
## 💡 Important considerations and best practices

- **Shared definition:** A modifier changes the blueprint, so every active instance created from that definition observes the updated definition on subsequent processing.
- **Sequential application:** The staged operations are applied in order, not as an atomic transaction. Do not assume rollback if an operation or callback fails.
- **State removal:** A valid supplied fallback is preferred; otherwise the FSM's initial state is used. Keep application-level cleanup and recovery behavior explicit.
- **Review before applying:** Validate state names, transition endpoints, processing-group changes, and callbacks before calling `ModifyDefinition()`.

-----

[➡️ Continue to: 07. Robust Error Handling: Cascading Degradation System](07_Error_Handling.md)