(() => {
  "use strict";
  const root = document.querySelector("[data-fsm-demo]");
  if (!root) return;
  const cards = [...root.querySelectorAll("[data-state]")];
  const current = root.querySelector("[data-current]");
  const requested = root.querySelector("[data-requested]");
  const ticks = root.querySelector("[data-tick]");
  const log = root.querySelector("[data-log]");
  const flip = root.querySelector("[data-flip]");
  const step = root.querySelector("[data-step]");
  const reset = root.querySelector("[data-reset]");
  if (![current, requested, ticks, log, flip, step, reset].every(Boolean)) return;

  const initial = Object.freeze({ state: "Off", requested: "Off", tick: 0, entryPending: true });
  let model = { ...initial };

  function render(message) {
    cards.forEach(card => {
      const active = card.dataset.state === model.state;
      card.classList.toggle("is-active", active);
      card.setAttribute("aria-current", active ? "step" : "false");
    });
    current.textContent = model.state;
    requested.textContent = model.requested;
    ticks.textContent = String(model.tick);
    log.textContent = message;
    flip.textContent = model.requested === "Off" ? "Request ON" : "Request OFF";
    flip.setAttribute("aria-label", model.requested === "Off" ? "Request transition to On" : "Request transition to Off");
  }

  flip.addEventListener("click", () => {
    model.requested = model.requested === "Off" ? "On" : "Off";
    render("Context changed: requested " + model.requested + ". No machine work has run. The host still owns the clock.");
  });

  step.addEventListener("click", () => {
    model.tick += 1;
    const phases = [];
    if (model.entryPending) {
      phases.push("onEnter(" + model.state + ")");
      model.entryPending = false;
    }
    phases.push("onUpdate(" + model.state + ")");
    if (model.requested !== model.state) {
      const previous = model.state;
      phases.push("onExit(" + previous + ")");
      model.state = model.requested;
      model.entryPending = true;
      phases.push("condition accepted");
      phases.push("onEnter(" + model.state + ") waits for next tick");
    } else {
      phases.push("no transition");
    }
    render("Host tick " + model.tick + " · " + phases.join(" → "));
  });

  reset.addEventListener("click", () => {
    model = { ...initial };
    render("Teaching model reset. The initial entry action will run on the next host tick.");
  });

  render("Ready. Change the request, then advance the host-controlled tick.");
})();
