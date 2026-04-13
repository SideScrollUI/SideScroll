import { dotnet } from './_framework/dotnet.js'
import { BrowserStorage } from './_content/SideScroll.Serialize.Browser/localStorage.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

console.log('🌐 Starting SideScroll Browser Demo with localStorage persistence...');

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

// Verify BrowserStorage is available
if (BrowserStorage) {
    console.log('✓ BrowserStorage module loaded successfully');
    const stats = BrowserStorage.getStats();
    console.log(`📊 localStorage: ${stats.itemCount} items, ~${stats.estimatedSizeMB} MB used`);
} else {
    console.error('❌ BrowserStorage module failed to load');
}

// Export BrowserStorage functions so they're accessible to C# JSImport
export { BrowserStorage };

// Run the .NET application
const config = dotnetRuntime.getConfig();
await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);

console.log('✓ SideScroll Browser Demo started');
