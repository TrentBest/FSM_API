---
title: FSM_API — Behavior, made visible
---

<link rel="stylesheet" href="Documentation/Experience/fsm-api-experience.css">

<div class="fsm-experience">
  <header class="fsm-hero">
    <div class="fsm-kicker">The Singularity Workshop · Behavior Laboratory</div>
    <h1>Make behavior<br>something you can see.</h1>
    <p>Applications are full of things that change: a door opens, a character reacts, a workflow advances, a connection fails and recovers. FSM_API gives those changes a deliberate structure — states, conditions, actions, and a heartbeat controlled by your application.</p>
  </header>

  <section class="fsm-demo" data-fsm-demo aria-label="Interactive LightSwitch finite state machine demonstration">
    <div class="fsm-demo-head">
      <span class="fsm-live">Interactive model</span>
      <span class="fsm-status">A tiny machine. A real idea.</span>
    </div>
    <div class="fsm-machine" aria-label="Two-state LightSwitch finite state machine">
      <div class="fsm-state is-active" data-state="Off" aria-current="step">
        <svg viewBox="0 0 64 64" role="img" aria-label="Light bulb switched off">
          <path d="M22 43c-2-4-8-8-8-17a18 18 0 1 1 36 0c0 9-6 13-8 17H22Z" fill="none" stroke="#59d8ed" stroke-width="3" stroke-linejoin="round"/>
          <path d="M24 49h16M26 55h12M28 43h8" stroke="#59d8ed" stroke-width="3" stroke-linecap="round"/>
          <path d="M25 20l14 14m0-14L25 34" stroke="#a9bdd0" stroke-width="2" stroke-linecap="round"/>
        </svg>
        <strong>OFF</strong>
        <small>Quiet · waiting</small>
      </div>
      <div class="fsm-transition">
        <span class="fsm-arrow" aria-hidden="true">⇄</span>
        <span>request / condition</span>
        <small>evaluated on tick</small>
      </div>
      <div class="fsm-state" data-state="On" aria-current="false">
        <svg viewBox="0 0 64 64" role="img" aria-label="Light bulb switched on">
          <path d="M22 43c-2-4-8-8-8-17a18 18 0 1 1 36 0c0 9-6 13-8 17H22Z" fill="none" stroke="#ffbf69" stroke-width="3" stroke-linejoin="round"/>
          <path d="M24 49h16M26 55h12M28 43h8" stroke="#ffbf69" stroke-width="3" stroke-linecap="round"/>
          <path d="M32 1v7M7 26h7m36 0h7M13 8l5 5m28-5-5 5" stroke="#ffbf69" stroke-width="3" stroke-linecap="round"/>
          <path d="M32 15v12" stroke="#ffbf69" stroke-width="3" stroke-linecap="round"/>
        </svg>
        <strong>ON</strong>
        <small>Illuminating · active</small>
      </div>
    </div>
    <div class="fsm-controls" aria-label="LightSwitch controls">
      <button class="primary" type="button" data-flip>Request ON</button>
      <button type="button" data-step>Advance one tick</button>
      <button type="button" data-reset>Reset</button>
    </div>
    <div class="fsm-readout" aria-live="polite">
      <div><span>Current state</span><strong data-current>Off</strong></div>
      <div><span>Requested state</span><strong data-requested>Off</strong></div>
      <div><span>Host ticks</span><strong data-tick>0</strong></div>
    </div>
    <div class="fsm-log" data-log role="status">Ready. Request a state change, then advance the host-controlled tick to evaluate it.</div>
  </section>
  <p class="fsm-note">This is a browser-side teaching model of the FSM lifecycle, not the .NET library running in your browser. It illustrates the important boundary: input can change the context at any time, but state processing advances only when the host calls the update method. A successful transition exits the old state immediately; entry into the new state is deferred until the next tick.</p>

  <nav class="fsm-paths" aria-label="Explore FSM_API">
    <a class="fsm-path" href="User%20Guide/00_Introduction.html"><strong>01 · Understand</strong><span>Explore finite state machines, their purpose, and the ideas behind the library.</span></a>
    <a class="fsm-path" href="User%20Guide/03_Getting_Started_CSharp.html"><strong>02 · Build</strong><span>Create a first state machine in ordinary C# — no game engine required.</span></a>
    <a class="fsm-path" href="User%20Guide/01_Core_Concepts.html"><strong>03 · Explore the engine</strong><span>Definitions, instances, contexts, transitions, and lifecycle.</span></a>
    <a class="fsm-path" href="Performance/BenchmarkingContract.html"><strong>04 · Measure</strong><span>See how performance claims are measured and what evidence is required.</span></a>
    <a class="fsm-path" href="TheSingularityWorkshop.FSM_API.html"><strong>05 · API reference</strong><span>Browse the generated API documentation and implementation surface.</span></a>
  </nav>
</div>

<script defer src="fsm-api-experience.js"></script>

## The idea behind the machine

An FSM is not the object it controls. Your application owns its data and decides when the machine gets to run. FSM_API supplies the reusable behavior model: a definition of states and transitions, plus independent instances bound to your application-owned contexts.

That separation is the point. A door, a document, a network connection, a game agent, and a background process can all have very different data — while using the same clear model for how behavior changes.

## Where to go next

- [Finite state machine theory](User%20Guide/Theory/README.html) — the conceptual foundations and trade-offs.
- [Tick boundaries and registry ownership](User%20Guide/Theory/06_Tick_Boundaries_And_Registry_Ownership.html) — how host cadence and runtime ownership work.
- [FSM_API 2.0.0 readiness](FSM_API_2.0.0_READINESS.html) — current development scope and release gates.
- [Source repository](https://github.com/TrentBest/FSM_API) · [NuGet package](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API).

---

<sub>The interactive diagram is a teaching aid. The C# implementation, tests, and generated API reference remain authoritative for runtime behavior.</sub>
