"""Sync the CLI's MAUI templates from the working demo copies.

    python scripts/sync-templates.py


The demo (examples/MAUI.Demo/Components/UI) is where components are developed and run;
this copies each file into its *Template.cs as the [NativePlatform.MAUI] verbatim string,
restoring the YourProjectNamespace placeholder, and recomputes shared dependencies.
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DEMO = os.path.join(ROOT, "examples", "MAUI.Demo", "Components", "UI")
TPL = os.path.join(ROOT, "src", "ShellUI.Native.Templates", "Templates")

# New templates that don't exist yet: name -> (registry key, display, description, category, tags)
NEW = {
    "Icon": ("icon", "Icon", "Lucide stroke icons drawn with MAUI shapes (no font or package)", "Utility",
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
    "EmptyState": ("empty-state", "Empty State", "Placeholder for an empty list or screen", "DataDisplay",
                   ["empty", "placeholder", "state"]),
}

# Component-to-component dependencies the code scan can't infer.
EXTRA_DEPS = {
    "HoverCard": ["hover-card-trigger", "hover-card-content"],
    "DatePicker": ["calendar"],
}

SHELL_API = re.compile(r"\bShellTheme\b|\.Token\(|\bShellToken\b|\bShellFocus\b|\bShellPlatform\b|\bShellPopups\b|"
                       r"\bShellTriggerView\b|\bShellOverlayHost\b|\bShellPopoverHost\b|\bShellPortal\b|"
                       r"\bShellPopup\w+\b|\bIShellFocusable\b|\bIShellPopup\b|\bIShellOverlayContent\b")
ICON_API = re.compile(r"\bnew Icon\b|\bIconName\b")
EXT_API = re.compile(r"\bFindParentOfType\b|\bFindDescendantsOfType\b|\bAnimateExpandAsync\b")

MAUI_BLOCK = re.compile(r'(\[NativePlatform\.MAUI\] = @")(.*?)(\n"\n    \};)', re.S)
DEPS_LINE = re.compile(r'^(\s*)Dependencies = new List<string>(?:\s*\{([^}]*)\})?(?:\(\))?,\s*$', re.M)


def to_template_content(source: str) -> str:
    source = source.replace("\r\n", "\n")
    source = source.replace("MAUI.Demo.Components.UI", "YourProjectNamespace.Components.UI")
    if "MAUI.Demo" in source:
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

    print("\n".join(changed))
    print(f"{len(changed)} templates written")


if __name__ == "__main__":
    main()
