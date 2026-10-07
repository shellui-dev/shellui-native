# Releasing ShellUI Native

Runbook for publishing `ShellUI.Native.CLI` to NuGet. Same setup as ShellDocs.

## One-time setup

Publishing uses **NuGet Trusted Publishing**: each run of `.github/workflows/release.yml` exchanges a GitHub OIDC token for a NuGet API key that lasts one hour. No long-lived API key is stored. Official docs: <https://learn.microsoft.com/nuget/nuget-org/trusted-publishing>.

### 1. Create the `release` environment on GitHub

Repo → Settings → Environments → **New environment** → name it exactly `release`. Nothing else is required (optionally add required reviewers for a manual gate on each publish).

### 2. Add a Trusted Publishing policy on nuget.org

Sign in → your username → **Trusted Publishing** → **Add**, choose the owner of the package, then (case-insensitive):

| Field | Value |
|---|---|
| Repository Owner | `shellui-dev` |
| Repository | `shellui-native` |
| Workflow File | `release.yml` (file name only, no `.github/workflows/`) |
| Environment | `release` (must match `environment: release` in the workflow) |

A policy for a private repository starts as provisional for 7 days; the first successful publish locks it to the repository. If nothing is published in 7 days it goes inactive and can be restarted.

### 3. Add the `NUGET_USER` secret

Repo → Settings → Secrets and variables → Actions → **New repository secret** (or add it to the `release` environment):

| Name | Value |
|---|---|
| `NUGET_USER` | Your nuget.org profile name, as in `nuget.org/profiles/<name>`; not the email and not the GitHub org |

No `NUGET_API_KEY` secret is needed; delete an old one if it exists.

## Releasing a version

1. Set the version in `Directory.Build.props` (`ShellUINativeVersion` and `ShellUINativeVersionSuffix`).
2. Add a `# ShellUI Native v<version>` section to [RELEASE_NOTES.md](./RELEASE_NOTES.md). It becomes the GitHub release text.
3. Merge to `main`, then tag and push:

   ```bash
   git switch main && git pull --ff-only
   git tag -a v<version> -m "ShellUI Native v<version>"
   git push origin v<version>
   ```

The tag push runs the workflow: it checks the tag matches the props version, extracts the release notes, builds, runs the tests, packs the CLI, stops if that version is already on nuget.org, logs in via Trusted Publishing, pushes the package and creates the GitHub release (a prerelease when the version has a `-`).

If the login step fails, check in this order: `NUGET_USER` missing or wrong, the policy's Workflow File has a path prefix, the policy's Environment doesn't match `release`, the policy expired (provisional, private repo).

## Dry run

Actions → Release → **Run workflow** with "Pack and validate only" checked: runs the version, release-notes, build, test and pack steps and skips the version check, login, push and GitHub release.

## After the release

- The package shows at <https://www.nuget.org/packages/ShellUI.Native.CLI> after indexing (a few minutes).
- Check the install in a scratch folder: `dotnet tool install -g ShellUI.Native.CLI --prerelease`.
