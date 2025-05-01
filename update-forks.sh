#!/bin/bash

# --- PKHeX Plugins ---

cd external/PKHeX-Plugins || { echo "Sub-repo not found"; exit 1; }
git remote | grep -q upstream || git remote add upstream https://github.com/santacrab2/PKHeX-Plugins
git fetch upstream
git checkout cherrytree

# Rebase onto upstream/cherrytree
if git rebase upstream/cherrytree; then
  git push --force
else
  echo "Rebase of PKHeX-Plugins failed, resolve local conflicts."
fi 

# Move back to main repo root
cd ../..

# --- PKHeX Core ---
cd external/PKHeX || { echo "Sub-repo not found"; exit 1; }
git remote | grep -q upstream || git remote add upstream https://github.com/kwsch/PKHeX
git fetch upstream
git checkout master

if git rebase upstream/master; then
  git push --force
else
  echo "Rebase PKHeX.Core failed, resolve local conflicts."
fi