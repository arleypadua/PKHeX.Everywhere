---
title: How to use Nuzlocking plugin
date: 2024-09-02
tags:
  - pkhex
  - tutorial
  - plugins
  - nuzlocke
layout: post.njk
permalink: /posts/pkhex-web-plugin-nuzlocke-helpers/ # Specific permalink for this post
---

PKHeX.Web includes a plug-in offering a couple of helper features specifically designed for players undertaking Nuzlocke challenges.

This post outlines the available configurations and features within the Nuzlocke Helpers plug-in.

## Configurations

These settings adjust how the "Edging" feature behaves.

### AmountOfExperienceToEdge

This defines the specific amount of experience points that will be subtracted from a level threshold when using the "Edge" feature. It determines how close to leveling up the Pokémon will be placed.

### EdgeOnPreviousLevel

This toggle controls the target level threshold for the "Edge" feature.

*   When `enabled`, edging will set the Pokémon's experience just below the threshold required to reach its *current* level (effectively putting it at the very end of the *previous* level). It subtracts `AmountOfExperienceToEdge` from the experience required to reach the current level.
*   When `disabled`, edging will set the Pokémon's experience just below the threshold required to reach the *next* level. It subtracts `AmountOfExperienceToEdge` from the experience required to gain the next level.

## Features

These are the specific actions provided by the plug-in, which can be toggled on or off.

### Will edge a pokemon to the current level

*   **Default:** `enabled`

Adds a button to the Pokémon editing interface allowing you to "edge" a Pokémon – setting its experience just shy of a level-up. The exact behavior depends on the `AmountOfExperienceToEdge` and `EdgeOnPreviousLevel` configurations described above.

**Example:**

If a Pokémon is level 10, `EdgeOnPreviousLevel` is `enabled`, and you click the `Edge` button, the Pokémon's experience will be set to the maximum value for level 9, minus the `AmountOfExperienceToEdge` value. It will be very close to reaching level 10.

<img width="232" alt="image" src="https://github.com/user-attachments/assets/4213a41d-a271-40fd-b517-89b8794c0d5c">

### Adds a button to zero all EVs when editing a Pokemon

*   **Default:** `disabled`

Some Nuzlocke rulesets restrict or forbid the use of Effort Values (EVs) to increase the challenge. Enabling this feature adds a "Zero EVs" button.

This button appears at the bottom of the `Stats` tab when editing a Pokémon. Clicking it will set all EV values for that Pokémon to 0.

<img width="662" alt="image" src="https://github.com/user-attachments/assets/b604e2cb-195c-413b-b92d-d809d73993ce">

<img width="337" alt="image" src="https://github.com/user-attachments/assets/7f417f22-e3ce-4a93-a4d0-143fd5af1d89">

### Adds a button on the home page to give max rare candies

*   **Default:** `enabled`

This feature adds a convenient button directly on the PKHeX.Web [home page](https://pkhex-web.github.io/). Clicking this button automatically locates the appropriate item pouch in your save file's bag and adds the maximum possible number of Rare Candies allowed by the game.

<img width="501" alt="image" src="https://github.com/user-attachments/assets/d18dcae2-2c0f-48d0-b25e-4a60fed7726c">

After clicking, a confirmation message will appear indicating the change was successful.

<img width="415" alt="image" src="https://github.com/user-attachments/assets/b31e9aa4-5619-41fc-8be2-329259373815">
