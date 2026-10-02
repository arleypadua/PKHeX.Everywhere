---
title: How to use the Live Run Plug-in (Browser Emulator)
date: 2024-09-02
tags:
  - pkhex
  - tutorial
  - plugins
  - emulator
  - live-run
layout: post.njk
permalink: /posts/pkhex-web-plugin-live-run-emulator/
---

The Live Run plug-in for PKHeX.Web offers a fully integrated browser-based emulator, allowing you to load your edited save file directly into a game environment to see how your changes look in practice.

Currently, this feature supports save files compatible with [mGBA](https://mgba.io/) (Game Boy Advance and Game Boy Color games). Support for NDS games via [DeSmuME](https://desmume.org/) integration is planned for the future.

A key aspect of this integration is its ability to detect when you save your progress *within the emulator*. When this happens, PKHeX.Web will prompt you, asking if you want to load this newly saved state back into the editor.

*_Disclaimer: We do not endorse or support piracy. Please only use ROM files that you have legally obtained from cartridges you own._*

## Configurations

These settings control the emulator's behavior and specify the necessary game ROMs.

### Show frame count

*   **Default:** `enabled`

If enabled, the emulator display will include a running frame counter. This can be useful for specific tasks like RNG manipulation.

### ROM Files (Required)

You must provide the corresponding ROM file for the type of save game you intend to load into the emulator. Upload your legally obtained ROM file for each game type you plan to use:

*   **Emerald ROM:** For Pokémon Emerald saves.
*   **FireRed/LeafGreen ROM:** For Pokémon FireRed or LeafGreen saves.
*   **Red/Blue/Yellow ROM:** For Pokémon Red, Blue, or Yellow saves.
*   **Gold/Silver/Crystal ROM:** For Pokémon Gold, Silver, or Crystal saves.
*   **Ruby/Sapphire ROM:** For Pokémon Ruby or Sapphire saves.

To upload a ROM, navigate to the plug-in's configuration page and use the `Upload` button next to the corresponding game type.

<img width="388" alt="image" src="https://github.com/user-attachments/assets/0126f9fc-967e-4b36-818a-cfbedad8956e">

## Features

This plug-in primarily adds the core emulation functionality.

### Adds a button to play the game with the current opened save game

*   **Default:** `enabled`

This feature adds the main "Play with this save" button to the PKHeX.Web [home page](https://pkhex-web.github.io/). This button launches the emulator with the save file currently loaded in the editor.

The button will appear disabled if:
a) The currently loaded save game is from an unsupported system (e.g., NDS, Switch).
b) You haven't uploaded the required ROM file for the loaded save's game type in the plug-in configuration.

## How to Use the Live Run Emulator

1.  **Upload ROM:** Go to the Live Run plug-in configuration page (accessible from the left menu) and upload your legally obtained ROM file(s) using the `Upload` buttons.
2.  **Edit Save:** Load the save file you want to test into PKHeX.Web and make any desired edits.
3.  **Launch Emulator:** Navigate back to the [home page](https://pkhex-web.github.io/) and click the `Play with this save` button.

    <img width="481" alt="image" src="https://github.com/user-attachments/assets/114e9e6a-606a-40f5-bb03-00d50d2fdcd7">

4.  **Verify Data:** The emulator will launch. Load your save within the game and verify that your edits are reflected correctly.

    <img width="240" alt="image" src="https://github.com/user-attachments/assets/7cc5e6f7-b670-401f-8d89-5a89d300a658">

5.  **Play and Save:** Play the game as needed. When you want to save progress made within the emulator back to PKHeX.Web, use the game's internal save function.

    <img width="239" alt="image" src="https://github.com/user-attachments/assets/3c475f58-b64f-4928-8a3b-9cf0d29efb3f">

6.  **Update PKHeX.Web:** Immediately after the in-game save completes, PKHeX.Web will detect it and show a prompt asking if you want to load this new save data.

    <img width="245" alt="image" src="https://github.com/user-attachments/assets/2a4e1bdd-85e4-4616-9441-79c0fb7aa444">

    *   Clicking `Cancel` will dismiss the prompt, and the save data in PKHeX.Web will remain unchanged. You can continue playing in the emulator.
    *   Clicking `OK` will load the save data from the emulator into PKHeX.Web, replacing the previously loaded data. You will be automatically redirected back to the PKHeX.Web home page, and the emulator session will close.
