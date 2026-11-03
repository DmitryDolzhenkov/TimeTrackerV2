# Mission

Implement TimeTrackerV2 incrementally from the approved specification.

The goal is not to maximize code volume.
The goal is to implement the smallest correct solution satisfying
the current issue acceptance criteria.

# Sources of truth

Priority:

1. Current issue
2. docs/prd.md
3. docs/adr/*
4. Existing tests
5. Existing code

If these sources conflict, do not guess.
Report the conflict.

# Working rule

Work on ONE issue at a time.

Do not implement future issues unless required for the current
acceptance criteria.

# Before implementation

1. Read the current issue.
2. Read relevant parts of the PRD.
3. Inspect existing implementation.
4. State assumptions.
5. Propose a short implementation plan.

# Architecture constraints

<!-- - Modular monolith. -->
<!-- - EF Core. -->
- No message broker.
- No MediatR.
- No AutoMapper.
- No additional production dependency without explicit approval.
- Prefer standard .NET capabilities over new frameworks.

# Scope control

Do not:

- refactor unrelated code;
- redesign working components without a requirement;
- introduce generic frameworks for a single use case;
- implement speculative extensibility;
- add features not mentioned in the current issue.

# Verification

Before considering an issue complete:

1. Run build.
2. Run relevant tests.
3. Run the complete test suite.
4. Compare implementation against every acceptance criterion.
5. Report any criterion without evidence.

# Completion report

Provide:

- summary;
- files changed;
- tests added;
- commands executed;
- acceptance criteria evidence;
- assumptions;
- remaining risks.

# Commands

Use quiet output; it is enough to see failures.

- Build: `dotnet build -v q`
- All tests: `dotnet test -v q --logger "console;verbosity=minimal"`
- Single test class: add `--filter "FullyQualifiedName~<ClassName>"`
- Local run: see README.md.

# Git and PR

- One branch per issue, from up-to-date `main`: `feature/<feature-slug>-<NN>-<short-slug>`,
  e.g. `feature/timer-03-idle-detection`. Docs/tooling: `docs/<slug>`, `chore/<slug>`.
- Never commit to `main` directly.
- Commit messages in English, imperative, with the feature and ticket number: `Add ... (<feature-slug>/03)`.
- PR: title = issue title, body starts with `Implements .scratch/<feature-slug>/issues/<NN>-<slug>.md` and contains
  the completion report (acceptance criteria → evidence table).
- Windows PowerShell 5.1 breaks multi-line `git commit`/`gh` arguments:
  pass messages via a Bash heredoc or `--body-file`.
- Never commit `.env` or secrets.

# Progress

Update docs/progress.md after completing an issue.

Do not mark an issue complete if tests fail.

# Human decisions

Stop and request human decision when:

- requirements are ambiguous;
- architecture boundaries must change;
- a new dependency is needed;
- a business rule is missing;
- security semantics are unclear;
- the implementation requires changing another issue's scope.

## Agent skills

### Issue tracker

Issues and specs live as local markdown files under `.scratch/`. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
