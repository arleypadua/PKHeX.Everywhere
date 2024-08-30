#!/bin/bash

for project in src/PKHeX.Web.Plugins.*/*.csproj; do
    if [ -f "$project" ]; then
        # Get the project name and version
        project_name=$(basename "$(dirname "$project")")
        version=$(xmllint --xpath "string(//Project/PropertyGroup/Version)" "$project")

        # Build the project in Release mode
        echo "Building $project_name (v$version) in Release mode..."
        dotnet build "$project" -c Release

        # Find the DLL and copy it to the desired output directory
        dll_path=$(find "$(dirname "$project")/bin/Release" -name "$project_name.dll" -print -quit)
        if [ -f "$dll_path" ]; then
            output_dir="./plugins/$project_name/$version"
            mkdir -p "$output_dir"
            cp "$dll_path" "$output_dir"
            echo "Copied $dll_path to $output_dir"
        else
            echo "DLL not found for $project_name"
        fi
    fi
done