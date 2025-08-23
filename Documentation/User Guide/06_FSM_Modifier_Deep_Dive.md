# 06\. FSMModifier Deep Dive: Modifying Your FSMs at Runtime

> The `FSMModifier` provides advanced capabilities to **dynamically alter the blueprint (definition) of your Finite State Machines at runtime**. This allows for highly adaptive behaviors, hot-swappable logic, or configuration changes without rebuilding your application.

While the `FSMBuilder` is used to **initially design and build** an FSM's definition, the `FSMModifier` allows you to **change that definition after it has been created and registered** with `FSM_API`. This is a powerful feature for scenarios requiring:

  * **Adaptive AI:** Changing an enemy's behavior patterns on the fly (e.g., learning new attacks, adapting to player strategies).
  * **Dynamic UI:** Adding or removing navigation paths or UI states based on user unlocks or content downloads.
  * **Hot Reloading:** Modifying FSM logic in a live environment (e.g., development tools for rapid iteration).
  * **A/B Testing:** Dynamically switching between different FSM behaviors for experimentation.

-----

## 📚 Table of Contents

[00. Introduction to FSM\_API](https://www.google.com/search?q=00_Introduction.md)

[01. Core Concepts: Your Guide to FSM\_API](https://www.google.com/search?q=01_Core_Concepts.md)

[02. Getting Started with Unity](https://www.google.com/search?q=02_Getting_Started_Unity.md)

[03. Getting Started with C\# (Non-Unity)](https://www.google.com/search?q=03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](https://www.google.com/search?q=04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](https://www.google.com/search?q=05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](https://www.google.com/search?q=06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](https://www.google.com/search?q=07_Error_Handling.md)

[08. Performance Tips & Best Practices](https://www.google.com/search?q=08_Performance_Tips.md)

[09. Common Use Cases & Examples](https://www.google.com/search?q=09_Common_Use_Cases.md)

[10. FSM\_API for Non-Coders: A Big Picture Overview](https://www.google.com/search?q=10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](https://www.google.com/search?q=11_FAQ.md)

-----

## 🔑 Accessing the `FSMModifier`

You access the `FSMModifier` through the `FSM_API.Modify` static property, providing the name of the FSM definition you wish to alter.

**C\# Example:**

```csharp
// Get the modifier for an existing FSM definition
var fsmModifier = FSM_API.Modify("MyPlayerFSM");
if (fsmModifier == null)
{
    Console.WriteLine("FSM 'MyPlayerFSM' not found for modification.");
    return;
}

// Now you can use fsmModifier to add/remove states/transitions
fsmModifier.AddState("NewSuperState", onEnter: ctx => Console.WriteLine("Entered new super state!"));
fsmModifier.AddTransition("ExistingState", "NewSuperState", ctx => /* condition */ true);
```

-----

## ⚠️ Important Considerations for Runtime Modification

  * **Affects Definition:** Changes made via `FSMModifier` directly alter the **FSM Definition (blueprint)**.
  * **Impact on Existing Instances:** Changes to an FSM definition *immediately affect all existing `FSMHandle` instances* that were created from that definition. This is a critical point to understand for debugging and predictability. If you remove a state that a live instance is currently in, that instance might transition to its initial state or an error state, depending on FSM\_API's internal handling of invalid current states (refer to FUBAR system).
  * **Thread Safety:** If you are modifying FSM definitions from multiple threads, ensure you have proper synchronization mechanisms in place to avoid race conditions.

-----

## 🛠 `FSMModifier` Methods: Altering the Blueprint

Here's a breakdown of the methods available on `FSMModifier` for changing your FSM definitions at runtime:

### 1\. `AddState()`: Introducing New States

Adds a new state to an existing FSM definition.

**C\# Example:**

```csharp
fsmModifier.AddState(
    "SpecialAttack",
    onEnter: ctx => ((CharacterContext)ctx).PrepareSpecialAttack(),
    onUpdate: ctx => ((CharacterContext)ctx).ExecuteSpecialAttack(),
    onExit: ctx => ((CharacterContext)ctx).CleanupSpecialAttack()
);
```

**Parameters:**

  * `stateName` (string): The unique name for the new state.
  * `onEnter`, `onUpdate`, `onExit` (Action\<IStateContext\>, optional): Delegates/lambdas defining the behavior for the new state, just like in `FSMBuilder.State()`.

### 2\. `RemoveState()`: Taking States Out

Removes a state and all transitions associated with it (both incoming and outgoing).

**C\# Example:**

```csharp
fsmModifier.RemoveState("DeadState");
```

**Parameters:**

  * `stateName` (string): The name of the state to remove.

**Caveat:** If an active `FSMHandle` instance is currently in the state being removed, it will become an "invalid state" (effectively a null state). `FSM_API` will attempt to transition it to its initial state upon the next update cycle, or the FUBAR system may flag it as unstable. Always consider the impact on live instances.

### 3\. `AddTransition()`: Creating New Paths

Adds a new transition rule between two existing states.

**C\# Example:**

```csharp
// Add a new transition from "Idle" to "Sleeping" if a condition is met
fsmModifier.AddTransition(
    "Idle",
    "Sleeping",
    ctx => ((CharacterContext)ctx).IsTired && Time.timeOfDay == Time.TimeOfDay.Night
);
```

**Parameters:**

  * `fromStateName` (string): The source state.
  * `toStateName` (string): The destination state.
  * `condition` (Func\<IStateContext, bool\>): The condition that must be true for the transition to occur.

### 4\. `RemoveTransition()`: Blocking Paths

Removes a specific transition rule.

**C\# Example:**

```csharp
// Remove the specific transition that allows moving from "Running" to "Jumping"
fsmModifier.RemoveTransition(
    "Running",
    "Jumping",
    ctx => ((CharacterContext)ctx).HasJumpInput
);
```

**Parameters:**

  * `fromStateName` (string): The source state of the transition to remove.
  * `toStateName` (string): The destination state of the transition to remove.
  * `condition` (Func\<IStateContext, bool\>): The *exact* condition delegate that was used when the transition was added. This requires you to keep a reference to the delegate if you want to remove it precisely.

### 5\. `AddAnyTransition()`: Adding Global Rules

Adds a new "Any" transition, which can occur from any state.

**C\# Example:**

```csharp
// Add a global transition to "PauseMenu" state if Escape key is pressed
fsmModifier.AddAnyTransition(
    "PauseMenu",
    ctx => ((GameContext)ctx).Input.GetKeyDown(KeyCode.Escape)
);
```

**Parameters:**

  * `toStateName` (string): The destination state.
  * `condition` (Func\<IStateContext, bool\>): The condition for this global transition.

### 6\. `RemoveAnyTransition()`: Removing Global Rules

Removes a specific "Any" transition.

**C\# Example:**

```csharp
// Assuming you stored the condition delegate when adding
Func<IStateContext, bool> globalExitCondition = ctx => ((GameContext)ctx).GameIsOver;
fsmModifier.RemoveAnyTransition(
    "ExitGame",
    globalExitCondition
);
```

**Parameters:**

  * `toStateName` (string): The destination state of the "Any" transition to remove.
  * `condition` (Func\<IStateContext, bool\>): The *exact* condition delegate that was used when the "Any" transition was added.

### 7\. `ChangeInitialState()`: Adjusting the Start Point

Changes the default initial state for **new instances** created from this FSM definition. This does *not* affect existing instances.

**C\# Example:**

```csharp
fsmModifier.ChangeInitialState("DebugStartingState");
```

**Parameters:**

  * `newInitialStateName` (string): The name of the state that should now be the initial state. This state must already exist in the FSM definition.

-----

## 🔄 Manipulating Live FSM Instances (`FSM_API.Interaction`)

While `FSMModifier` changes the FSM blueprint, `FSM_API.Interaction` provides methods to directly influence **individual, live `FSMHandle` instances** at runtime. These operations do not alter the underlying FSM definition.

### 1\. `GoToState()`: Force a State Change

Forces an `FSMHandle` instance to immediately transition to a specified state, bypassing any conditions.

**C\# Example:**

```csharp
// Assume 'playerFSM' is an active FSMHandle
FSM_API.Interaction.GoToState(playerFSM, "Dodge");
```

**Parameters:**

  * `fsmHandle` (FSMHandle): The specific FSM instance to modify.
  * `stateName` (string): The name of the state to transition to. This state **must exist** in the FSM's definition.

### 2\. `SetProcessRate()`: Adjust Update Frequency

Changes the update frequency (`onUpdate` calls and transition checks) for a specific `FSMHandle` instance. This overrides the `processRate` set in the FSM definition for *this instance only*.

**C\# Example:**

```csharp
// Make this enemy's FSM update less frequently to save performance
FSM_API.Interaction.SetProcessRate(enemyFSMHandle, 5);
```

**Parameters:**

  * `fsmHandle` (FSMHandle): The specific FSM instance.
  * `newProcessRate` (int): The new update frequency (0 for manual, 1 for every tick, N for every N ticks).

### 3\. `SetProcessingGroup()`: Reassign Update Group

Changes the processing group for a specific `FSMHandle` instance. This moves the instance from its current processing group to a new one, affecting when `FSM_API.Update("GroupName")` will process it.

**C\# Example:**

```csharp
// Move this UI element's FSM to a dedicated UI group
FSM_API.Interaction.SetProcessingGroup(uiElementFSMHandle, "UIGroup");
```

**Parameters:**

  * `fsmHandle` (FSMHandle): The specific FSM instance.
  * `newGroupName` (string): The name of the new processing group. If the group doesn't exist, it will be created.

-----

## 💡 Best Practices and Potential Pitfalls

  * **Plan Dynamic Changes:** While powerful, runtime modification can lead to complex debugging if not used thoughtfully. Plan your dynamic changes carefully.
  * **Understand Implications:** Always consider how changes to an FSM definition will affect existing `FSMHandle` instances.
  * **Error Handling:** FSM\_API's Cascading Degradation System (FUBAR) is designed to handle errors, but it's always better to avoid issues through careful design and defensive coding (e.g., checking if states exist before adding transitions to them).
  * **Performance:** While these operations are efficient, frequent or very large-scale modifications might have performance implications. Profile if necessary.
  * **State Reference:** When removing transitions, remember you need to pass the *exact* condition delegate you used to add it. This means you might need to store references to your delegates if you plan to remove them later.

By mastering `FSMModifier` and `FSM_API.Interaction`, you gain unparalleled control over your FSMs, enabling highly dynamic and responsive applications.

-----

[➡️ Continue to: 07. Robust Error Handling: Cascading Degradation System](https://www.google.com/search?q=07_Error_Handling.md)