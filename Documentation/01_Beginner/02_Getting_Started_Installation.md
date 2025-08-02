
# 03_Your_First_FSM_States_and_Transitions

**FSM_API Documentation** | **The Singularity Workshop**
[GitHub Repo](https://github.com/TrentBest/FSM_API) | [GitPages Docs](https://trentbest.github.io/FSM_API/)

---

⬅️ [Previous: 02_Getting_Started_Installation](02_Getting_Started_Installation.md) | [Next: 04_Making_Your_FSM_Do_Things_FSMHandles](04_Making_Your_FSM_Do_Things_FSMHandles.md) ➡️

---

## Defining Your First Finite State Machine (FSM)

Now that you have FSM_API in your project, let's build your very first [Finite State Machine](javascript:void(0) "A mathematical model of computation that describes the behavior of systems based on their current 'state' and triggered 'events'.")! This document will guide you through defining the core components: your data [Context](javascript:void(0) "The application-specific data or object that an FSM operates on and whose behavior it controls. Must implement IStateContext."), [States](javascript:void(0) "A distinct condition or mode that a system or entity can be in at a specific point in time."), and [Transitions](javascript:void(0) "The movement or change from one state to another state.") between them.

We'll use a simple "Light Switch" example, which perfectly illustrates how FSMs manage predictable behavior.

---

## Step 1: Define Your Data Context (`IStateContext`)

Every FSM instance needs to operate on some data. This data is called the "[Context](javascript:void(0) "The application-specific data or object that an FSM operates on and whose behavior it controls. Must implement IStateContext.")," and it must implement the `IStateContext` interface provided by FSM_API. This keeps your FSM logic clean and separate from your application's specific data.

For our light switch, the context simply needs to know if the light `IsOn`.

```csharp
using TheSingularityWorkshop.FSM_API; // Don't forget this using statement!

public class LightSwitchContext : IStateContext
{
    // This property will determine the state of our light switch
    public bool IsOn = false;

    // IStateContext requires an IsValid property. For now, just return true.
    // This is used for advanced FSM validation and lifecycle management.
    public bool IsValid => true;

    // IStateContext also requires a Name property.
    public string Name { get; set; } = "KitchenLight";

    // You can add other relevant data here for your specific FSM
    // public int BrightnessLevel = 0;
}
````

**What's happening here?**

* We've created a class `LightSwitchContext` to hold the data relevant to our light switch.
* The `IsOn` boolean variable is what our FSM will "look at" to decide what state the light is in.
* `IsValid` and `Name` are required by the `IStateContext` interface. For simple FSMs, `IsValid => true;` is perfectly fine.

## Step 2: Define Your FSM Structure (`FSM_API.Create`)

Now comes the fun part: defining the actual states and transitions of your FSM. We'll use the `FSM_API.Create` helper class and its fluent methods (meaning you chain calls together) to build our FSM definition.

```csharp
using TheSingularityWorkshop.FSM_API; // Still need this!
using System; // For Console.WriteLine in our example actions

public static class LightSwitchFSMDefinition
{
    public static void DefineLightSwitchFSM()
    {
        // Start defining a new Finite State Machine
        // Give it a unique name: "LightSwitchFSM"
        // processRate: -1 means it's event-driven, we'll manually tell it to process later.
        //              (0 means it processes immediately when its context changes,
        //               >0 means it ticks every X milliseconds)
        var fsmDefinition = FSM_API.Create.CreateFiniteStateMachine(
            fsmName: "LightSwitchFSM",
            processRate: -1 // Manual processing for now
        )
        // Define the "Off" State
        .State("Off",
            // What happens when the FSM enters the "Off" state
            onEnter: ctx =>
            {
                if (ctx is LightSwitchContext l)
                {
                    l.IsOn = false; // Ensure the context reflects the state
                    Console.WriteLine($"{l.Name} is now OFF.");
                }
            }
        )
        // Define a Transition FROM "Off" TO "On"
        // This transition happens IF the condition (ctx => ctx is LightSwitchContext l && l.IsOn) is true
        .TransitionIf("On", ctx => ctx is LightSwitchContext l && l.IsOn)

        // Define the "On" State
        .State("On",
            // What happens when the FSM enters the "On" state
            onEnter: ctx =>
            {
                if (ctx is LightSwitchContext l)
                {
                    l.IsOn = true; // Ensure the context reflects the state
                    Console.WriteLine($"{l.Name} is now ON.");
                }
            }
        )
        // Define a Transition FROM "On" TO "Off"
        // This transition happens IF the condition (ctx => ctx is LightSwitchContext l && !l.IsOn) is true
        .TransitionIf("Off", ctx => ctx is LightSwitchContext l && !l.IsOn)

        // Set the initial state that new FSM instances will start in
        .WithInitialState("Off")

        // Finalize and register this FSM definition with the FSM_API system
        .BuildDefinition();

        Console.WriteLine("LightSwitchFSM definition created!");
    }
}
```

**Let's break down this FSM definition:**

* `FSM_API.Create.CreateFiniteStateMachine("LightSwitchFSM", processRate: -1)`: This is where you tell FSM\_API you're creating a new FSM blueprint named "LightSwitchFSM". `processRate: -1` means you'll manually trigger its updates (which we'll cover in the next document).
* `.State("Off", onEnter: ...)`: You're defining a [State](https://www.google.com/search?q=javascript:void\(0\) "A distinct condition or mode that a system or entity can be in at a specific point in time.") named "Off". The `onEnter` action is a piece of code (a "lambda expression") that runs *every time* the FSM enters this state. Here, we ensure `IsOn` is `false` and print a message.
* `.TransitionIf("On", ctx => ctx is LightSwitchContext l && l.IsOn)`: This line defines a [Transition](https://www.google.com/search?q=javascript:void\(0\) "The movement or change from one state to another state.") from the *current* state (which is "Off" in this chain) to the "On" state. The transition only occurs `if` the condition `l.IsOn` is true (meaning the light switch was flipped).
* `.State("On", onEnter: ...)` and `.TransitionIf("Off", ...)`: We do the same for the "On" state, defining its behavior and how it transitions back to "Off".
* `.WithInitialState("Off")`: This tells FSM\_API that any *new* instances of "LightSwitchFSM" should always start in the "Off" state.
* `.BuildDefinition()`: This crucial call finalizes your FSM blueprint and registers it within the FSM\_API system, making it available for use.

## What's Next?

You've successfully defined your first FSM\! Now, let's learn how to create a live instance of this FSM and make it do something in your application.

➡️ Continue to: [04\_Making\_Your\_FSM\_Do\_Things\_FSMHandles](https://www.google.com/search?q=04_Making_Your_FSM_Do_Things_FSMHandles.md) to discover `FSMHandle` and how to interact with your FSMs.
