# GitHub Pages Deployment

SideScroll includes automatic deployment of the WebAssembly browser demo to GitHub Pages.

## Live Demo

The demo is automatically deployed to: **https://sidescrollui.github.io/SideScroll/**

## How It Works

The deployment is handled by the `.github/workflows/deploy-pages.yml` workflow:

1. **Trigger**: Automatically runs on every push to the `main` branch
2. **Build**: Compiles the `SideScroll.Demo.Avalonia.Browser` project using `dotnet publish`
3. **Deploy**: Publishes the output to GitHub Pages using GitHub Actions

## Fork-Friendly Design

The workflow includes a repository check:
```yaml
if: github.repository == 'SideScrollUI/SideScroll'
```

This ensures:
- ✅ Only the main repository deploys to GitHub Pages
- ✅ Forks won't encounter deployment errors
- ✅ Contributors can still build and test locally

## Testing Locally

To test the WebAssembly build locally:

```bash
cd Programs/SideScroll.Demo.Avalonia.Browser
dotnet publish -c Release
```

Then serve the output folder:
```bash
cd bin/Release/net8.0-browser/publish/wwwroot
python -m http.server 8000
```

Visit: http://localhost:8000

**Note**: The `index.html` file automatically detects the environment and sets the correct base path (uses `/SideScroll/` for GitHub Pages and `/` for local development), so no manual changes are needed for local testing.

## Enabling on Your Fork (Optional)

If you fork this repository and want to deploy to your own GitHub Pages:

1. **Enable GitHub Pages** in your fork's settings:
   - Go to Settings → Pages
   - Set Source to "GitHub Actions"

2. **Update the repository check** in `.github/workflows/deploy-pages.yml`:
   ```yaml
   if: github.repository == 'YourUsername/SideScroll'
   ```

3. **Update the base path detection** in `wwwroot/index.html` script:
   ```javascript
   // Change the hostname check to match your GitHub Pages URL
   base.href = window.location.hostname === 'yourusername.github.io' ? '/YourRepoName/' : '/';
   ```

4. **Push to main** and the workflow will deploy to your Pages site

## Troubleshooting

### Assets not loading (404 errors)
- Check the dynamic base path logic in `index.html` is correctly detecting the hostname
- Verify the repository name matches the base href path in the script
- Open browser developer tools to see what base URL is being set

### Workflow fails with permissions error
- Ensure GitHub Pages is enabled in repository settings
- Verify the source is set to "GitHub Actions"
- Check that workflow permissions allow Pages deployment

### Changes not appearing on the live site
- GitHub Pages may cache content - try a hard refresh (Ctrl+Shift+R)
- Check the Actions tab to ensure the workflow completed successfully
- Wait a few minutes for CDN propagation

## Manual Deployment

You can also trigger deployment manually:

1. Go to the Actions tab in GitHub
2. Select "Deploy to GitHub Pages"
3. Click "Run workflow"

## Future Enhancements

Potential improvements for the deployment:

- **Optimization**: Enable trimming and compression to reduce app size
- **Versioning**: Display commit hash or version number on the demo page
- **Custom Domain**: Configure a custom domain like `demo.sidescroll.io`
- **Preview Deployments**: Deploy pull requests to preview URLs
- **Performance Monitoring**: Add analytics or performance tracking
