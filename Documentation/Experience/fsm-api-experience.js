(() => {
  "use strict";
  const root = document.querySelector("[data-fsm-demo]");
  if (!root) return;

  const stateCards = [...root.querySelectorAll("[data-state]")];
  const current = root.querySelector("[data-current]");
  const requested = root.querySelector("[data-requested]");
  const tickNumber = root.querySelector("[data-tick]");
  const log = root.querySelector("[data-log]");
  const flip = root.querySelector("[data-flip]");
  const step = root.querySelector("[data-step]");
  const reset = root.querySelector("[data-reset]");

  const initial = { state: "Off", requested: "Off", tick: 0, entryPending: true };
  let model = { ...initial };

  function render(message) {
    stateCards.forEach(card => {
      const active = card.dataset.state === model.state;
      card.classList.toggle("is-active", active);
      card.setAttribute("aria-current", active ? "step" : "false");
    });
    current.textContent = model.state;
    requested.textContent = model.requested;
    tickNumber.textContent = String(model.tick);
    log.textContent = message;
    flip.textContent = model.requested === "Off" ? "Request ON" : "Request OFF";
    flip.setAttribute("aria-label", model.requested === "Off" ? "Request transition to On" : "Request transition to Off");
  }

  flip.addEventListener("click", () => {
    model.requested = model.requested === "Off" ? "On" : "Off";
    render("Input recorded in the application context. The FSM has not advanced yet; the host still controls the next tick.");
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
      phases.push("condition satisfied: " + previous + " → " + model.state);
      phases.push("onEnter(" + model.state + ") is deferred until the next tick");
    } else {
      phases.push("no transition; remain in " + model.state);
    }
    render("Tick " + model.tick + " · " + phases.join(" → "));
  });

  reset.addEventListener("click", () => {
    model = { ...initial };
    render("Reset to the initial state. The first entry action will run on the next tick.");
  });

  render("Ready. Request a state change, then advance the host-controlled tick to evaluate it.");
})();
