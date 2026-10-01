# Plug-ins move into the Engine

Blazor will be removed completely, so plug-ins need an SDK v2 without Razor. Plug-ins stay .NET assemblies, and the Engine loads and runs them. React renders their UI from data. Context: pkhex-web/issue-tracker#43 and pkhex-web/issue-tracker#54.

## Why plug-ins stay in .NET

AutoLegality depends on ALM, which is C#. Every other plug-in also edits the save through PKHeX in C#. Keeping them as .NET assemblies means their logic moves over as it is, and only their UI changes.

## Plug-in UI is data

A plug-in describes its UI as data, and React renders it: action labels, disabled state, settings, and outcomes such as "notify" or "open page". No plug-in references Razor or React to add a menu entry or a button.

A plug-in that needs a custom page can ship an optional JS module. LiveRun's page becomes one.

## Engine events are the seam

Typed Engine events connect plug-ins to the save. During the migration, Blazor subscribes to them and forwards them to v1 plug-ins. With v2, the Engine's plug-in host subscribes instead, and Blazor drops out.

## No external migration

Only the in-repo plug-ins exist: AutoLegality, LiveRun and Nuzlocking. We move them ourselves, so v2 doesn't need a compatibility layer for v1.

## Ordering

The v2 spec comes before the first React page that renders plug-in actions (Home or the editor), and before the layout swap from [ADR 0002](0002-react-pages-inside-blazor-over-a-dotnet-engine.md).

## Amendment: SDK v2

Context: pkhex-web/issue-tracker#63.

The v2 contracts live in `PKHeX.Everywhere.PlugIns`, with no Razor, Blazor or ASP.NET Core reference. The plug-in host, `PKHeX.Everywhere.Engine.PlugIns`, loads v2 assemblies from bytes and runs their hooks. The Engine references neither. The host reads an assembly's references before loading it, and it reports an assembly built against `PKHeX.Web.Plugins` as v1 instead of loading it. The host runs SDK 2 only.

### Distribution

Each `PublishedVersions` entry in a source manifest declares the SDK major it targets as `sdk`. An entry without `sdk` counts as 1. The app installs the newest version whose `sdk` it supports, so an older app never picks up a version it can't load. CI publishes `PKHeX.Everywhere.PlugIns` to NuGet.

### Page modules

A plug-in declares its pages as `{ path, module, title?, layout }`. `module` names a JS module embedded in the plug-in assembly. The module exports `mount(element, ctx)`, which returns an `unmount` function. `ctx` is `{ plugInId, theme, getSave(), getSetting(key), loadSave(bytes, fileName), navigate(url) }`. `getSave()` returns `{ bytes, fileName, version }`, where `version` lets a page such as LiveRun pick the ROM for the save. `getSetting` returns file settings as bytes. The types ship as `@pkhex-everywhere/plugin-sdk`. The app imports the module from a Blob URL, so a page works on reload and from a link.

### Stored v1 plug-ins

Players' browsers hold the DLL bytes of every plug-in they installed. On startup, the app updates a stored v1 plug-in to the newest v2 version from its source and keeps its enabled state, hook toggles and settings. If the update fails, the plug-ins page marks it "needs reinstall".

## Rejected alternatives

- **Plug-ins as JS/React modules.** AutoLegality and every existing plug-in would need a rewrite, and AutoLegality would lose ALM.
