const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");

const listeners = new Map();
const elements = {};
for (const selector of ["[data-current]", "[data-requested]", "[data-tick]", "[data-log]", "[data-flip]", "[data-step]", "[data-reset]"]) {
  elements[selector] = {
    textContent: "",
    attributes: {},
    listeners: {},
    addEventListener(name, callback) { this.listeners[name] = callback; },
    setAttribute(name, value) { this.attributes[name] = value; }
  };
}

const cards = ["Off", "On"].map(state => ({
  dataset: { state },
  attributes: {},
  classes: new Set(),
  classList: {
    toggle(name, enabled) {
      if (enabled) this.owner.classes.add(name);
      else this.owner.classes.delete(name);
    },
    owner: null
  },
  setAttribute(name, value) { this.attributes[name] = value; }
}));
for (const card of cards) card.classList.owner = card;

const root = {
  querySelectorAll(selector) {
    assert.equal(selector, "[data-state]");
    return cards;
  },
  querySelector(selector) {
    assert.ok(elements[selector], "unexpected selector: " + selector);
    return elements[selector];
  }
};

const document = {
  querySelector(selector) {
    assert.equal(selector, "[data-fsm-demo]");
    return root;
  }
};

const source = fs.readFileSync("Documentation/Experience/fsm-api-experience.js", "utf8");
vm.runInNewContext(source, { document }, { filename: "fsm-api-experience.js" });

const click = selector => {
  const callback = elements[selector].listeners.click;
  assert.equal(typeof callback, "function", "missing click handler: " + selector);
  callback();
};

assert.equal(elements["[data-current]"].textContent, "Off");
assert.equal(elements["[data-requested]"].textContent, "Off");
assert.equal(elements["[data-tick]"].textContent, "0");
assert.ok(cards[0].classes.has("is-active"));
assert.ok(!cards[1].classes.has("is-active"));

click("[data-flip]");
assert.equal(elements["[data-current]"].textContent, "Off", "request must not transition before the host tick");
assert.equal(elements["[data-requested]"].textContent, "On");
assert.match(elements["[data-log]"].textContent, /has not advanced/);

click("[data-step]");
assert.equal(elements["[data-current]"].textContent, "On");
assert.equal(elements["[data-tick]"].textContent, "1");
assert.match(elements["[data-log]"].textContent, /onExit\(Off\)/);
assert.match(elements["[data-log]"].textContent, /onEnter\(On\) waits for the next tick/);
assert.ok(cards[1].classes.has("is-active"));

click("[data-step]");
assert.equal(elements["[data-tick]"].textContent, "2");
assert.match(elements["[data-log]"].textContent, /onEnter\(On\)/);
assert.match(elements["[data-log]"].textContent, /onUpdate\(On\)/);

click("[data-flip]");
assert.equal(elements["[data-current]"].textContent, "On");
assert.equal(elements["[data-requested]"].textContent, "Off");
click("[data-step]");
assert.equal(elements["[data-current]"].textContent, "Off");
assert.equal(elements["[data-tick]"].textContent, "3");

click("[data-reset]");
assert.equal(elements["[data-current]"].textContent, "Off");
assert.equal(elements["[data-requested]"].textContent, "Off");
assert.equal(elements["[data-tick]"].textContent, "0");
assert.match(elements["[data-log]"].textContent, /Reset to the initial state/);

console.log("PASS: LightSwitch experience requests, host ticks, deferred entry, reverse transition, and reset.");
