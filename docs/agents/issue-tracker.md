# Issue tracker: GitHub (separate repo)

Issues and specs for this repo live as GitHub issues in **`pkhex-web/issue-tracker`**, not in this code repo (`arleypadua/PKHeX.Everywhere`). Use the `gh` CLI for all operations, and **always pass `-R pkhex-web/issue-tracker`**: `gh` otherwise infers the code repo from `git remote -v`, which is the wrong place.

## Shared with pke-emu

The tracker also holds issues for [pkhex-web/pke-emu](https://github.com/pkhex-web/pke-emu), labelled `repo:pke-emu`. They belong to that repo: leave them out of every list, search and triage here by adding `--search "-label:repo:pke-emu"`. An issue for this repo never carries that label.

## Conventions

- **Create an issue**: `gh issue create -R pkhex-web/issue-tracker --title "..." --body "..."`. Use a heredoc for multi-line bodies.
- **Read an issue**: `gh issue view <number> -R pkhex-web/issue-tracker --comments`, filtering comments by `jq` and also fetching labels.
- **List issues**: `gh issue list -R pkhex-web/issue-tracker --state open --search "-label:repo:pke-emu" --json number,title,body,labels,comments --jq '[.[] | {number, title, body, labels: [.labels[].name], comments: [.comments[].body]}]'` with appropriate `--label` and `--state` filters.
- **Comment on an issue**: `gh issue comment <number> -R pkhex-web/issue-tracker --body "..."`
- **Apply / remove labels**: `gh issue edit <number> -R pkhex-web/issue-tracker --add-label "..."` / `--remove-label "..."`
- **Close**: `gh issue close <number> -R pkhex-web/issue-tracker --comment "..."`

## Cross-repo references

Issues and code live in different repos, so a bare `#42` is ambiguous:

- In this code repo (commits, PRs), refer to an issue as `pkhex-web/issue-tracker#42`. A PR that fixes an issue says `Fixes pkhex-web/issue-tracker#42`.
- In an issue, refer to a code PR or commit as `arleypadua/PKHeX.Everywhere#123`.

## Pull requests as a triage surface

**PRs as a request surface: no.** _(Set to `yes` if this repo treats external PRs as feature requests; `/triage` reads this flag.)_

When set to `yes`, PRs run through the same labels and states as issues. PRs live in the code repo (`arleypadua/PKHeX.Everywhere`), so the `gh pr` commands run without `-R`:

- **Read a PR**: `gh pr view <number> --comments` and `gh pr diff <number>` for the diff.
- **List external PRs for triage**: `gh pr list --state open --json number,title,body,labels,author,authorAssociation,comments` then keep only `authorAssociation` of `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR`, or `NONE` (drop `OWNER`/`MEMBER`/`COLLABORATOR`).
- **Comment / label / close**: `gh pr comment`, `gh pr edit --add-label`/`--remove-label`, `gh pr close`.

## When a skill says "publish to the issue tracker"

Create a GitHub issue in `pkhex-web/issue-tracker`.

## When a skill says "fetch the relevant ticket"

Run `gh issue view <number> -R pkhex-web/issue-tracker --comments`.

## Wayfinding operations

Used by `/wayfinder`. The **map** is a single issue with **child** issues as tickets. All in `pkhex-web/issue-tracker`.

- **Map**: a single issue labelled `wayfinder:map`, holding the Notes / Decisions-so-far / Fog body. `gh issue create -R pkhex-web/issue-tracker --label wayfinder:map`.
- **Child ticket**: an issue linked to the map as a GitHub sub-issue (`gh api` on `repos/pkhex-web/issue-tracker/issues/<map>/sub_issues`). Where sub-issues aren't enabled, add the child to a task list in the map body and put `Part of #<map>` at the top of the child body. Labels: `wayfinder:<type>` (`research`/`prototype`/`grilling`/`task`). Once claimed, the ticket is assigned to the driving dev.
- **Blocking**: GitHub's **native issue dependencies**, the canonical, UI-visible representation. Add an edge with `gh api --method POST repos/pkhex-web/issue-tracker/issues/<child>/dependencies/blocked_by -F issue_id=<blocker-db-id>`, where `<blocker-db-id>` is the blocker's numeric **database id** (`gh api repos/pkhex-web/issue-tracker/issues/<n> --jq .id`, _not_ the `#number` or `node_id`). GitHub reports `issue_dependencies_summary.blocked_by` (open blockers only, the live gate). Where dependencies aren't available, fall back to a `Blocked by: #<n>, #<n>` line at the top of the child body. A ticket is unblocked when every blocker is closed.
- **Frontier query**: list the map's open children (`gh issue list -R pkhex-web/issue-tracker --state open`, scoped to the map's sub-issues / task list), drop any with an open blocker (`issue_dependencies_summary.blocked_by > 0`, or an open issue in the `Blocked by` line) or an assignee; first in map order wins.
- **Claim**: `gh issue edit <n> -R pkhex-web/issue-tracker --add-assignee @me`, the session's first write.
- **Resolve**: `gh issue comment <n> -R pkhex-web/issue-tracker --body "<answer>"`, then `gh issue close <n> -R pkhex-web/issue-tracker`, then append a context pointer (gist + link) to the map's Decisions-so-far.
