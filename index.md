---
title: FSM_API — The heartbeat beneath the Workshop
---

<link rel="stylesheet" href="Documentation/Experience/fsm-api-experience.css">

<div class="fsm-experience">
  <header class="fsm-hero">
    <div class="fsm-kicker"><span class="pulse-dot"></span> The Singularity Workshop · Core Systems</div>
    <p class="fsm-eyebrow">Everything that changes has a story.</p>
    <h1>Give behavior<br><span>a heartbeat.</span></h1>
    <p class="fsm-lede">A door opens. An agent reacts. A city breathes. A build fails, learns, and tries again. Different worlds. Different data. The same fundamental question: <em>what happens next, and who decides?</em></p>
    <div class="fsm-hero-actions">
      <a class="fsm-cta" href="#heartbeat">Enter the living model <span aria-hidden="true">↓</span></a>
      <a class="fsm-text-link" href="https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API">Get FSM_API on NuGet ↗</a>
    </div>
    <div class="fsm-orbit" aria-hidden="true">
      <span class="orbit-ring ring-one"></span><span class="orbit-ring ring-two"></span>
      <span class="orbit-core">FSM<br><b>API</b></span>
      <span class="orbit-node node-one">DATA</span><span class="orbit-node node-two">TIME</span><span class="orbit-node node-three">ACTION</span>
    </div>
  </header>

  <section class="fsm-thesis">
    <div class="fsm-section-index">01 / THE CORE IDEA</div>
    <h2>The machine is not the thing it controls.</h2>
    <p>Your application owns its objects, data, services, and clock. FSM_API supplies a reusable model of behavior: definitions, states, transitions, conditions, and actions. It does not seize the host loop. It waits for the host to advance it.</p>
    <div class="fsm-principles">
      <article><span>01</span><strong>Define once</strong><p>Describe behavior independently from any one object.</p></article>
      <article><span>02</span><strong>Instantiate many</strong><p>Bind independent machine instances to application-owned contexts.</p></article>
      <article><span>03</span><strong>Advance deliberately</strong><p>Your host controls when a processing group receives its update.</p></article>
    </div>
  </section>

  <section class="fsm-lab" id="heartbeat" data-fsm-demo aria-label="Interactive FSM heartbeat laboratory">
    <div class="fsm-lab-heading">
      <div><div class="fsm-section-index">02 / TOUCH THE HEARTBEAT</div><h2>One signal. One deliberate transition.</h2></div>
      <span class="fsm-live"><i></i> Live teaching model</span>
    </div>
    <p class="fsm-lab-intro">Change the application context first. Then advance the host clock. Watch the machine distinguish an input from permission to process it.</p>
    <div class="fsm-machine" aria-label="Light switch finite state machine">
      <div class="fsm-state is-active" data-state="Off" aria-current="step">
        <svg viewBox="0 0 64 64" role="img" aria-label="Light bulb off"><path d="M22 43c-2-4-8-8-8-17a18 18 0 1 1 36 0c0 9-6 13-8 17H22Z" fill="none" stroke="currentColor" stroke-width="3"/><path d="M24 49h16M26 55h12M25 20l14 14m0-14L25 34" stroke="currentColor" stroke-width="3" stroke-linecap="round"/></svg>
        <strong>OFF</strong><small>Waiting</small>
      </div>
      <div class="fsm-transition"><span>⇄</span><b>condition</b><small>checked on host tick</small></div>
      <div class="fsm-state" data-state="On" aria-current="false">
        <svg viewBox="0 0 64 64" role="img" aria-label="Light bulb on"><path d="M22 43c-2-4-8-8-8-17a18 18 0 1 1 36 0c0 9-6 13-8 17H22Z" fill="none" stroke="currentColor" stroke-width="3"/><path d="M24 49h16M26 55h12M32 1v7M7 26h7m36 0h7M13 8l5 5m28-5-5 5M32 15v12" stroke="currentColor" stroke-width="3" stroke-linecap="round"/></svg>
        <strong>ON</strong><small>Illuminating</small>
      </div>
    </div>
    <div class="fsm-controls">
      <button class="primary" type="button" data-flip>Request ON</button>
      <button type="button" data-step>Advance host tick <span aria-hidden="true">→</span></button>
      <button type="button" data-reset>Reset model</button>
    </div>
    <div class="fsm-readout">
      <div><span>Machine state</span><strong data-current>Off</strong></div>
      <div><span>Context request</span><strong data-requested>Off</strong></div>
      <div><span>Host ticks</span><strong data-tick>0</strong></div>
    </div>
    <div class="fsm-log" data-log role="status" aria-live="polite">Ready. Change the request, then advance the host-controlled tick.</div>
    <p class="fsm-note">This is a browser-side teaching model, not the .NET runtime executing in your browser. It demonstrates the ownership boundary: input changes context; the host decides when FSM processing occurs. The source, tests, and generated API reference define actual runtime behavior.</p>
  </section>

  <section class="fsm-scale">
    <div class="fsm-section-index">03 / SAME PRINCIPLE. DIFFERENT WORLDS.</div>
    <h2>From a light switch to a living ecosystem.</h2>
    <p>The interesting part is not the demo. It is what happens when behavior becomes a reusable substrate for systems that are much larger than the example.</p>
    <div class="fsm-worlds">
      <article class="world-card world-foundation"><div class="world-number">01</div><div class="world-tag">THE FOUNDATION</div><h3>FSM_API</h3><p>Separate behavior from data. Give applications explicit state, transition, and lifecycle machinery without taking ownership of their world.</p><a href="https://github.com/TrentBest/FSM_API">Explore the source ↗</a></article>
      <article class="world-card world-ciai"><div class="world-number">02</div><div class="world-tag">THE FIRST COMMERCIAL PRODUCT</div><h3>CiAi</h3><p>Bring CI evidence into AI development without handing authority to the model. Observe exact commits, verify evidence, apply fail-closed policy, and keep human authorization at the boundary.</p><a href="https://trentbest.github.io/TheSingularityWorkshop.CiAi/">Enter the CiAi experience ↗</a></article>
      <article class="world-card world-next"><div class="world-number">03</div><div class="world-tag">THE WORKSHOP EXPANDS</div><h3>What comes next?</h3><p>Agents, simulations, tools, worlds, and experiences. Each system contributes a new proof of what becomes possible when behavior is explicit and composable.</p><a href="https://github.com/TrentBest">Explore the Workshop repositories ↗</a></article>
    </div>
  </section>

  <section class="fsm-ciai-bridge">
    <div class="bridge-signal" aria-hidden="true"><span></span><span></span><span></span><span></span></div>
    <div class="fsm-section-index">04 / THE NEXT DOOR</div>
    <h2>What if your development pipeline could think — without being allowed to improvise authority?</h2>
    <p>That is the question CiAi is built to explore. CI remains the source of truth. Evidence is tied to exact commits and workflow attempts. Policy decides whether evidence is sufficient. A model may propose; it does not grant itself permission.</p>
    <a class="fsm-cta" href="https://trentbest.github.io/TheSingularityWorkshop.CiAi/">Meet CiAi <span aria-hidden="true">↗</span></a>
    <p class="bridge-footnote">CiAi is the Workshop's first paid-product direction. This page introduces the product; consult CiAi's own experience for current capabilities and implementation status.</p>
  </section>

  <nav class="fsm-paths" aria-label="Continue exploring FSM_API">
    <a class="fsm-path" href="User%20Guide/00_Introduction.html"><strong>Understand</strong><span>Start with the theory and the problem FSMs solve.</span></a>
    <a class="fsm-path" href="User%20Guide/03_Getting_Started_CSharp.html"><strong>Build</strong><span>Create a machine in ordinary C#.</span></a>
    <a class="fsm-path" href="User%20Guide/01_Core_Concepts.html"><strong>Explore the engine</strong><span>Definitions, contexts, instances, transitions, and lifecycle.</span></a>
    <a class="fsm-path" href="Performance/BenchmarkingContract.html"><strong>Demand evidence</strong><span>Understand the benchmark contract and performance claims.</span></a>
    <a class="fsm-path" href="TheSingularityWorkshop.FSM_API.html"><strong>Inspect the API</strong><span>Open the generated reference for the implementation.</span></a>
  </nav>
</div>

<script defer src="fsm-api-experience.js"></script>

## The foundation is deliberately small

FSM_API is framework-independent. It does not need a game engine, web host, UI framework, database, or Workshop application layer. A host application defines when the machine runs and retains ownership of its data and scheduling.

That is why the same foundation can be used in a console application, a service, a simulation, a game, or a development pipeline. The domain changes. The behavior model remains explicit.

## Go deeper

- [Finite state machine theory](User%20Guide/Theory/README.html) — concepts, trade-offs, and formal foundations.
- [Tick boundaries and registry ownership](User%20Guide/Theory/06_Tick_Boundaries_And_Registry_Ownership.html) — host cadence and runtime ownership.
- [FSM_API 2.0.0 readiness](FSM_API_2.0.0_READINESS.html) — development scope and release gates.
- [Source repository](https://github.com/TrentBest/FSM_API) · [NuGet package](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API) · [CiAi](https://trentbest.github.io/TheSingularityWorkshop.CiAi/).

---

<sub>Implementation claims must be read against the source, tests, and version-specific API reference. The interactive model is explanatory, not runtime evidence.</sub>
