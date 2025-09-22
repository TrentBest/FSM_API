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

[03. Getting Started with C# (Non-Unity)](03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](07_Error_Handling.md)

[08. Performance Tips & Best Practices](08_Performance_Tips.md)

[09. Common Use Cases & Examples](09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](11_FAQ.md)


-----

06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime

The FSMModifier provides advanced capabilities to dynamically alter the blueprint (definition) of your Finite State Machines at runtime. This allows for highly adaptive behaviors, hot-swappable logic, or configuration changes without rebuilding your application.

While the FSMBuilder is used to initially design and build an FSM's definition, the FSMModifier is the tool you use to change that definition after it has been created and registered.

This is a powerful feature for scenarios requiring:

    Adaptive AI: Changing an enemy's behavior patterns on the fly (e.g., learning new attacks, adapting to player strategies).

    Dynamic UI: Adding or removing navigation paths or UI states based on user unlocks or content downloads.

    Hot Reloading: Modifying FSM logic in a live environment (e.g., development tools for rapid iteration).

    A/B Testing: Dynamically switching between different FSM behaviors for experimentation.

🔑 Accessing and Using the FSMModifier

The FSMModifier is an internal class, so you can't create it directly. Instead, you access its functionality through a set of public, static helper methods on FSM_API.Interaction. These methods are designed to simplify common modification tasks like adding or removing states and transitions.

The FSMModifier uses a fluent API pattern where you first stage the changes you want to make and then apply them with a single ModifyDefinition() call. All changes are batched and applied sequentially when ModifyDefinition() is called, making the process safe and predictable.

🛠 FSMModifier Methods: Altering the Blueprint

All the methods below are called on an instance of the FSMModifier class. You get this instance by calling a public method on FSM_API.Interaction.

Staging Changes with With... and Without...

WithState(): Adding a New State

This method adds a new state to an existing FSM definition. If a state with the given name already exists, the operation is ignored. Use WithModifiedState() to change an existing state.

C# Example:
```csharp

FSM_API.Interaction.AddStateToFSM(
    "PlayerFSM",
    "SpecialAttack",
    onEnter: ctx => Console.WriteLine("Prepare special attack!"),
    onUpdate: ctx => Console.WriteLine("Execute special attack."),
    onExit: ctx => Console.WriteLine("Cleanup after special attack.")
);
```

WithModifiedState(): Updating an Existing State

This method modifies the actions (onEnter, onUpdate, and onExit) of a state that is already part of the FSM blueprint. If the state doesn't exist, the operation is ignored.

C# Example:
```csharp

// Change the "Idle" state's behavior
FSM_API.Interaction.ModifyState(
    "PlayerFSM",
    "Idle",
    onEnter: ctx => Console.WriteLine("Entered a NEW and improved Idle state."),
    onUpdate: null, // You can pass null to remove an action
    onExit: ctx => Console.WriteLine("Exiting new Idle state.")
);
```

WithoutState(): Removing a State

This method stages a state for removal from the FSM definition.

C# Example:
```csharp
// Remove a state and transition all instances to "InitialState"
FSM_API.Interaction.RemoveStateFromFSM("PlayerFSM", "Dead", null);
```

Important: When a state is removed, any active FSMHandle instances currently in that state will be immediately transitioned to a specified fallbackStateName. If you pass null or an empty string for the fallback, instances will automatically transition to the FSM's InitialState. The OnExit action of the removed state will be executed for these instances.

WithTransition(): Creating a New Path

This method adds a new transition rule between two states. If the transition already exists, the operation is ignored. Use WithModifiedTransition() to change an existing one.

C# Example:
```csharp
// Add a new transition from "Idle" to "Sleeping"
FSM_API.Interaction.AddTransition(
    "PlayerFSM",
    "Idle",
    "Sleeping",
    ctx => ((PlayerContext)ctx).IsTired() && DateTime.Now.Hour > 21
);
```
WithModifiedTransition(): Updating a Transition

This method updates the condition of an existing transition between two states. If the transition doesn't exist, the operation is ignored.

C# Example:
```csharp
// Change the condition for the "Damaged" -> "CriticallyInjured" transition
FSM_API.Interaction.ModifyTransition(
    "PlayerFSM",
    "Damaged",
    "CriticallyInjured",
    ctx => ((CharacterContext)ctx).Health <= 10 // Change from 25 to 10
);
```

WithoutTransition(): Blocking a Path

This method removes a specific transition rule. It requires the fromState and toState names to uniquely identify the transition.

C# Example:
```csharp
// Remove the transition from "Running" to "Jumping"
FSM_API.Interaction.RemoveTransition("PlayerFSM", "Running", "Jumping");
```
🔄 Manipulating Live FSM Instances (FSMHandle)

While FSMModifier changes the FSM blueprint, the FSMHandle provides methods to directly influence individual, live FSMHandle instances at runtime. These operations do not alter the underlying FSM definition.

    TransitionTo(): Forces an FSMHandle instance to immediately transition to a specified state, bypassing any conditions. This is useful for debug commands or for recovering from an error state.

    ResetFSMInstance(): Resets the FSM instance to its initial state, as defined in its blueprint.

    EvaluateConditions(): Manually checks all transition rules from the current state. This is especially useful for FSMs with a ProcessRate of 0, which do not update automatically.

    DestroyHandle(): Shuts down this FSM instance, removes it from the API's internal system, and calls the OnExit action of its current state. This is a crucial cleanup step for any live instance that is no longer needed.

💡 Important Considerations and Best Practices

    Affects All Instances: Remember that changes made with FSMModifier affect the shared blueprint. All active FSMHandle instances created from that blueprint will immediately use the new states, transitions, and properties on their next update cycle.

    Atomicity: The FSMModifier stages changes and applies them all at once when ModifyDefinition() is called. This is a robust design that prevents partial or inconsistent changes from corrupting the FSM blueprint.

    State Removal is Handled Gracefully: The API will automatically handle instances in a removed state by transitioning them to a fallback state, ensuring your application doesn't crash or get stuck in an invalid state.

-----

[➡️ Continue to: 07. Robust Error Handling: Cascading Degradation System](07_Error_Handling.md)