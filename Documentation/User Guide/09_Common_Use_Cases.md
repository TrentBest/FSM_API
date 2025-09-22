# 09\. Common Use Cases & Examples

> Finite State Machines (FSMs) are a foundational pattern in software development, providing a clear and organized way to manage behavior that changes based on different modes or stages. FSM\_API empowers you to implement these patterns efficiently across a wide range of applications.

This section explores common scenarios where FSM\_API excels, providing conceptual examples and highlighting how its features streamline implementation.

-----

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


-----

## 🎮 Game Development

FSMs are ubiquitous in game development for managing character behaviors, game mechanics, and UI.

### 1\. **Character AI (Player & NPC)**

This is perhaps the most common application. FSMs provide a structured way to define how characters react to stimuli and what actions they perform.

  * **States:** `Idle`, `Patrol`, `Chase`, `Attack`, `Flee`, `Reload`, `Stunned`, `Dead`.
  * **Transitions:**
      * `Idle` → `Patrol` (timer expires)
      * `Patrol` → `Chase` (player detected)
      * `Chase` → `Attack` (player in range)
      * `Attack` → `Reload` (ammo low)
      * `Any` → `Stunned` (hit by stun effect)
      * `Any` → `Dead` (health \<= 0)
  * **FSM\_API Advantage:**
      * Clear separation of concerns: AI logic (`onUpdate`) is encapsulated within states, triggered by context data.
      * Scalability: Easily create hundreds of AI agents from a single FSM definition, each with its own `IStateContext`.
      * Runtime modification (`FSMModifier`): Dynamically change AI behavior (e.g., enemy "enrages" after taking damage, switching to a more aggressive FSM definition).
      * Processing groups: Update distant, less critical AI at a lower `processRate` for performance optimization.

**Conceptual C\# Example (Enemy AI):**

```csharp
FSM_API.CreateFiniteStateMachine("EnemyPatrolAI", processingGroup: "AI_Update", processRate: 2) // Update every 2 frames
    .State("Patrol",
        onEnter: ctx => ((EnemyContext)ctx).StartPatrolRoute(),
        onUpdate: ctx => ((EnemyContext)ctx).AdvancePatrol(),
        onExit: ctx => ((EnemyContext)ctx).StopPatrol())
    .State("Chase",
        onEnter: ctx => ((EnemyContext)ctx).PlayChaseMusic(),
        onUpdate: ctx => ((EnemyContext)ctx).MoveTowardsPlayer(),
        onExit: ctx => ((EnemyContext)ctx).StopChase())
    .WithInitialState("Patrol")
    .Transition("Patrol", "Chase", ctx => ((EnemyContext)ctx).IsPlayerDetected())
    .Transition("Chase", "Patrol", ctx => !((EnemyContext)ctx).IsPlayerDetected() && !((EnemyContext)ctx).IsPlayerInAttackRange())
    .BuildDefinition();

// ... Later, in an EnemyController MonoBehaviour:
// FSM_API.CreateInstance("EnemyPatrolAI", this);
```

-----

### 2\. **Game Mechanics & Object States**

FSMs are perfect for managing the states of interactive objects in your game world.

  * **Door:** `Closed`, `Opening`, `Open`, `Closing`, `Locked`.
  * **Lever:** `Off`, `Activating`, `On`, `Deactivating`.
  * **Power-up:** `Active`, `Expiring`, `Consumed`.
  * **FSM\_API Advantage:**
      * Encapsulation: All logic for a door's behavior is self-contained within its FSM definition.
      * Clear flow: It's immediately obvious what states an object can be in and how it transitions.
      * Easy extension: Add new states (e.g., `Jammed`, `Broken`) without refactoring existing code.

**Conceptual C\# Example (Door Control):**

