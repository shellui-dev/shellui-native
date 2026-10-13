import { dotnet } from './_framework/dotnet.js';

const runtime = await dotnet.create();
const config = runtime.getConfig();
const preview = (await runtime.getAssemblyExports(config.mainAssemblyName)).AvaloniaPreview.Program;

// For the page that hosts the frame, and for tools that measure a preview.
globalThis.shelluiPreview = {
    setTheme: (theme) => preview.SetTheme(String(theme)),
    contentHeight: () => preview.ContentHeight(),
};

// ShellDocs posts the reader's theme to its preview frames when it changes.
window.addEventListener('message', (event) => {
    if (event.data?.type === 'shelldocs-theme') preview.SetTheme(String(event.data.theme));
});

await runtime.runMain(config.mainAssemblyName, [globalThis.location.href]);
performance.mark('preview-started');
