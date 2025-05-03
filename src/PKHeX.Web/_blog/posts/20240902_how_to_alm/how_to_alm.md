---
title: How to use Auto-Legality Mode (ALM) in PKHeX.Web
date: 2024-09-02
tags:
  - pkhex
  - tutorial
  - plugins
  - alm
  - auto-legality
layout: post.njk
permalink: /posts/pkhex-web-plugin-auto-legality-mode/
---

The Auto-Legality Mode (ALM) plug-in for PKHeX.Web serves as a proxy for applying the [auto-legality mode logic](https://github.com/santacrab2/PKHeX-Plugins) originally maintained by [@santacrab2](https://github.com/santacrab2) for the desktop version of PKHeX.

This post details the configuration options and features available within this plug-in.

## Configurations

These settings control the core behavior of the ALM plug-in.

### Timeout (seconds)

*   **Default:** `2 seconds`

Auto-legality mode attempts to find a valid set of parameters to make a Pokémon legal. This process can sometimes take a noticeable amount of time. This setting establishes the maximum number of seconds the plug-in will spend trying to legalize a Pokémon during a single attempt. If the threshold is reached before a legal version is found, the execution stops, and no changes are applied.

### Force lvl 100 from 50

*   **Default:** `disabled`

When enabled, this option assumes that any Pokémon encountered at level 50 is intended to be a level 100 competitive Pokémon. If you edit a Pokémon that is currently level 50, the plug-in will automatically change its level to 100 before applying legality checks.

## Features

These features determine *when* the auto-legality logic is triggered. You can enable or disable them individually based on your workflow.

### Try to make your pokemon legal on every change.

*   **Default:** `disabled`

If enabled, the auto-legality process will be triggered automatically every time you modify any detail (like moves, IVs, EVs, ability, etc.) while editing a Pokémon in the interface. Be mindful that this can introduce slight delays during editing due to the processing time.

### Adds a button to legalize a pokemon when editing it.

*   **Default:** `enabled`

This feature adds a dedicated "Make Legal" button to the Pokémon editing interface. The auto-legality logic will only run when you explicitly click this button.

<img width="200" alt="image" src="https://github.com/user-attachments/assets/090e68b8-83c1-4520-80ee-c65a106291be">

Upon clicking the button, you should see a confirmation pop-up indicating whether the process was successful or timed out.

<img width="382" alt="image" src="https://github.com/user-attachments/assets/f234567b-aabf-4eb5-9713-590fce01b378">

### Try to make your pokemon legal whenever a pokemon is saved

*   **Default:** `disabled`

When this feature is enabled, the plug-in will automatically attempt to apply auto-legality just before the Pokémon data is saved (i.e., when you click the "Save" button in the editor).
