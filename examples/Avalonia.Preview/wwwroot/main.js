import { dotnet } from './_framework/dotnet.js';

const runtime = await dotnet.create();
const config = runtime.getConfig();
const exports = await runtime.getAssemblyExports(config.mainAssemblyName);

// ShellDocs posts the reader's theme to its preview frames when it changes.
window.addEventListener('message', (event) => {
    if (event.data?.type === 'shelldocs-theme') exports.AvaloniaPreview.Program.SetTheme(String(event.data.theme));
});

await runtime.runMain(config.mainAssemblyName, [globalThis.location.href]);
performance.mark('preview-started');
