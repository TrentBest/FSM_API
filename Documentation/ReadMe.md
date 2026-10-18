![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API — finite-state behavior you can see

A state machine is easier to understand when you can watch its structure and timing. FSM_API turns named states, guarded transitions, and state actions into reusable behavior—without tying the core library to a game engine, UI framework, or application domain.

[![NuGet](https://img.shields.io/nuget/v/TheSingularityWorkshop.FSM_API?style=flat-square&logo=nuget)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
[![License: MIT](https://img.shields.io/badge/license-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_API/dotnet.yml?branch=master&style=flat-square)](https://github.com/TrentBest/FSM_API/actions)

## 🟦 01 What

FSM_API is a pure C# finite-state-machine library. Define a behavior once, create independent instances, and let each instance progress through its states using its own context data.

- **Definition:** the reusable blueprint—states, transitions, and actions.
- **Instance / handle:** one live execution of that blueprint.
- **State context:** your application-owned data and operations.
- **Processing group:** a named set of instances that the host can tick together.

## 🟣 02 Why

Complex behavior often collapses into scattered conditionals and implicit lifecycle rules. An FSM makes the legal phases and the conditions for moving between them explicit. Separating the definition, live instance, and context keeps behavior reusable and application data in your hands.

## 🔺 03 How

Describe the states and transition conditions with the builder API, create an instance with an IStateContext, then advance the chosen processing group through FSM_API.Interaction.Update(...). The host owns its main loop; FSM_API owns the state-machine execution semantics.

The animation below is a **conceptual model**, not a live debugger. It illustrates the relationship between the reusable definition, a running instance, and its context. The executable code and tests remain authoritative.

<p align="center">
  <a href="User%20Guide/Visuals/FSM_API_Architecture.svg">
    <img src="User%20Guide/Visuals/FSM_API_Architecture.svg" alt="Animated architecture diagram: a reusable FSM definition feeds a live instance, which evaluates guarded transitions and interacts with application-owned context data" width="960" />
  </a>
</p>

The SVG is deliberately self-contained and declarative. It uses SVG shapes and animation—not JavaScript—and can be inspected as source. For the richer SVG manifestation vocabulary, animation model, accessibility rules, and security boundary, see [GUI.SVG's adapter documentation](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/SVG_ADAPTER.md).

## 🟢 04 See it in a minute

1. Read [00 — Introduction](User%20Guide/00_Introduction.md) for the big picture.
2. Follow [03 — Getting Started with C#](User%20Guide/03_Getting_Started_CSharp.md) to build a real FSM.
3. Explore [01 — Core Concepts](User%20Guide/01_Core_Concepts.md) for definitions, instances, states, and contexts.
4. Browse the [FSM theory: What Are FSMs?](User%20Guide/Theory/01_What_Are_FSMs.md) to understand the wider FSM domain.

## 🟪 05 Documentation and theory

- [Getting started](User%20Guide/03_Getting_Started_CSharp.md)
- [Core concepts](User%20Guide/01_Core_Concepts.md)
- [FSMBuilder deep dive](User%20Guide/04_FSM_Builder_Deep_Dive.md)
- [Context implementation](User%20Guide/05_Context_Implementation.md)
- [Runtime modification](User%20Guide/06_FSM_Modifier_Deep_Dive.md)
- [Error handling](User%20Guide/07_Error_Handling.md)
- [Performance tips](User%20Guide/08_Performance_Tips.md)
- [FSM theory: What Are FSMs?](User%20Guide/Theory/01_What_Are_FSMs.md)
- [Types of FSMs](User%20Guide/Theory/02_Types_Of_FSMs.md)
- [Advantages and disadvantages](User%20Guide/Theory/03_Advantages_And_Disadvantages.md)
- [When to use FSMs](User%20Guide/Theory/04_When_To_Use_FSMs.md)
- [Limitations and alternatives](User%20Guide/Theory/05_FSM_Limitations_And_Alternatives.md)
- [Tick boundaries and registry ownership](User%20Guide/Theory/06_Tick_Boundaries_And_Registry_Ownership.md)
- [Use cases](User%20Guide/09_Common_Use_Cases.md)
- [FAQ](User%20Guide/11_FAQ.md)
- [Generated API reference](../_site/TheSingularityWorkshop.FSM_API.html)
- [Source repository](https://github.com/TrentBest/FSM_API)
- [GUI.SVG adapter and gallery](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/SVG_ADAPTER.md)


---

<p align="center">
  <a href="https://github.com/TrentBest/WebPage/tree/development/TheSingularityWorkshop/wwwroot/Images" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
