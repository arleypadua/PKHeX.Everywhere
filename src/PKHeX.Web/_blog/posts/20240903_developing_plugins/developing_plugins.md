---
title: Developing Plugins for PKHeX.Web - SDK Guide
date: 2024-09-03
tags:
  - pkhex
  - sdk
  - plugins
  - development
  - tutorial
layout: post.njk
permalink: /posts/pkhex-web-plugin-sdk-guide/
---

This guide provides information for developers interested in extending PKHeX.Web by creating their own plugins using the official SDK.

## What is the Plug-in SDK?

The PKHeX.Web Plug-in SDK is a .NET package that provides the necessary tools, interfaces, and base classes to develop custom plug-ins that integrate with the PKHeX.Web application.

## Pre-requisites

Before you start, ensure you have the following installed:

*   [.NET 8 SDK or later](https://dotnet.microsoft.com/en-us/download)
*   [Visual Studio Code](https://code.visualstudio.com/Download) (or another preferred IDE/editor for .NET development)

## Getting started

Follow these steps to create a basic plug-in project.

1.  **Create a .NET project**

    Open your terminal or command prompt:

    ```sh
    # create a folder for your plugin
    mkdir PlugIn.Demo

    # change the directory
    cd PlugIn.Demo

    # create a razor class lib
    # (can also be a classlib if your plug-in doesn't need to render UI components)
    dotnet new razorclasslib

    # open VS Code (optional)
    code .
    ```

2.  **Configure your project file**

    Edit your project file (`.csproj` file, e.g., `./PlugIn.Demo.csproj` in this demo) and add the following properties within the `<PropertyGroup>` section:

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Razor">

      <PropertyGroup>
        <!-- Default properties like TargetFramework -->
        <TargetFramework>net8.0</TargetFramework> <!-- Or your target version -->
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>

        <!-- Add the following lines -->
        <DebugType>portable</DebugType>
        <EmbedAllSources>true</EmbedAllSources>
        <PublishSingleFile>true</PublishSingleFile>
        <Version>1.0.0</Version>
        <!-- Until here -->

      </PropertyGroup>

      <!-- Other items like PackageReference might be here -->

    </Project>
    ```
    *(Ensure you keep existing relevant properties like `<TargetFramework>`)*. These settings help ensure the plugin is packaged correctly for PKHeX.Web.

3.  **Add the SDK package to your project**

    Add the PKHeX.Web Plugins SDK NuGet package. You can find the latest version on [NuGet](https://www.nuget.org/packages/PKHeX.Web.Plugins).

    ```sh
    dotnet add package PKHeX.Web.Plugins
    ```

    A successful command will output logs similar to this:

    ```
      Determining projects to restore...
    info : Adding PackageReference for package 'PKHeX.Web.Plugins' into project '...'.
    info :   GET https://api.nuget.org/v3/registration5-gz-semver2/pkhex.web.plugins/index.json

    ... sequence omitted

    info : PackageReference for package 'PKHeX.Web.Plugins' version 'X.Y.Z' added to file './PlugIn.Demo.csproj'.
    info : Writing assets file to disk. Path: .../obj/project.assets.json
    log  : Restored .../PlugIn.Demo.csproj (in X.YZ sec).
    ```

4.  **Add your plug-in manifest class**

    Create a new C# class file (e.g., `PlugInDemo.cs`). This class must inherit from the abstract `Settings` class provided by the SDK. This class defines metadata, configuration options, and default feature states for your plugin.

    ```csharp
    using PKHeX.Web.Plugins;
    using PKHeX.Web.Plugins.Common; // Required for SettingValue

    namespace PlugIn.Demo; // Use your project's namespace

    public class PlugInDemo : Settings
    {
        // Constructor calling the base class with the Manifest
        public PlugInDemo() : base(Manifest)
        {
           // Optionally define settings that users can configure.
           // Keys are strings, values use specific SettingValue types.
           // this["Some boolean setting"] = new SettingValue.BooleanValue(true);
           // this["Some integer setting"] = new SettingValue.IntegerValue(1);
           // this["Some string setting"] = new SettingValue.StringValue("a default value");
           // this["Some file setting"] = new SettingValue.FileValue([], string.Empty); // For file uploads

           // Optionally define which features (hooks) are enabled by default.
           // If none are specified here, all features start disabled.
           // EnabledByDefault<DemoAction>();
        }

        // Static manifest defining plugin metadata
        private static readonly PlugInManifest Manifest = new PlugInManifest(
            PlugInName: "Demo Plug-In",
            Description: "A simple demonstration plug-in.");
            // Optional parameters: ProjectUrl, Information
    }
    ```
    See the [Settings](#settings) section below for more details.

5.  **Add a feature (Hook Implementation)**

    Create classes that implement one or more "hook" interfaces provided by the SDK. These hooks allow your code to execute at specific points within PKHeX.Web or add UI elements.

    For this example, we'll implement `IPokemonEditAction` to add a button to the Pokémon editor:

    ```csharp
    using PKHeX.Core; // Required for Pokemon type
    using PKHeX.Web.Plugins;
    using PKHeX.Web.Plugins.Common; // Required for Outcome
    using System.Threading.Tasks; // Required for Task

    namespace PlugIn.Demo; // Use your project's namespace

    public class DemoAction : IPokemonEditAction // Implement the desired hook interface
    {
        // Text displayed on the button
        public string Label => "Demo Action";

        // Description shown on hover or in settings
        public string Description => "Demo action available when editing a pokemon";

        // Method executed when the button is clicked
        public Task<Outcome> OnActionRequested(Pokemon pokemon)
        {
            // Example: Show an info notification
            return Outcome.Notify(
                message: "Action executed",
                description: $"Action executed for pokemon: {pokemon.Species.Name}",
                type: Outcome.Notification.NotificationType.Info).Completed();

            // Other outcomes like Outcome.Void or Outcome.Page are possible
        }
    }
    ```
    Check the [Plug-In Hooks](#plug-in-hooks) section for more available hooks.

6.  **Publishing the package**

    Once your code is complete, build your project in `Release` configuration:

    ```sh
    dotnet publish -c Release
    ```
    This will typically generate a single `.dll` file in a path like `bin/Release/net8.0/publish/`. This DLL is your packaged plug-in.

    Currently, there isn't an automated marketplace for publishing. To make your plug-in available to other users via the default PKHeX.Web sources, please [open an issue](https://github.com/arleypadua/PKHeX.Everywhere/issues/new) in the main repository including:
    *   A description of the plug-in and its functionality.
    *   A link to the plug-in's code repository (e.g., GitHub).
