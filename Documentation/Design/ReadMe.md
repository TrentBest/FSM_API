
# MyVR: A Decoupled and Distributed Platform for Digital Experiences

MyVR is a visionary platform designed to create immersive, platform-agnostic digital experiences. By leveraging the power of Finite State Machines (FSMs), the core behavior of software is fully decoupled from its hosting environment, allowing for unparalleled portability and reusability. This architecture is a layered system, building from a foundational API up to a fully realized cloud-based digiverse.

---

## System Architecture Overview

Click on any component in the diagram below to explore its detailed documentation.

```mermaid
graph TD
    subgraph Core Foundation
        FSM_API[FSM_API: Foundational Behavior Logic]
    end

    subgraph Unity Ecosystem
        UnityEcosystem[Unity Ecosystem: Packages & AnyApp]
    end

    subgraph Cloud Infrastructure
        MyVR[MyVR: Cloud-based Digiverse]
    end

    linkStyle 0 href "./FSM_API/FSM_API.md" _self

```

-----

## Core Components: Executive Summary

### The Core Foundation: FSM\_API

The **FSM\_API** is the bedrock of the entire MyVR system. It's a foundational technology that uses the mathematical principles of Finite State Machines to normalize and encapsulate software behavior. The core mission of this API is to completely decouple software logic from the platform it runs on, ensuring that a single set of behaviors can be deployed anywhere, from local devices to the cloud.

### The Unity Ecosystem: Packages & AnyApp

This layer is the bridge between the core FSM logic and practical, deployable applications. The **Unity Ecosystem** consists of a library of **Unity Asset Packages**, each providing a reusable, FSM-based implementation of a common software pattern or game mechanic. These packages are consumed by **AnyApp**, a minimal, self-loading application and editor that acts as the local host for any MyVR experience. This layer ensures that behaviors can be efficiently distributed and managed using Unity's **Addressables system**.

### The Cloud Infrastructure: MyVR

**MyVR** is the ultimate destination—a fully cloud-based "digiverse" that abstracts and unifies all experiences. It's a scalable server architecture that will host and connect all of the experiences contained within it. The digiverse is organized into **Domains**, which are user-facing, immersive environments that serve as containers for specific applications and content. This layer provides the final abstraction, allowing users to move seamlessly between experiences within a unified digital world.
