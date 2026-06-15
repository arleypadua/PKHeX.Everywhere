---
name: update-dependencies
description: Teaches how to update the external submodules/dependencies (PKHeX and PKHeX-Plugins) in this project, ensuring forks are updated against upstream and local patches/ProjectReferences are correctly applied.
---

# Updating Project Submodules / External Dependencies

Use this skill when you need to update the external dependencies/submodules (`external/PKHeX` and `external/PKHeX-Plugins`) to match their upstream repositories.

## Workflow

### 1. Update `external/PKHeX` (PKHeX.Core)
`PKHeX.Core` is updated by syncing the fork's `master` branch against the upstream `master` branch:

```bash
# Fetch changes from the original repository (upstream)
git -C external/PKHeX fetch upstream

# Ensure you are on the local master branch
git -C external/PKHeX checkout master

# Merge the latest upstream changes into master
git -C external/PKHeX merge upstream/master --no-edit

# Push the updated master branch to your fork (origin)
git -C external/PKHeX push origin master
```

### 2. Update `external/PKHeX-Plugins` (PKHeX.Core.AutoMod)
`PKHeX.Core.AutoMod` is updated by merging upstream changes into the local `cherrytree` branch, then applying (or preserving) the local project reference patch:

```bash
# Fetch changes from the upstream plugins repository
git -C external/PKHeX-Plugins fetch upstream

# Ensure you are on the local cherrytree branch
git -C external/PKHeX-Plugins checkout cherrytree

# Merge the latest upstream changes (usually from upstream/master)
git -C external/PKHeX-Plugins merge upstream/master --no-edit
```

#### Apply/Verify the Local Patch
After merging, verify that `PKHeX.Core.AutoMod/PKHeX.Core.AutoMod.csproj` correctly references the local project instead of a NuGet package reference. 

If the merge changed it back to a package reference, restore the local project reference:

```xml
<!-- In external/PKHeX-Plugins/PKHeX.Core.AutoMod/PKHeX.Core.AutoMod.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\PKHeX\PKHeX.Core\PKHeX.Core.csproj" />
  </ItemGroup>
</Project>
```

Once verified, commit the patch adjustment if necessary and push the changes:

```bash
git -C external/PKHeX-Plugins push origin cherrytree
```

### 3. Update Submodule Pointers in the Main Repository
Once submodules are updated and pushed, update the main repository to point to the new submodule commits:

```bash
# Add the updated submodule references in the main project
git add external/PKHeX external/PKHeX-Plugins

# Commit and push
git commit -m "update external submodules to latest upstream"
git push
```
