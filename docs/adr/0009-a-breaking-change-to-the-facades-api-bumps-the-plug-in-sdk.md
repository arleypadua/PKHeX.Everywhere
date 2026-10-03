# A breaking change to the Facade's API bumps the plug-in SDK

Plug-ins are .NET assemblies compiled against `PKHeX.Everywhere.PlugIns`, which exposes the Facade. A plug-in built against an older Facade fails when the app loads it or calls a member that has changed. So a breaking change to the Facade's public API bumps the plug-in SDK major. Context: pkhex-web/issue-tracker#162.

## Decision

- `src/PKHeX.Facade/PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` list the Facade's public API. `Microsoft.CodeAnalysis.PublicApiAnalyzers` fails the build when the API changes and the files don't.
- New members go in `PublicAPI.Unshipped.txt`. Adding to the API is not breaking. Removing or changing a line in `PublicAPI.Shipped.txt` is.
- The files include the PKHeX types in Facade signatures, so a PKHeX fork update that changes one fails the build too. It breaks plug-ins the same way.
- A breaking change bumps the plug-in SDK major. Plug-in SDK detection tells majors apart by the referenced `PKHeX.Everywhere.PlugIns` assembly version, not only its name.
- The host supports the new major once the change ships. The app marks installed plug-ins on the old major `needsReinstall` and replaces them on start with the manifest's newest version for the new major.
- The plug-in release workflow rebuilds and publishes AutoLegality, LiveRun and Nuzlocking with the new `Sdk` before the app that needs it deploys. Otherwise the app flags installed plug-ins for reinstall with nothing to install.

## Why

Without the files, a Facade change could break every installed plug-in and nobody would notice until it failed in a user's browser. The files make the change show in review, and the SDK bump makes the app replace plug-ins instead of failing silently.

## Rejected alternatives

- **Keep the Facade's API stable.** The Facade is the domain model, and it changes with the features. Freezing it would push domain logic into the Engine, against [ADR 0004](0004-domain-logic-lives-in-the-facade.md).
- **A separate plug-in API over the Facade.** Plug-ins such as AutoLegality need most of the Facade, so the wrapper would be a second copy of it to maintain.