```csharp
FSM_API.CreateFiniteStateMachine("DoorFSM")
    .State("Closed",
        onEnter: ctx => ((DoorContext)ctx).SetDoorVisuals(false),
        onUpdate: ctx => { /* Maybe check for auto-unlock */ })
    .State("Opening",
        onEnter: ctx => ((DoorContext)ctx).PlayOpeningAnimation(),
        onUpdate: ctx => ((DoorContext)ctx).ContinueOpeningAnimation())
    .State("Open",
        onEnter: ctx => ((DoorContext)ctx).SetDoorVisuals(true),
        onUpdate: ctx => { /* Maybe check for auto-close */ })
    .State("Closing",
        onEnter: ctx => ((DoorContext)ctx).PlayClosingAnimation(),
        onUpdate: ctx => ((DoorContext)ctx).ContinueClosingAnimation())
    .WithInitialState("Closed")
    .Transition("Closed", "Opening", ctx => ((DoorContext)ctx).IsActivated && !((DoorContext)ctx).IsLocked)
    .Transition("Opening", "Open", ctx => ((DoorContext)ctx).AnimationFinished())
    .Transition("Open", "Closing", ctx => ((DoorContext)ctx).ShouldAutoClose || ((DoorContext)ctx).IsDeactivated)
    .Transition("Closing", "Closed", ctx => ((DoorContext)ctx).AnimationFinished())
    .BuildDefinition();
```

-----

### 3\. **User Interface (UI) Management**

Complex UI elements often have intricate state logic that benefits greatly from FSMs.

  * **Button:** `Normal`, `Hovered`, `Pressed`, `Disabled`, `Selected`.
  * **Menu System:** `MainMenu`, `OptionsMenu`, `Credits`, `InGamePause`.
  * **Inventory Panel:** `Hidden`, `FadingIn`, `Visible`, `FadingOut`.
  * **FSM\_API Advantage:**
      * Reactive design: UI elements can easily react to user input or application state changes.
      * Animation synchronization: `onEnter` and `onExit` are perfect for triggering UI animations.
      * Modularity: Break down complex UI into smaller, manageable FSMs.

**Conceptual C\# Example (Menu Navigation):**

```csharp
FSM_API.CreateFiniteStateMachine("MainMenuFlow", processingGroup: "UI_Input", processRate: 0) // Event-driven
    .State("MainMenu",
        onEnter: ctx => ((UIManagerContext)ctx).ShowMainMenuPanel(),
        onExit: ctx => ((UIManagerContext)ctx).HideMainMenuPanel())
    .State("Options",
        onEnter: ctx => ((UIManagerContext)ctx).ShowOptionsPanel(),
        onExit: ctx => ((UIManagerContext)ctx).HideOptionsPanel())
    .State("Credits",
        onEnter: ctx => ((UIManagerContext)ctx).ShowCreditsPanel(),
        onExit: ctx => ((UIManagerContext)ctx).HideCreditsPanel())
    .WithInitialState("MainMenu")
    .Transition("MainMenu", "Options", ctx => ((UIManagerContext)ctx).OptionsButtonClicked())
    .Transition("Options", "MainMenu", ctx => ((UIManagerContext)ctx).OptionsBackButtonCLicked())
    .Transition("MainMenu", "Credits", ctx => ((UIManagerContext)ctx).CreditsButtonClicked())
    .Transition("Credits", "MainMenu", ctx => ((UIManagerContext)ctx).CreditsBackButtonCLicked())
    .BuildDefinition();

// FSM_API.Interaction.Step(myMenuFSMHandle) could be called on button clicks
```

-----

## 🖥️ General Application Development (Pure C\#)

FSMs are not limited to games. Any application with predictable, sequential workflows or conditional logic can benefit.

### 1\. **Workflow Management**

For applications that process data through defined stages.

  * **Order Processing:** `Received`, `Validated`, `Processing`, `Shipped`, `Cancelled`.
  * **Document Approval:** `Draft`, `UnderReview`, `Approved`, `Rejected`, `Revised`.
  * **FSM\_API Advantage:**
      * Clarity: Makes complex business logic easy to understand and audit.
      * Robustness: Ensures that processes only move forward when specific conditions are met, preventing invalid states.
      * Testability: Each state and transition can be unit tested in isolation.

**Conceptual C\# Example (Order Processing):**

