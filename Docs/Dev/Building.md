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
- `git tag v0.3.0`
- `dotnet pack -o Packages`

## Publish

The [Publish to NuGet](../../.github/workflows/publish-nuget.yml) workflow builds, tests, packs, and pushes every package to [nuget.org](https://www.nuget.org/profiles/SideScrollUI).

Run it from the Actions tab, selecting the tag to release from the ref dropdown:

- Update `<Version>` in [Directory.Build.props](../../Directory.Build.props) and commit it
- `git tag v0.24`
- `git push origin v0.24`
- Run the workflow against `v0.24` with **Push the packages to nuget.org** checked

Leaving that box unchecked packs the packages and uploads them as a build artifact without publishing, which is a way to verify a release before committing to it. The tag has to match `<Version>` or the workflow fails before publishing anything.

The automatic `v*` tag trigger is commented out in the workflow until a manual run has verified the setup end to end.

### Setup

Publishing uses [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), so there's no API key to store or rotate. The workflow requests a GitHub OIDC token, and nuget.org exchanges it for a temporary key that expires after an hour.

On nuget.org, under your username → **Trusted Publishing**, add a policy:

| Field | Value |
| --- | --- |
| Repository Owner | `SideScrollUI` |
| Repository | `SideScroll` |
| Workflow File | `publish-nuget.yml` |
| Environment | *(leave empty)* |

Then add a `NUGET_USER` repository secret (Settings → Secrets and variables → Actions) holding the nuget.org profile name that owns the policy — not the account's email address.

Renaming the workflow file breaks the policy, since it's matched by file name.
