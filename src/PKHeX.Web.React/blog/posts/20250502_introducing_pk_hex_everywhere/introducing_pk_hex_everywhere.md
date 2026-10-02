---
title: "Introducing PKHeX.Everywhere: A Web-Based Pokémon Save Editor"
date: 2025-05-02
permalink: "/posts/introducing-pk-hex-everywhere/"
layout: post.njk
tags:
  - project
  - pokemon
  - pkhex
  - blazor
  - webassembly
  - open_source
---

This post introduces a project I've been developing called PKHeX.Everywhere. The source code and issue tracking can be found on GitHub: [https://github.com/arleypadua/PKHeX.Everywhere](https://github.com/arleypadua/PKHeX.Everywhere).

**Origins: A Cross-Platform Terminal Editor**

The project originated from my use of PKHeX, the well-regarded Pokémon save editing tool created by Kurt ([kwsch](https://github.com/kwsch)). My initial goal for PKHeX.Everywhere was to build a version that could run in a terminal, aiming for cross-platform compatibility (Windows, macOS, Linux) without requiring a graphical user interface. This involved exploring techniques like Aspect-Oriented Programming (AOP) to adapt the original codebase for a terminal environment.

**Evolution: Moving to the Web**

As development progressed, the idea shifted towards broader accessibility. The project evolved into a web application, aiming to make save editing possible on any device with a modern web browser.

Using Blazor and WebAssembly (.NET), PKHeX.Everywhere ports the core functionality of PKHeX to run client-side in the browser. This allows users to edit save files on desktops or mobile devices through a responsive web interface, without needing separate installations for each platform.

**Core Features**

PKHeX.Everywhere is extensible with builtin plug-ins:

1.  **Nuzlocke:** It includes functionalities like helpers aimed at Nuzlocke players.
2.  **Integrated Emulator:** An embedded emulator is included, which can read the save file state after an in-game save, potentially streamlining the play-edit cycle.
3.  **Auto Legality Mode:** It incorporates PKHeX's legality checking logic, alongside [santacrab2](https://github.com/santacrab2)'s fork of Auto-Legality Mode. Which is updated at least once a month.

**The Development Process: Learning and Project Management**

Developing PKHeX.Everywhere has been a useful learning experience. It provided a practical context for working more deeply with Blazor and WebAssembly, understanding how to integrate a substantial .NET library like PKHeX into a web environment. Managing it as an open-source project has also offered insights into that process. Overall, it's been a nice ride exploring these technologies.

**Open Source and Contributions**

PKHeX.Everywhere is intended as a tool for the Pokémon community, building upon the foundation laid by the original PKHeX.

The project is open source ([https://github.com/arleypadua/PKHeX.Everywhere](https://github.com/arleypadua/PKHeX.Everywhere)). Contributions are welcome, whether through feedback, feature suggestions, issue reporting, or proposing changes via forks and pull requests.

**Next Steps**

Development is ongoing. Future work involves refining the existing features, adding new ones, and maintaining compatibility with updates from the core PKHeX project. Feedback and contributions will help guide its direction.

You can explore the repository to see the code or build it yourself.