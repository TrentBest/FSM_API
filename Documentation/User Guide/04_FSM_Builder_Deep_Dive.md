# 04. FSMBuilder Deep Dive: Building Your FSMs

> The `FSMBuilder` is your primary tool for **designing and configuring the blueprint** for your Finite State Machines. It provides a fluent, step-by-step approach to define complex state behaviors and transitions.

Think of the `FSMBuilder` as a specialized workshop where you craft the detailed plans for how your FSM will operate. You'll use it to:

* **Name** your FSM blueprint.
* Define all its possible **states** and what happens when entering, updating, or exiting them.
* Establish the **transitions** (rules) for moving between these states.
* Set its **initial state** where instances will begin.
* Configure its **default update frequency** and **processing group**.

Once built, this **FSM Definition** can be used to create many live FSM instances.

---

## 📚 Table of Contents

[00. Introduction to FSM_API](User%20Guide/00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](User%20Guide/01_Core_Concepts.md)

[03. Getting Started with C# (Non-Unity)](User%20Guide/03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](User%20Guide/04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](User%20Guide/05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](User%20Guide/06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](User%20Guide/07_Error_Handling.md)

[08. Performance Tips & Best Practices](User%20Guide/08_Performance_Tips.md)

[09. Common Use Cases & Examples](User%20Guide/09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](User%20Guide/10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](User%20Guide/11_FAQ.md)


---

## ✨ What is a Fluent API?

The `FSMBuilder` is designed with a **fluent API** pattern, meaning most of its methods return the builder instance itself. This allows you to **chain method calls** together, creating highly readable and declarative code that describes your FSM step-by-step.

**C# Example:**
```csharp
FSM_API.CreateFiniteStateMachine("MyPlayerFSM")
    .State("Idle", onUpdate: ctx => Console.WriteLine("Player is idling..."))
    .State("Walking", onUpdate: ctx => Console.WriteLine("Player is walking..."))
    .WithInitialState("Idle")
    .Transition("Idle", "Walking", ctx => /* player moves */ true)
    .Transition("Walking", "Idle", ctx => /* player stops */ false)
    .BuildDefinition();
```

Each call adds to the FSM's structure in a logical, flowing manner.

-----

## 🏁 Starting the Build: `FSM_API.CreateFiniteStateMachine()`

You begin defining any FSM blueprint by calling `FSM_API.CreateFiniteStateMachine()`. This factory method returns an `FSMBuilder` instance, ready for your configuration.

**C\# Example:**

```csharp
var fsmBuilder = FSM_API.CreateFiniteStateMachine(
    fsmName: "EnemyAI",
    processRate: 1, // Optional: default update frequency for instances of this FSM
    processingGroup: "AI" // Optional: default processing group for instances
);
```

### Parameters:

  * `fsmName` (string): **Required.** A **unique identifier** for this FSM definition. This is how you'll retrieve and create instances of this specific FSM later. If an FSM with this name already exists, the builder will allow you to modify its existing definition.
  * `processRate` (int, optional): Sets the **default frequency** at which instances of this FSM definition will `onUpdate` their current state and check for transitions.
      * `processRate = 0`: Updates **only when explicitly triggered** (`FSM_API.Step()`). Ideal for event-driven FSMs (e.g., UI elements that only react to clicks).
      * `processRate = 1`: Updates **every tick** of its processing group.
      * `processRate = N` (where N \> 1): Updates **every N ticks** of its processing group. (e.g., `processRate = 2` means it updates every other tick).
      * Default: `1`
  * `processingGroup` (string, optional): Assigns a **default processing group** name for instances created from this definition. Processing groups (e.g., "Update", "FixedUpdate", "AI\_Logic") allow you to organize and update multiple FSMs together from a single `FSM_API.Update("GroupName")` call.
      * Default: "Update" (a common group driven from Unity's `Update()` or a main C\# loop).

-----

## 🟢 Defining States: `.State()`

The `.State()` method is where you define a distinct **mode or phase** for your FSM and specify the actions that occur while in that state.

**C\# Example:**

```csharp
builder.State("Patrol",
    onEnter: ctx => ((EnemyContext)ctx).StartPatrolAnimation(),
    onUpdate: ctx => ((EnemyContext)ctx).ExecutePatrolLogic(),
    onExit: ctx => ((EnemyContext)ctx).StopPatrolAnimation()
);
```

### Parameters:

  * `stateName` (string): **Required.** A unique name for this state within the FSM definition (e.g., "Idle", "Attacking", "Door\_Open").
  * `onEnter` (Action\<IStateContext\>, optional): A delegate/lambda that executes **once** when the FSM **enters** this state. Ideal for one-time setup (e.g., starting an animation, playing a sound, initializing variables).
  * `onUpdate` (Action\<IStateContext\>, optional): A delegate/lambda that executes **repeatedly** for as long as the FSM **remains** in this state. The frequency depends on the FSM's `processRate` and its processing group's update cycle. Use for continuous actions (e.g., movement, checking conditions over time).
  * `onExit` (Action\<IStateContext\>, optional): A delegate/lambda that executes **once** when the FSM **exits** this state to transition to another. Useful for cleanup (e.g., stopping animations, resetting values, triggering final actions).

### Context Parameter (`ctx` or `context`)

Notice the `ctx` (or `context`) parameter in the state actions. This is your **`IStateContext`** object (your MonoBehaviour in Unity, or your plain C\# class). You'll typically cast it to your specific context type to access its unique properties and methods.

**C\# Example:**

```csharp
.State("ChargingAttack",
    onEnter: context => {
        var enemy = (EnemyContext)context; // Cast to your specific context type
        enemy.PlayChargingSound();
        enemy.TargetPlayer();
    },
    onUpdate: context => {
        var enemy = (EnemyContext)context;
        enemy.IncreaseCharge();
        if (enemy.IsChargeMaxed()) { /* Trigger transition */ }
    }
)
```

-----

## ↩️ Setting the Start Point: `.WithInitialState()`

This method designates the **starting state** for any new `FSMHandle` instance created from this FSM definition.

**C\# Example:**

```csharp
builder.WithInitialState("Idle");
```

### Parameters:

  * `stateName` (string): The name of the state that instances of this FSM definition should begin in.
      * **Important:** This state must already be defined using the `.State()` method before calling `WithInitialState()`.

-----

## ➡️ Defining Transitions: `.Transition()`

The `.Transition()` method establishes a **directional rule** for moving from a specific "source" state to a "destination" state, based on a given condition.

**C\# Example:**

```csharp
builder.Transition("Idle", "Walking", ctx => ((PlayerContext)ctx).HasMovementInput);
```

### Parameters:

  * `fromStateName` (string): The name of the state from which the transition originates (the "source").
  * `toStateName` (string): The name of the state to which the FSM will move (the "destination").
  * `condition` (Func\<IStateContext, bool\>): A delegate/lambda that returns `true` if the transition should occur, and `false` otherwise. This is your rule for moving between states.
      * The `condition` is checked during the `onUpdate` phase of the current state.
      * If multiple transitions from the current state become true simultaneously, **the order in which they were defined in the `FSMBuilder` determines priority.** The first true condition found will trigger its transition.

**Example with multiple conditions:**

```csharp
builder
    .State("Chasing", ...)
    .State("Attacking", ...)
    .State("Fleeing", ...)
    .Transition("Chasing", "Attacking", ctx => ((EnemyContext)ctx).IsInAttackRange()) // Checked first
    .Transition("Chasing", "Fleeing", ctx => ((EnemyContext)ctx).HealthIsLow())      // Checked second
    .Transition("Chasing", "Idle", ctx => ((EnemyContext)ctx).TargetLost());         // Checked third
```

-----

## 🌍 Global Transitions: `.AnyTransition()`

`AnyTransition()` defines a special type of transition that can occur **from any state** within the FSM to a specified destination state, provided its condition is met. These are often used for "interrupt" or "emergency" behaviors.

**C\# Example:**

```csharp
builder.AnyTransition("Dead", ctx => ((CharacterContext)ctx).CurrentHealth <= 0);
```

### Parameters:

  * `toStateName` (string): The name of the state to which the FSM will move.
  * `condition` (Func\<IStateContext, bool\>): A delegate/lambda that returns `true` if the global transition should occur.

### Important Considerations for `AnyTransition()`:

  * **Priority:** `AnyTransition()` conditions are evaluated **before** specific `Transition()` conditions from the current state. This makes them ideal for critical, high-priority state changes (e.g., "If health drops to zero, immediately go to 'Dead' state, regardless of what I was doing.").
  * **Order:** If multiple `AnyTransition()` conditions are true, their order of definition in the `FSMBuilder` determines priority.

-----

## ⏱ Controlling Update Rate: `.WithProcessRate()`

While `FSM_API.CreateFiniteStateMachine()` allows you to set an initial `processRate`, you can also define or override it explicitly using `WithProcessRate()`. This method controls how frequently `onUpdate` methods are called and how often transitions are checked for *instances* created from this specific FSM definition.

**C\# Example:**

```csharp
builder.WithProcessRate(5); // Instances of this FSM will update every 5 ticks of their processing group.
```

### Parameters:

  * `processRate` (int): The update frequency.
      * `0`: Only updates when manually stepped using `FSM_API.Step()`.
      * `1`: Updates every tick.
      * `N` (where N \> 1): Updates every N ticks.

-----

## 🏷 Naming Your FSM Blueprint: `.WithName()`

If you didn't provide a name in `FSM_API.CreateFiniteStateMachine()`, or if you wish to change the FSM definition's name before building it, use `WithName()`.

**C\# Example:**

```csharp
FSM_API.CreateFiniteStateMachine()
    .WithName("DynamicEnemyBehavior") // Setting the FSM's name
    .State(...)
    .BuildDefinition();
```

### Parameters:

  * `fsmName` (string): The unique name for this FSM definition.

-----

## ⚙️ Assigning a Processing Group: `.WithProcessingGroup()`

This method assigns the **default processing group** for all `FSMHandle` instances created from this FSM definition. Processing groups are critical for organizing and updating your FSMs efficiently, especially in environments with distinct update loops (like Unity's `Update`, `FixedUpdate`).

**C\# Example:**

```csharp
builder.WithProcessingGroup("GameLogic"); // Instances will default to the "GameLogic" group
```

### Parameters:

  * `groupName` (string): The name of the processing group (e.g., "Gameplay", "UIA", "Physics").

### How it integrates with `FSM_UnityIntegration` (for Unity users):

The `FSM_UnityIntegration` script (discussed in **[02. Getting Started with Unity](02_Getting_Started_Unity.md)**) will automatically create and update the "Update", "FixedUpdate", "LateUpdate", etc., processing groups. By using `WithProcessingGroup("Update")` or `WithProcessingGroup("FixedUpdate")` in your builder, you're instructing FSM_API to include instances of this definition in those Unity-driven update cycles by default.

-----

## 🚀 Finalizing the Blueprint: `.BuildDefinition()`

This is the **essential final step** in the `FSMBuilder` chain. Calling `BuildDefinition()` processes all the states, transitions, and configurations you've defined, then **registers this complete FSM blueprint** with the FSM\_API system.

**C\# Example:**

```csharp
var playerFSMDefinition = FSM_API.CreateFiniteStateMachine("PlayerFSM")
    // ... all your State(), Transition(), etc. calls ...
    .BuildDefinition(); // Must be called to finalize
```

### What `BuildDefinition()` does:

  * **Validates:** It performs internal checks to ensure your FSM is valid (e.g., initial state exists, no duplicate state names).
  * **Registers:** It makes your FSM definition available by its name (`"PlayerFSM"` in the example) for future instantiation using `FSM_API.CreateInstance()`.
  * **Returns:** It returns the `IFSMDefinition` object, representing your immutable blueprint. You usually don't need to store this, as you'll retrieve FSMs by name for instance creation.

-----

## 🧩 Summary of `FSMBuilder` Methods

| Method                  | Purpose                                                                                                                                                                                                                          |
| :---------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `State(...)`            | Defines a named state and its `onEnter`, `onUpdate`, and `onExit` actions.                                                                                                                                                   |
| `WithInitialState(...)` | Sets the default starting state for any new FSM instance created from this blueprint.                                                                                                                                          |
| `Transition(...)`       | Defines a conditional rule for moving from a specific source state to a specific destination state. Order matters for priority.                                                                                                |
| `AnyTransition(...)`    | Defines a global conditional rule for moving from *any* state to a specific destination state. These have higher priority than regular transitions.                                                                          |
| `WithProcessRate(...)`  | Sets the default update frequency (`onUpdate` calls and transition checks) for instances of this FSM definition. (e.g., every tick, every N ticks, or only when manually stepped).                                          |
| `WithName(...)`         | Assigns the unique identifier for this FSM definition blueprint.                                                                                                                                                             |
| `WithProcessingGroup(...)` | Assigns the default processing group for instances of this FSM definition, controlling when they are collectively updated by `FSM_API.Update("GroupName")`.                                                                 |
| `BuildDefinition()`     | **Finalizes and registers** the FSM blueprint with FSM\_API. This is a mandatory last step after configuring your FSM.                                                                                                        |

`FSMBuilder` is your FSM factory — design once, reuse everywhere, and rapidly iterate on your state-driven behaviors.

-----

[➡️ Continue to: 05. Understanding and Implementing Your Context (IStateContext)](05_Context_Implementation.md)