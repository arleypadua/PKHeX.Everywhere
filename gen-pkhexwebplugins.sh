#!/bin/bash

json_url="https://raw.githubusercontent.com/pkhex-web/plugins-source-assets/main/pkhexwebplugins.json"
json_file="plugins/pkhexwebplugins.json"

mkdir -p plugins
everywhere_sdk=$(xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" src/PKHeX.Everywhere.PlugIns/PKHeX.Everywhere.PlugIns.csproj | cut -d. -f1)
curl -s -o "$json_file" "$json_url"

for project in src/PKHeX.Web.Plugins.*/*.csproj; do
    if [ -f "$project" ]; then
        # Get the project name and version
        project_name=$(basename "$(dirname "$project")")
        version=$(xmllint --xpath "string(//Project/PropertyGroup/Version)" "$project")
        if grep -q 'PKHeX\.Everywhere\.PlugIns' "$project"; then sdk=$everywhere_sdk; else sdk=1; fi
        
        # Check if the plugin already exists in the JSON file
        plugin_exists=$(jq --arg id "$project_name" '.PlugIns[] | select(.Id == $id)' "$json_file")
        
        if [ -n "$plugin_exists" ]; then
            # Plugin exists, check if the version is already published
            version_exists=$(echo "$plugin_exists" | jq --arg version "$version" '.PublishedVersions[] | select(. == $version or (type == "object" and .Version == $version))')

            if [ -z "$version_exists" ]; then
                # Add the new version to the PublishedVersions
                jq --arg id "$project_name" --arg version "$version" --argjson sdk "$sdk" '
                    (.PlugIns[] | select(.Id == $id) | .PublishedVersions) += [{"Version": $version, "Sdk": $sdk}]
                ' "$json_file" > tmp.json && mv tmp.json "$json_file"

                echo "Added new version $version (SDK $sdk) to existing plugin $project_name."
            else
                echo "Version $version already exists for plugin $project_name."
            fi
        else
            # Plugin does not exist, add new plugin with the version
            jq --arg id "$project_name" --arg version "$version" --argjson sdk "$sdk" '
                .PlugIns += [{"Id": $id, "PublishedVersions": [{"Version": $version, "Sdk": $sdk}]}]
            ' "$json_file" > tmp.json && mv tmp.json "$json_file"

            echo "Added new plugin $project_name with version $version (SDK $sdk)."
        fi
    fi
done