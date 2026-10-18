<p align="center">
  <a href="https://github.com/TrentBest/WebPage/tree/development/TheSingularityWorkshop/wwwroot/Images" aria-label="The Singularity Workshop living moniker">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living moniker — a digital vortex converging on its central eye" width="128" />
  </a>
</p>

# FSM_API — finite-state behavior you can see

A state machine is easier to understand when its states, transitions, and lifecycle can be seen. FSM_API is a pure C# library for describing reusable finite-state behavior and running independent instances in any host application.

[![NuGet version](https://img.shields.io/nuget/v/TheSingularityWorkshop.FSM_API?style=flat-square&logo=nuget)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
[![License: MIT](https://img.shields.io/badge/license-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_API/dotnet.yml?branch=master&style=flat-square)](https://github.com/TrentBest/FSM_API/actions)

## 🟦 01 What

FSM_API separates a behavior's reusable definition from each live instance and from the application data that instance operates on.

- **Definition:** named states, transition conditions, and state actions.
- **Instance / handle:** one independent execution of a definition.
- **IStateContext:** your application-owned data and operations.
- **Processing group:** a named set of FSM instances advanced together by the host.

<p align="center">
  <a href="https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/Visuals/FSM_API_Architecture.svg">
    <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/User%20Guide/Visuals/FSM_API_Architecture.svg" alt="Animated FSM_API architecture: a reusable definition feeds a live instance that evaluates guarded transitions and works through application-owned context data" width="960" />
  </a>
</p>

*This is a conceptual animation, not a live process monitor. It uses declarative SVG animation; the implementation and tests define actual runtime behavior.*

## 🟣 02 Why

Scattered conditionals make complex behavior difficult to reason about and test. An FSM makes possible states and transition rules explicit. Reusing a definition avoids duplicating behavior, while separate instances retain independent state and context.

FSM_API is designed to remain host-agnostic: the core is pure C#, does not require a game engine, and does not own your application's main loop.

## 🔺 03 How

Install [TheSingularityWorkshop.FSM_API from NuGet](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API), then define your context, build a finite-state machine, create an instance, and tick its processing group.

```csharp
using System;
using TheSingularityWorkshop.FSM_API;

public sealed class LightSwitch : IStateContext
{
    public string Name { get; set; } = "KitchenLight";
    public bool IsValid => true;
    public bool RequestedOn { get; set; } // Set by application input.
    public bool IsOn { get; set; }        // Set by FSM state actions.
}

FSM_API.CreateProcessingGroup("MainLoop");

Func<IStateContext, bool> shouldTurnOn =
    context => context is LightSwitch light && light.RequestedOn;

Func<IStateContext, bool> shouldTurnOff =
    context => context is LightSwitch light && !light.RequestedOn;

FSM_API.Create.CreateFiniteStateMachine(
        "LightSwitchFSM", processRate: 1, processingGroup: "MainLoop")
    .State("Off", onEnter: context =>
    {
        if (context is LightSwitch light) light.IsOn = false;
    })
    .State("On", onEnter: context =>
    {
        if (context is LightSwitch light) light.IsOn = true;
    })
    .WithInitialState("Off")
    .Transition("Off", "On", shouldTurnOn)
    .Transition("On", "Off", shouldTurnOff)
    .BuildDefinition();

var kitchenLight = new LightSwitch();
var handle = FSM_API.Create.CreateInstance(
    "LightSwitchFSM", kitchenLight, "MainLoop");

FSM_API.Interaction.Update("MainLoop");
```

The host decides when to call Update. Transition conditions determine whether an instance changes state; the application context remains yours to own and evolve.

## 🟢 04 See it in a minute

- Start with the [C# getting-started guide](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/03_Getting_Started_CSharp.md).
- Explore [core concepts](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/01_Core_Concepts.md).
- Read the [theory guides](https://github.com/TrentBest/FSM_API/tree/master/Documentation/User%20Guide/Theory).
- Inspect the [animated architecture SVG](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/Visuals/FSM_API_Architecture.svg).

## 🟪 05 Documentation and theory

- [Full documentation index](https://github.com/TrentBest/FSM_API/blob/master/Documentation/ReadMe.md)
- [FSMBuilder deep dive](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/04_FSM_Builder_Deep_Dive.md)
- [Context implementation](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/05_Context_Implementation.md)
- [Runtime modification](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/06_FSM_Modifier_Deep_Dive.md)
- [Error handling](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/07_Error_Handling.md)
- [Performance tips](https://github.com/TrentBest/FSM_API/blob/master/Documentation/User%20Guide/08_Performance_Tips.md)
- [GUI.SVG adapter documentation](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/SVG_ADAPTER.md)
- [Source repository and issues](https://github.com/TrentBest/FSM_API)


---

<p align="center">
  <a href="https://github.com/TrentBest/WebPage/tree/development/TheSingularityWorkshop/wwwroot/Images" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
