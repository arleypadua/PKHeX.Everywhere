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

## Rejected alternatives

- **Plug-ins as JS/React modules.** AutoLegality and every existing plug-in would need a rewrite, and AutoLegality would lose ALM.
