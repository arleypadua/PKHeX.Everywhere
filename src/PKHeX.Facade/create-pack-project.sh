#!/bin/bash

script_dir=$(dirname "$0")

# Define the input and output files
input_file="$script_dir/PKHeX.Facade.csproj"
output_file="$script_dir/PKHeX.Facade.nupkg.csproj"

# Read the input file and modify it
awk '
/<\/Project>/ {
    print "  <ItemGroup>";
    print "    <PackageReference Include=\"Teronis.MSBuild.Packaging.ProjectBuildInPackage\" Version=\"1.0.0\">";
    print "      <PrivateAssets>all</PrivateAssets>";
    print "      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>";
    print "    </PackageReference>";
    print "  </ItemGroup>";
}
/<ProjectReference / {
    sub(/\/>/, " PrivateAssets=\"all\" />");
}
{ print }
' "$input_file" > "$output_file"

echo "Modified project file created: $output_file"