"""Sync the CLI's templates from the working demo copies.

    python scripts/sync-templates.py

The demos (examples/MAUI.Demo and examples/Avalonia.Demo, Components/UI) are where components
are developed and run; this copies each file into its *Template.cs as the platform's verbatim
string, restoring the YourProjectNamespace placeholder. Dependencies come from the MAUI file;
an Avalonia file may only use dependencies the template already declares.
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DEMO = os.path.join(ROOT, "examples", "MAUI.Demo", "Components", "UI")
AVALONIA_DEMO = os.path.join(ROOT, "examples", "Avalonia.Demo", "Components", "UI")
TPL = os.path.join(ROOT, "src", "ShellUI.Native.Templates", "Templates")

# New templates that don't exist yet: name -> (registry key, display, description, category, tags)
NEW = {
    "Icon": ("icon", "Icon", "Lucide stroke icons drawn as native shapes (no font or package)", "Utility",
             ["icon", "lucide", "svg", "utility"]),
    "ThemeToggle": ("theme-toggle", "Theme Toggle", "Light/dark mode toggle button", "Utility",
                    ["theme", "dark-mode", "toggle"]),
    "Spinner": ("spinner", "Spinner", "Rotating loading indicator", "Feedback",
                ["loading", "spinner", "progress"]),
    "Avatar": ("avatar", "Avatar", "Circular image with initials or icon fallback", "DataDisplay",
               ["avatar", "image", "profile", "user"]),
    "AlertDialog": ("alert-dialog", "Alert Dialog", "Confirmation dialog that requires a choice", "Overlay",
                    ["dialog", "confirm", "modal", "alert"]),
    "Toast": ("toast", "Toast", "Sonner-style stacked notifications (Toaster + Toast API)", "Feedback",
              ["toast", "sonner", "notification", "snackbar"]),
    "Tooltip": ("tooltip", "Tooltip", "Hover tooltip for any view", "Overlay",
                ["tooltip", "hover", "hint"]),
    "HoverCard": ("hover-card", "Hover Card", "Rich content that floats next to its trigger on hover", "Overlay",
                  ["hover", "card", "preview", "popover"]),
    "HoverCardTrigger": ("hover-card-trigger", "Hover Card Trigger", "View that opens a hover card while hovered", "Overlay",
                         ["hover", "card", "trigger"]),
    "HoverCardContent": ("hover-card-content", "Hover Card Content", "Floating panel of a hover card", "Overlay",
                         ["hover", "card", "content"]),
    "Calendar": ("calendar", "Calendar", "Month calendar with day selection", "Form",
                 ["calendar", "date", "picker"]),
    "Toggle": ("toggle", "Toggle", "Two-state button that stays pressed", "Form",
               ["toggle", "button", "pressed"]),
    "InputOtp": ("input-otp", "Input OTP", "One-time-code input with a slot per character", "Form",
                 ["otp", "code", "verification", "input"]),
    "Pagination": ("pagination", "Pagination", "Page navigation with previous/next and ellipsis", "Navigation",
                   ["pagination", "pages", "navigation"]),
    "Callout": ("callout", "Callout", "Highlighted note tinted by variant (info, warning, danger, tip)", "Feedback",
                ["callout", "note", "admonition", "info"]),
    "Combobox": ("combobox", "Combobox", "Searchable select with a filter field", "Form",
                 ["combobox", "select", "search", "autocomplete"]),
    "Table": ("table", "Table", "Data table with header, rows, hover and optional caption", "DataDisplay",
              ["table", "data", "grid", "rows"]),
    "ContextMenu": ("context-menu", "Context Menu", "Right-click / long-press menu that opens at the pointer", "Overlay",
                    ["context", "menu", "right-click", "long-press"]),
    "Carousel": ("carousel", "Carousel", "Swipeable slides with arrows, dots and auto-play", "DataDisplay",
                 ["carousel", "slider", "slides", "gallery"]),
    "Stepper": ("stepper", "Stepper", "Step-by-step flow with numbered steps and navigation", "Navigation",
                ["stepper", "wizard", "steps", "progress"]),
    "ToggleGroup": ("toggle-group", "Toggle Group", "Row of toggles with single or multiple selection", "Form",
                    ["toggle", "group", "segmented", "selection"]),
    "NumberInput": ("number-input", "Number Input", "Number field with decrement and increment buttons", "Form",
                    ["number", "input", "stepper", "quantity"]),
    "TagInput": ("tag-input", "Tag Input", "Text field that turns entries into removable tags", "Form",
                 ["tags", "chips", "input", "labels"]),
    "Kbd": ("kbd", "Kbd", "Keyboard key hint", "DataDisplay",
            ["keyboard", "shortcut", "key"]),
    "StatCard": ("stat-card", "Stat Card", "Dashboard metric with trend and description", "DataDisplay",
                 ["stat", "metric", "kpi", "dashboard"]),
    "Timeline": ("timeline", "Timeline", "Vertical list of events joined by a line", "DataDisplay",
                 ["timeline", "history", "activity", "steps"]),
    "WrapLayout": ("wrap-layout", "Wrap Layout", "Children flow left to right and wrap onto new rows", "Layout",
                   ["wrap", "flow", "flex", "layout"]),
    "CopyButton": ("copy-button", "Copy Button", "Copies text to the clipboard and confirms with a check", "Utility",
                   ["copy", "clipboard", "button"]),
    "AspectRatio": ("aspect-ratio", "Aspect Ratio", "Keeps content at a fixed width / height ratio", "Layout",
                    ["aspect", "ratio", "layout", "media"]),
    "LinkCard": ("link-card", "Link Card", "Tappable card with icon, title and description", "Navigation",
                 ["link", "card", "navigation"]),
    "TreeView": ("tree-view", "Tree View", "Expandable tree of rows with selection", "Navigation",
                 ["tree", "hierarchy", "files", "navigation"]),
    "MultiSelect": ("multi-select", "Multi Select", "Searchable select for several options, shown as chips", "Form",
                    ["select", "multiple", "chips", "search"]),
    "EmptyState": ("empty-state", "Empty State", "Placeholder for an empty list or screen", "DataDisplay",
                   ["empty", "placeholder", "state"]),
}

# Component-to-component dependencies the code scan can't infer.
EXTRA_DEPS = {
    "HoverCard": ["hover-card-trigger", "hover-card-content"],
    "DatePicker": ["calendar"],
    "TagInput": ["wrap-layout"],
    "MultiSelect": ["wrap-layout"],
}

SHELL_API = re.compile(r"\bShellTheme\b|\.Token\(|\bShellToken\b|\bShellFocus\b|\bShellPlatform\b|\bShellPopups\b|"
                       r"\bShellTriggerView\b|\bShellOverlayHost\b|\bShellPopoverHost\b|\bShellPortal\b|"
                       r"\bShellPopup\w+\b|\bIShellFocusable\b|\bIShellPopup\b|\bIShellOverlayContent\b")
ICON_API = re.compile(r"\bnew Icon\b|\bIconName\b")
EXT_API = re.compile(r"\bFindParentOfType\b|\bFindDescendantsOfType\b|\bAnimateExpandAsync\b|\bSetExpanded\b")

# A block ends at the closing quote that is followed by the next entry or the end of the dictionary.
MAUI_BLOCK = re.compile(r'(\[NativePlatform\.MAUI\] = @")(.*?)(\n"(?=,\n|\n    \};))', re.S)
AVALONIA_BLOCK = re.compile(r'(\[NativePlatform\.Avalonia\] = @")(.*?)(\n"(?=,\n|\n    \};))', re.S)
DEPS_LINE = re.compile(r'^(\s*)Dependencies = new List<string>(?:\s*\{([^}]*)\})?(?:\(\))?,\s*$', re.M)


def to_template_content(source: str, demo_namespace: str = "MAUI.Demo") -> str:
    source = source.replace("\r\n", "\n")
    source = source.replace(f"{demo_namespace}.Components.UI", "YourProjectNamespace.Components.UI")
    if demo_namespace in source:
        raise SystemExit("demo-specific namespace left in source")
    # The closing quote sits on its own line after the block, so drop the trailing newline.
    return source.rstrip("\n").replace('"', '""')


def compute_deps(name: str, source: str, existing: list[str]) -> list[str]:
    shared = []
    if name != "Shell" and SHELL_API.search(source):
        shared.append("shell")
    if name != "Icon" and ICON_API.search(source):
        shared.append("icon")
    # Shell is the base layer: element-extensions depends on it, never the reverse.
    if name not in ("ElementExtensions", "Shell") and EXT_API.search(source):
        shared.append("element-extensions")
    # Composites that render ShellUI Buttons (AlertDialog footer, Toast action).
    if name not in ("Button", "ButtonVariants") and re.search(r"\bnew Button\b|\bButtonVariant\b", source):
        shared.append("button")
    rest = [d for d in existing if d not in ("shell", "icon", "element-extensions", "button")]
    rest += [d for d in EXTRA_DEPS.get(name, []) if d not in rest]
    return shared + rest


def new_template(name: str, content: str, deps: list[str]) -> str:
    key, display, desc, category, tags = NEW[name]
    dep_list = "new List<string>()" if not deps else "new List<string> { " + ", ".join(f'"{d}"' for d in deps) + " }"
    tag_list = ", ".join(f'"{t}"' for t in tags)
    return (
        "using ShellUI.Native.Core.Models;\n\n"
        "namespace ShellUI.Native.Templates.Templates;\n\n"
        f"public static class {name}Template\n"
        "{\n"
        "    public static ComponentMetadata Metadata => new()\n"
        "    {\n"
        f'        Name = "{key}",\n'
        f'        DisplayName = "{display}",\n'
        f'        Description = "{desc}",\n'
        f"        Category = ComponentCategory.{category},\n"
        f'        FilePath = "{name}.cs",\n'
        f"        Dependencies = {dep_list},\n"
        f"        Tags = new List<string> {{ {tag_list} }}\n"
        "    };\n\n"
        "    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>\n"
        "    {\n"
        f'        [NativePlatform.MAUI] = @"{content}\n"\n'
        "    };\n"
        "}\n"
    )


def main() -> None:
    changed = []
    for dirpath, _, files in os.walk(DEMO):
        for file in sorted(files):
            if not file.endswith(".cs"):
                continue
            name = file[:-3]
            source = open(os.path.join(dirpath, file), encoding="utf-8-sig").read()
            content = to_template_content(source)
            tpl_path = os.path.join(TPL, f"{name}Template.cs")

            if not os.path.exists(tpl_path):
                if name not in NEW:
                    print(f"SKIP (no template, not declared new): {name}")
                    continue
                deps = compute_deps(name, source, [])
                open(tpl_path, "w", encoding="utf-8", newline="\n").write(new_template(name, content, deps))
                changed.append(f"NEW  {name} deps={deps}")
                continue

            raw = open(tpl_path, encoding="utf-8-sig", newline="").read()
            crlf = "\r\n" in raw
            text = raw.replace("\r\n", "\n")

            match = MAUI_BLOCK.search(text)
            if not match:
                raise SystemExit(f"no MAUI block in {tpl_path}")
            text = text[:match.start(2)] + content + text[match.end(2):]

            deps_match = DEPS_LINE.search(text)
            existing = re.findall(r'"([^"]+)"', deps_match.group(2) or "") if deps_match else []
            deps = compute_deps(name, source, existing)
            dep_list = "new List<string>()" if not deps else "new List<string> { " + ", ".join(f'"{d}"' for d in deps) + " }"
            if deps_match:
                text = text[:deps_match.start()] + f"{deps_match.group(1)}Dependencies = {dep_list}," + text[deps_match.end():]
            elif deps:
                text = text.replace("        FilePath = ", f"        Dependencies = {dep_list},\n        FilePath = ", 1)

            if crlf:
                text = text.replace("\n", "\r\n")
            if text != raw:
                open(tpl_path, "w", encoding="utf-8", newline="").write(text)
                changed.append(f"UPD  {name} deps={deps}")

    changed += sync_avalonia()
    print("\n".join(changed))
    print(f"{len(changed)} templates written")


def sync_avalonia() -> list[str]:
    changed = []
    # Walks subfolders too (Variants/ButtonVariants.cs), like the MAUI pass.
    files = sorted((d, f) for d, _, fs in os.walk(AVALONIA_DEMO) for f in fs if f.endswith(".cs"))
    for dirpath, file in files:
        name = file[:-3]
        source = open(os.path.join(dirpath, file), encoding="utf-8-sig").read()
        content = to_template_content(source, "AvaloniaDemo")
        tpl_path = os.path.join(TPL, f"{name}Template.cs")
        if not os.path.exists(tpl_path):
            print(f"SKIP (Avalonia, no MAUI template yet): {name}")
            continue

        raw = open(tpl_path, encoding="utf-8-sig", newline="").read()
        crlf = "\r\n" in raw
        text = raw.replace("\r\n", "\n")

        deps_match = DEPS_LINE.search(text)
        declared = re.findall(r'"([^"]+)"', deps_match.group(2) or "") if deps_match else []
        missing = [d for d in compute_deps(name, source, []) if d not in declared]
        if missing:
            raise SystemExit(f"Avalonia {name} uses {missing}, which the template doesn't declare")

        match = AVALONIA_BLOCK.search(text)
        if match:
            text = text[:match.start(2)] + content + text[match.end(2):]
        else:
            maui = MAUI_BLOCK.search(text)
            if not maui:
                raise SystemExit(f"no MAUI block in {tpl_path}")
            text = text[:maui.end()] + f',\n        [NativePlatform.Avalonia] = @"{content}\n"' + text[maui.end():]

        if crlf:
            text = text.replace("\n", "\r\n")
        if text != raw:
            open(tpl_path, "w", encoding="utf-8", newline="").write(text)
            changed.append(f"AVA  {name}")
    return changed


if __name__ == "__main__":
    main()
