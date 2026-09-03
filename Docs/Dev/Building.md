# Building

## Clone

- `git clone https://github.com/SideScrollUI/SideScroll.git`
- `cd SideScroll`

## Build

- `dotnet build`

## Run

- `dotnet run --project Programs/SideScroll.Demo.Avalonia.Desktop/SideScroll.Demo.Avalonia.Desktop.csproj`

### Pack

- Update `<Version>` and `<PackageReleaseNotes>` in [Directory.Build.props](../../Directory.Build.props)
- `dotnet pack -o Packages`

## Publish

The [Publish to NuGet](../../.github/workflows/publish-nuget.yml) workflow builds, tests, packs, and pushes every package to [nuget.org](https://www.nuget.org/profiles/SideScrollUI).

Releasing is a version bump:

- Update `<Version>` and `<PackageReleaseNotes>` in [Directory.Build.props](../../Directory.Build.props)
- Commit and push to `main`

A `v<version>` tag is the record of what's been released, so a version without one is a new release. The workflow publishes it and then tags the commit it published, which keeps later pushes from publishing the same version twice. Pushes made while the current version is already tagged stop after the version check, without building.

The first push to `main` after the bump is what releases, whether or not that push is the bump commit itself.

Running the workflow manually from the Actions tab builds and packs without publishing, uploading the packages as a build artifact to check before committing to a release. Checking **Push the packages to nuget.org** publishes from a manual run, which is also how to retry a release that failed partway through.

### Setup

Publishing uses [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), so there's no API key to store or rotate. The workflow requests a GitHub OIDC token, and nuget.org exchanges it for a temporary key that expires after an hour.

On nuget.org, under your username → **Trusted Publishing**, add a policy:

| Field | Value |
| --- | --- |
| Repository Owner | `SideScrollUI` |
| Repository | `SideScroll` |
| Workflow File | `publish-nuget.yml` |
| Environment | *(leave empty)* |

The workflow passes the `sidescrollui` profile name to `NuGet/login`, which has to match the policy's package owner.

Renaming the workflow file breaks the policy, since it's matched by file name.
