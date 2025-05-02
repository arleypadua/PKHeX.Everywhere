---
title: How to Install and Configure Plugins in PKHeX.Web
date: 2025-05-03
permalink: "/posts/install_configure_plugins/"
tags:
  - pkhex
  - tutorial
  - plugins
layout: post.njk
---

PKHeX.Web offers some basic functionality via a default set of plug-ins. These cover common use cases, but the tool is designed to be extensible.

It also publishes an SDK, in case you have an idea to develop your own plugin to tailor the experience further.

This guide will walk you through installing and configuring plugins in PKHeX.Web.

## Installing a Plug-in

PKHeX.Web uses the concept of "plug-in sources". Think of these as collections or repositories of plugins that can be installed in the tool. By default, PKHeX.Web ships with a source containing several core plugins: ALM (auto-legality mode), a set of Nuzlocke quick actions, and a fully integrated browser emulator.

To install a new plug-in:

1.  Navigate to the "[Manage Plug-Ins](https://pkhex-web.github.io/plugins)" page. You can access this from the left-hand menu:

    ![PKHeX.Web Left Menu showing Manage Plug-Ins link](https://github.com/user-attachments/assets/ee8b9ea7-c8a4-411c-934c-7fafa0036113 "Manage Plug-Ins Menu Item")

2.  On the Manage Plug-Ins page, available plugins will appear under the `Discover` section:

    ![Discover section in Manage Plug-Ins page showing available plugins](https://github.com/user-attachments/assets/b286c104-a652-4051-b5df-7deea6d18f4b "Discover Plugins Section")

3.  Find the plug-in you wish to add and click the `Install` button next to it. After installation, you will usually be forwarded directly to that plug-in's configuration page.

## Configuring a Plug-in

Once a plug-in is installed, you'll need to configure it. As mentioned, you might be redirected there automatically after installation. Alternatively, you can access the configuration page for any installed plug-in by selecting it from the left-hand menu:

![PKHeX.Web Left Menu showing an installed plugin](https://github.com/user-attachments/assets/49f12416-d0d3-416b-b839-caa6dc199161 "Installed Plugin in Menu")

When you open a plug-in's configuration page, you'll find settings specific to that particular plug-in. However, nearly all plug-ins will include at least these two common elements:

*   **A master toggle** to `enable` or `disable` the entire plug-in:

    ![Enable/Disable toggle for a PKHeX.Web plugin](https://github.com/user-attachments/assets/1cd43acc-7fb2-4567-8e4f-9aef636e8621 "Plugin Enable/Disable Toggle")

*   **A list of features** provided by the plug-in, each with its own individual `enable`/`disable` toggle:

    ![List of features within a plugin, each with an enable/disable toggle](https://github.com/user-attachments/assets/eb3e2b6c-a3a4-491d-9dad-8146da2e1ff6 "Plugin Feature Toggles")

_For example, in the image above showing the Auto-Legality Mode plug-in configuration, the plug-in itself is `enabled`, but within its features, only `Adds a button to legalize a pokemon when editing it.` is currently active._

Make sure to enable the specific features you want to use within each plug-in after installing it.
