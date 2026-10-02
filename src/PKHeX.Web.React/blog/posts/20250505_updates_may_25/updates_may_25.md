---
title: Recent Updates to PKHeX.Web - May 2025
date: 2025-05-05
permalink: "/posts/pkhex-web-updates-may-2025/"
tags:
  - update
  - release
  - legality
  - privacy
  - bugfix
layout: post.njk
---

This post outlines some recent updates that have been deployed to PKHeX.Web. These changes aim to improve core functionality, enhance user privacy, and resolve known issues.

## Core Logic Updates

PKHeX.Web now incorporates more recent updates from the underlying libraries it depends on:

*   **PKHeX.Core:** The core legality logic and Pokémon data structures have been updated based on recent contributions from Kurt and the community. This ensures that PKHeX.Web benefits from the latest checks and entity information available in the main PKHeX project.
*   **AutoLegalityMode:** We have also integrated updates from AutoLegalityMode. This brings in the latest enhancements and fixes related to the automatic legality application used within PKHeX.Web.

These updates help maintain compatibility and ensure the legality checking remains current.

## Cookie Consent Implementation

To better align with privacy regulations such as GDPR and CCPA, we have added a cookie consent mechanism to PKHeX.Web. Upon visiting the site, users will now be presented with a banner requesting consent for the use of cookies. This allows users more transparency and control over their data when using the application. Cookies are primarily used for essential site functionality like session management.

## Bug Fix: Let's Go Eevee Save Export

An issue was identified where attempting to save modifications to a Pokémon: Let's Go, Eevee! save file would sometimes fail silently. The export process would not complete, and no error message was displayed to the user, leading to confusion.

This bug has been addressed in the latest update. Exporting modified Let's Go, Eevee! save files should now work as expected.

---

These updates are now live on PKHeX.Web. Any feedback on our GitHub repo is appreciated :)
