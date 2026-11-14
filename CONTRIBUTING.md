# Contributing to TriageIQ

This is a personal learning and portfolio project. External contributions are not expected, but the workflow below keeps history consistent.

## Git hooks

This repo uses a custom hooks path. After cloning, run:

```bash
git config core.hooksPath .githooks
```

On Windows, Git Bash runs the `commit-msg` hook. It strips any line starting with `Co-authored-by:` from commit messages so the recorded author stays a single identity (see `docs/SPEC.md` section 11).

## Commits

- Use [Conventional Commits](https://www.conventionalcommits.org/).
- Keep commits small and focused on one milestone or feature slice.
- Never commit `.env`, API keys, or content under `study-guides/` or `study/`.

## Pull requests

- One milestone per PR where possible (for example `feat/m0-foundation`).
- Fill in `.github/pull_request_template.md`.
- Plain language only; no tool attribution footers.