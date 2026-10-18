### `FSMHandle` Class: The FSM's Remote Control

The `FSMHandle` is your direct link to a live, running Finite State Machine instance. It acts as a **controller** that provides safe and managed access to the FSM's core components: the blueprint, the current state, and the instance-specific data. It abstracts away the complex internal workings of the FSM system, giving you a clean and intuitive way to manage an FSM's lifecycle.

-----

### Core Purpose

The `FSMHandle` is designed to be the single point of interaction for external systems. Rather than directly manipulating the FSM's internal state machine or data model, you use the handle to send commands, query the FSM's status, and access its context. This design is crucial for promoting the **decoupling** philosophy of the FSM\_API. It ensures that your application logic doesn't become tightly coupled to the FSM's implementation details.

#### Living picture: one handle through its lifecycle

![Animated lifecycle diagram for FSMHandle, from construction and first update through normal stepping, explicit override, and teardown.](../../../../User%20Guide/Visuals/FSMHandle_Lifecycle.svg)

The lifecycle is worth distinguishing because construction and state entry are not the same event:

- **Construction** links an FSM definition to an instance-specific `IStateContext` and initializes `CurrentState` to the definition's `InitialState`.
- **First update** performs the first step; the initial state's entry action is deferred until the handle is stepped.
- **Normal update** delegates to `Definition.Step` and stores the returned state name.
- **Forced transition** through `TransitionTo` bypasses normal transition conditions and immediately asks the definition to perform the transition.
- **Teardown** through the API's instance-destruction path exits the current state and invalidates the context.

The animation illustrates source-defined behavior; it is not connected to a live FSM instance.

-----

### Key Members

#### Public Properties

  * `public string CurrentState { get; internal set; }`: This is a fundamental property for designers and developers. It provides a simple way to check the FSM's current state without needing to understand its internal workings. For example, `if (playerHandle.CurrentState == "Idle")`.
  * `public IStateContext Context { get; set; }`: This property provides access to the FSM's data. While you can directly manipulate this data, the documentation correctly advises against this practice. It's generally safer to let the FSM's state actions (`OnEnter`, `OnUpdate`, `OnExit`) modify the context.
  * `public bool IsValid { get; }`: Reflects `Context.IsValid`. When false, automatic processing skips the handle while keeping it registered with its current state intact. Set the context's `IsValid` back to true to resume the same handle when safe.

#### Public Methods

  * `public void TransitionTo(string nextStateName)`: This method provides an escape hatch for situations where you need to override the FSM's logic and immediately force a state change. It's a powerful tool for handling events like a "game over" or a character's death.
  * `public void ResetFSMInstance()`: A utility method that resets the FSM to its `InitialState`, which is useful for situations like restarting a level or a game session.
  * `public void EvaluateConditions()`: This method is a manual trigger for event-driven FSMs. Instead of relying on a regular update loop, you can call this method whenever an external event occurs that might cause a state transition.
  * `public void Update(string processGroup)`: This is the method that drives the FSM's execution forward by one tick. The documentation correctly warns against calling this directly, as it's typically managed by the `FSM_API`'s internal scheduler.

-----

### Lifecycle Management

There are two different lifecycle operations:\n\n- **Suspend / reuse:** set `handle.Context.IsValid = false`. The scheduler skips the handle but retains it in the registered instance collection, preserving its current state. Set it back to `true` to resume that same handle when the context is safe to use.\n- **Destroy:** call `FSM_API.Interaction.DestroyInstance(handle)`. The API removes the handle from its registered collection and runs teardown; the internal `DestroyHandle()` path exits the current state and marks the context invalid. A destroyed handle is not a pooled inactive instance and should not be reused.
