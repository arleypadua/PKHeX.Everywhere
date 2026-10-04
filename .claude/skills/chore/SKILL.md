---
name: chore
description: Turn something found while using the product into a ready-for-agent issue, checked against the code first.
disable-model-invocation: true
---

# Chore

A person was using the web app, the CLI or a plug-in and noticed something small: a bug, a wrong word, a missing option, a leftover. They tell you what they found and, sometimes, what should change. You check it against the code and file one issue that an agent with none of this conversation can build from.

The report is one of two kinds:

- A **finding** says what is wrong and leaves the fix to you. You propose it.
- A **request** names the change. You work out what it takes, and you do not redesign it.

Ask for what the report does not carry and you cannot find yourself: the page or command, the game or save it happened with, and what they expected to see.

## 1. Check it against the code

Issues live in `pkhex-web/issue-tracker`; code lives here. Run `git fetch origin` and read the code as it is on `origin/main` (`git grep -n <term> origin/main`, `git show origin/main:<path>`). Local `main` falls behind while you work.

Find the code that produces what the person saw. Search by the text on the screen, the route, the command name or the error message under `src/`. Follow it down through the Engine and `PKHeX.Facade` to the line that does the wrong thing. When the claim is about behaviour, read the tests that cover it too.

Look for an open issue that already covers it: `gh issue list -R pkhex-web/issue-tracker --state open --search "<words>"`. Look at recently closed ones too, since a fix may be merged and not yet released.

Done when you can point at the file and line that produce what the person saw, or you can show that `origin/main` does not do it. If it does not, or an issue already covers it, say so with the evidence and stop.

## 2. Work out the change

For a finding, pick the fix. If there is a real choice, give it in one line each and say which one you recommend.

For a request, list what it touches: every file and function, the tests, and any `CONTEXT.md` term or ADR it runs into. If the request goes against one of those, say so. The person decides.

If the fix needs a change to a fork under `external/`, it is not a chore for an agent. File it `ready-for-human`, as CLAUDE.md says.

If the change needs decisions nobody has made yet, or touches more than one area in ways that could ship apart, it is not a chore. Say so and suggest `/to-spec`. Do not write the spec yourself.

Done when every file the change touches is named, with the tests to add.

## 3. Show the plan

In the terminal, short: what you found (file and line), the change you would ask for, and the issue title.

If the change shows in the UI, add the **surface**: the exact text it shows (labels, buttons, tooltips, messages, empty states) and where it sits (the page, the panel, what it goes next to). The person signs off on the surface as well as the code change, so write the real words, not a description of them.

Ask whether to file it. File nothing without a yes. The person may change the plan or the surface here.

## 4. File the issue

Load `/unslop` and read `docs/agents/issue-tracker.md`. Title it the way recent issues are titled: a plain sentence saying what is wrong or what changes (`Searchable selects keep the typed text after selecting an option`). Add the `ready-for-agent` label.

The agent that picks it up did not see the screen and did not read this conversation, so the body carries everything:

- **What happens now**: where the person saw it, the game or save type, and the file and line that cause it. Leave out emails, save file contents and anything else personal.
- **What to build**: each change named by file and function, with the exact values to use.
- **Surface**: when the change shows in the UI, the text and placement the person signed off on, word for word.
- **Tests**: the existing test file to extend, and the cases to add. One case shows the old behaviour is gone. Tests go in `PKHeX.Facade.Tests`, or `PKHeX.Everywhere.Engine.Tests` for the Engine. An E2E test only where ADR 0001 allows it.
- **Out of scope**: what you considered and are not asking for.
- **Done when**: the test projects for the parts it touches pass locally, and the SDK drift check passes if the Engine changed.

Report the issue link.
