---
title: FSM_API — Behavior, made visible
---

<link rel="stylesheet" href="Documentation/Experience/fsm-api-experience.css">

<div class="fsm-experience">
  <header class="fsm-hero">
    <div class="fsm-kicker">The Singularity Workshop · Behavior Laboratory</div>
    <h1>Behavior isn't magic.<span class="fsm-hero-line">Make it visible.</span></h1>
    <p>Every application is full of things that change: a door opens, an agent reacts, a workflow advances, a connection fails and recovers. FSM_API gives those changes an explicit model — states, conditions, actions, and a heartbeat controlled by your application.</p>
  </header>

  <section class="fsm-proof-strip" aria-label="Current evidence snapshot">
    <article class="fsm-proof"><strong>492 / 492</strong><span>Automated .NET 10 tests passed on development</span><a href="https://github.com/TrentBest/FSM_API/actions/runs/46087840183">Run #1134 · commit f296d4f</a></article>
    <article class="fsm-proof"><strong>6 targets</strong><span>Library compile matrix: .NET 10, .NET 6, .NET Core 3.1, .NET Standard 2.1/2.0, .NET Framework 4.7</span><a href="Documentation/FSM_API_2.0.0_READINESS.md">Compatibility and readiness details</a></article>
    <article class="fsm-proof"><strong>1.0.13</strong><span>Published NuGet release · historical benchmark baseline</span><a href="https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API">Inspect the published package</a></article>
  </section>

  <div class="fsm-section-kicker">01 / Touch the behavior</div>
  <section class="fsm-demo" data-fsm-demo aria-label="Interactive LightSwitch finite state machine demonstration">
    <div class="fsm-demo-head"><span class="fsm-live">Interactive model</span><span class="fsm-status">A tiny machine. A real idea.</span></div>
    <div class="fsm-machine" aria-label="Two-state LightSwitch finite state machine">
      <div class="fsm-state is-active" data-state="Off" aria-current="step">
        <svg viewBox="0 0 64 64" role="img" aria-label="Light bulb switched off"><path d="M22 43c-2-4-8-8-8-17a18 18 0 1 1 36 0c0 9-6 13-8 17H22Z" fill="none" stroke="#59d8ed" stroke-width="3" stroke-linejoin="round"/><path d="M24 49h16M26 55h12M28 43h8" stroke="#59d8ed" stroke-width="3" stroke-linecap="round"/><path d="M25 20l14 14m0-14L25 34" stroke="#a9bdd0" stroke-width="2" stroke-linecap="round"/></svg>
        <strong>OFF</strong><small>Quiet · waiting</small>
      </div>
      <div class="fsm-transition"><span class="fsm-arrow" aria-hidden="true">⇄</span><span>request / condition</span><small>evaluated on tick</small></div>
      <div class="fsm-state" data-state="On" aria-current="false">
        <svg viewBox="0 0 64 64" role="img" aria-label="Light bulb switched on"><path d="M22 43c-2-4-8-8-8-17a18 18 0 1 1 36 0c0 9-6 13-8 17H22Z" fill="none" stroke="#ffbf69" stroke-width="3" stroke-linejoin="round"/><path d="M24 49h16M26 55h12M28 43h8" stroke="#ffbf69" stroke-width="3" stroke-linecap="round"/><path d="M32 1v7M7 26h7m36 0h7M13 8l5 5m28-5-5 5" stroke="#ffbf69" stroke-width="3" stroke-linecap="round"/><path d="M32 15v12" stroke="#ffbf69" stroke-width="3" stroke-linecap="round"/></svg>
        <strong>ON</strong><small>Illuminating · active</small>
      </div>
    </div>
    <div class="fsm-controls" aria-label="LightSwitch controls"><button class="primary" type="button" data-flip>Request ON</button><button type="button" data-step>Advance one tick</button><button type="button" data-reset>Reset</button></div>
    <div class="fsm-readout" aria-live="polite"><div><span>Current state</span><strong data-current>Off</strong></div><div><span>Requested state</span><strong data-requested>Off</strong></div><div><span>Host ticks</span><strong data-tick>0</strong></div></div>
    <div class="fsm-log" data-log role="status">Ready. Request a state change, then advance the host-controlled tick.</div>
  </section>
  <p class="fsm-note">This browser-side teaching model is not the .NET runtime executing in your browser. It demonstrates the documented lifecycle boundary: input can update context, but the host decides when processing advances. A successful transition exits the old state on that tick; entry into the new state is deferred until the next tick.</p>

  <div class="fsm-section-kicker">02 / Put the numbers on the table</div>
  <div class="fsm-bench">
    <table aria-label="Historical FSM_API benchmark results">
      <thead><tr><th>Workload</th><th>Mean / operation</th><th>Allocated</th></tr></thead>
      <tbody>
        <tr><td>1 processing group</td><td><strong>305.1 ns</strong></td><td>360 B</td></tr>
        <tr><td>10 processing groups</td><td><strong>3,115.7 ns</strong></td><td>3,600 B</td></tr>
        <tr><td>50 processing groups</td><td><strong>15,736.6 ns</strong></td><td>18,000 B</td></tr>
      </tbody>
    </table>
    <div class="fsm-bench-foot">BenchmarkDotNet · September 2026 · published FSM_API 1.0.13 · .NET 10 · string-backed Update(group) workload · historical baseline, not a 2.0 measurement.</div>
  </div>
  <p class="fsm-evidence-note">The experiment measures real <code>Update(group)</code> calls through the published 1.0.13 interaction API and varies the number of explicitly updated groups. The observed growth is approximately linear for this workload and range. It is not a universal complexity guarantee or a prediction for every application. <a href="Documentation/Benchmarking.md">Read the benchmark method and interpretation</a> · <a href="https://github.com/TrentBest/FSM_API_Benchmark/blob/development/FSM_Benchmark/FSM_StringGroupIterationBenchmark.cs">Inspect the measured workload</a> · <a href="https://github.com/TrentBest/FSM_API_Benchmark/blob/development/Data/BenchmarkDotNet_FSM_StringGroupIterationBenchmark_20260923_211222.diagsession">Open the profiler session</a>.</p>

  <div class="fsm-truth" aria-label="How to read the evidence">
    <article><h3>Correctness evidence</h3><p>Automated tests check defined behavior and edge cases. The linked run is tied to the current development source commit; it is stronger evidence than a demo animation or a claim that the library has been used in production. The build succeeded with 261 warnings and 0 errors, so the project is not presented as warning-free.</p></article>
    <article><h3>Performance evidence</h3><p>Benchmarks measure a specific workload on a specific runtime and environment. The published 1.0.13 figures are useful baseline data; they do not establish 2.0 speedups.</p></article>
  </div>

  <nav class="fsm-paths" aria-label="Explore FSM_API">
    <a class="fsm-path" href="Documentation/User%20Guide/00_Introduction.md"><strong>03 · Understand</strong><span>What finite state machines are, and when they are useful.</span></a>
    <a class="fsm-path" href="Documentation/User%20Guide/03_Getting_Started_CSharp.md"><strong>04 · Build</strong><span>Create behavior in ordinary C# — no game engine required.</span></a>
    <a class="fsm-path" href="Documentation/User%20Guide/01_Core_Concepts.md"><strong>05 · Explore the engine</strong><span>Definitions, instances, contexts, transitions, and lifecycle.</span></a>
    <a class="fsm-path" href="Documentation/Performance/BenchmarkingContract.md"><strong>06 · Measure rigorously</strong><span>Stable benchmark identities, provenance, history, and regression policy.</span></a>
    <a class="fsm-path" href="Documentation/API_TRACEABILITY_2.0.0.md"><strong>07 · Inspect the contract</strong><span>What the concise API exposes, what each runtime implements, and where parity remains open.</span></a>
    <a class="fsm-path" href="Documentation/FSM_API_2.0.0_READINESS.md"><strong>08 · Track readiness</strong><span>Release gates and the evidence still required before 2.0 can ship.</span></a>
  </nav>