```csharp
FSM_API.CreateFiniteStateMachine("OrderWorkflow", processingGroup: "BackendProcessor", processRate: 5)
    .State("Received", onUpdate: ctx => { /* Validate data */ })
    .State("Validated", onEnter: ctx => ((OrderContext)ctx).LogValidationSuccess())
    .State("Processing", onUpdate: ctx => { /* Perform payment/inventory checks */ })
    .State("Shipped", onEnter: ctx => ((OrderContext)ctx).SendShippingNotification())
    .State("Cancelled", onEnter: ctx => ((OrderContext)ctx).RefundCustomer())
    .WithInitialState("Received")
    .Transition("Received", "Validated", ctx => ((OrderContext)ctx).AllDataValid)
    .Transition("Validated", "Processing", ctx => ((OrderContext)ctx).PaymentAuthorized)
    .Transition("Processing", "Shipped", ctx => ((OrderContext)ctx).InventoryReserved && ((OrderContext)ctx).PackageReady)
    .AnyTransition("Cancelled", ctx => ((OrderContext)ctx).CancellationRequested || ((OrderContext)ctx).PaymentFailed)
    .BuildDefinition();
```

-----

### 2\. **Network Connection Management**

Managing the lifecycle of a network connection.

  * **States:** `Disconnected`, `Connecting`, `Connected`, `Authenticating`, `Reconnecting`, `Error`.
  * **Transitions:**
      * `Disconnected` → `Connecting` (user attempts connect)
      * `Connecting` → `Connected` (connection established)
      * `Connected` → `Authenticating` (server requests auth)
      * `Any` → `Reconnecting` (connection lost)
      * `Connecting` → `Error` (timeout)
  * **FSM\_API Advantage:**
      * Handles complexity: Network states can be tricky due to asynchronous operations and error conditions. FSMs simplify this.
      * Clear error paths: Define specific error states and how to recover or report them.
      * Event-driven: Can use `FSM_API.Interaction.Step()` to advance FSMs based on network events (e.g., `OnConnected`, `OnDisconnected`).

-----

### 3\. **Input Handling & Command Processing**

Interpreting sequences of inputs or commands.

  * **States:** `WaitingForFirstInput`, `WaitingForSecondInput`, `ComboExecuting`.
  * **Transitions:** Based on specific key presses or button sequences.
  * **FSM\_API Advantage:**
      * Pattern recognition: Easily define and detect complex input patterns (e.g., fighting game combos).
      * Contextual actions: Different states can interpret the same input differently.

-----

## ✅ Best Practices Across All Use Cases

  * **Single Responsibility Principle:** Design each FSM to manage a single, well-defined aspect of behavior (e.g., a "Combat FSM" vs. a "Movement FSM" for a character).
  * **Lean Contexts:** Your `IStateContext` objects should primarily hold data and atomic methods. The complex logic of *when* to change state belongs in the FSM definition.
  * **Clear State Names:** Use descriptive names for your states (e.g., "PlayerWalking", "EnemyAttacking") to improve readability and debugging.
  * **Meaningful Conditions:** Ensure your transition conditions are clear and performant.
  * **Prioritize `AnyTransition`:** Use `AnyTransition` for critical, interrupt-driven states (e.g., `Dead`, `Paused`).
  * **Utilize `processRate`:** Tune the `processRate` for each FSM definition to match its required update frequency, optimizing performance.

By applying FSM\_API thoughtfully, you can bring structure, clarity, and maintainability to the most complex behavioral challenges in your projects. This approach contributes significantly to having a solid asset for submission to the Unity Asset Store, as it demonstrates best practices in code organization and performance.

-----

[➡️ Continue to: 10. FSM_API for Non-Coders: A Big Picture Overview](10_Non_Coder_Overview.md)


<a href="https://www.patreon.com/TheSingularityWorkshop" target="_blank">
    <img src="Branding/TheSingularityWorkshop.png" alt="Support The Singularity Workshop on Patreon" height="200" style="display: block;">
</a>

**Support the project:** [**Donate via PayPal**](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)