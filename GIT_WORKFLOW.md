# SolarGridOps Git Workflow

## Branch model
- main: production-safe history.
- develop: integration branch for day-to-day development.
- feature/<name>: new functionality.
- fix/<name>: non-critical bug fixes.
- release/<version>: release hardening and final checks.
- hotfix/<name>: urgent production fixes from main.

## Start a branch
Use the helper script:

```powershell
./scripts/git/new-branch.ps1 -Type feature -Name auth-permission-policies
./scripts/git/new-branch.ps1 -Type fix -Name customer-list-filter-bug
./scripts/git/new-branch.ps1 -Type release -Name v0.2.0
./scripts/git/new-branch.ps1 -Type hotfix -Name login-token-expiry
```

## Merge flow
1. feature/* and fix/* merge into develop.
2. release/* merges into main after validation, then back-merge into develop.
3. hotfix/* merges into main first, then back-merge into develop.

## Commit guidance
- Keep commits small and focused.
- Use clear commit messages:
  - feat: ...
  - fix: ...
  - chore: ...
  - docs: ...

## Safety rules
- Never commit data/Sample or docs/planning (already gitignored).
- Before push, run:

```powershell
git status -sb
git log --oneline --decorate --graph -10
```