</div>

<script defer src="Documentation/Experience/fsm-api-experience.js"></script>

## The idea behind the machine

An FSM is not the object it controls. Your application owns its data and decides when the machine gets to run. FSM_API supplies the reusable behavior model: a definition of states and transitions, plus independent instances bound to application-owned contexts.

That separation is the point. A door, a document, a network connection, a simulation entity, and a background process can all have different data while sharing a clear model for how behavior changes.

## The implementation boundary

The 2.0 API is under development on `development`; it is **not the published NuGet API**. The published package remains version **1.0.13**. The concise facade exposes Create, Interact, Transfer, and Error, but the string-backed and integer-backed runtimes are independent registries and do not have complete feature or diagnostic parity. Definition transfer is explicit; it does not move live instances.

See the [public API contract](Documentation/FSM_API_2.0.0_PUBLIC_API_CONTRACT.md) and [source-backed API traceability map](Documentation/API_TRACEABILITY_2.0.0.md). The docs distinguish implemented behavior from remaining verification and release gates.

## The next experiment

Start with the [C# getting-started guide](Documentation/User%20Guide/03_Getting_Started_CSharp.md), inspect the [generated API reference](https://trentbest.github.io/FSM_API/TheSingularityWorkshop.FSM_API.html), then follow the code and tests behind any behavior that matters to your application.

<sub>The interactive model is a teaching aid. Source code, tests, benchmark provenance, and generated API reference remain authoritative.</sub>
