"""Generate the MAUI `icon` component (examples/MAUI.Demo/Components/UI/Icon.cs) from the
ShellIcons catalog (Lucide-derived SVGs).

    python scripts/generate-icons.py [path-to-shell-icons]

Default catalog location: ../../icons/shell-icons (sibling of the native repo in the ShellUI
workspace). Each SVG's shapes (path, circle, ellipse, rect, line, polyline, polygon) are
converted to one absolute-coordinate path string, so MAUI's path parser never sees Lucide's
compact relative syntax. Run scripts/sync-templates.py afterwards to update the CLI template.

Only the icons in ICONS are emitted — the template is copied into user projects, so it ships a
curated set rather than all 1500+ Lucide icons. Add a kebab-case name and re-run.
"""
import math
import os
import re
import sys

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DEFAULT_CATALOG = os.path.normpath(os.path.join(REPO, "..", "..", "icons", "shell-icons"))
OUTPUT = os.path.join(REPO, "examples", "MAUI.Demo", "Components", "UI", "Icon.cs")

ICONS = [
    # actions
    "check", "x", "plus", "minus", "search", "filter", "settings", "sliders-horizontal",
    "pencil", "trash", "trash-2", "copy", "clipboard", "save", "download", "upload",
    "share", "share-2", "external-link", "link", "refresh-cw", "rotate-ccw", "undo-2", "redo-2",
    "log-in", "log-out", "send", "paperclip",
    # navigation
    "chevron-down", "chevron-up", "chevron-left", "chevron-right", "chevrons-up-down",
    "chevrons-left", "chevrons-right", "arrow-left", "arrow-right", "arrow-up", "arrow-down",
    "arrow-up-right", "menu", "ellipsis", "ellipsis-vertical", "grip-vertical", "house",
    "layout-dashboard", "panel-left",
    # status / feedback
    "info", "circle-alert", "circle-check", "circle-x", "circle-help", "triangle-alert", "ban",
    "loader", "loader-circle", "bell", "circle", "circle-plus", "dot",
    # people / account
    "user", "users", "user-plus", "lock", "lock-open", "key", "shield", "eye", "eye-off",
    # content / objects
    "mail", "inbox", "archive", "message-circle", "phone", "calendar", "clock", "globe",
    "map-pin", "image", "file", "file-text", "folder", "tag", "bookmark", "heart", "star",
    "thumbs-up", "shopping-cart", "credit-card", "dollar-sign", "activity", "trending-up",
    "chart-column", "zap", "sparkles", "command", "terminal", "code", "moon", "sun", "monitor",
    "smartphone", "laptop", "wifi", "camera", "mic", "play", "pause", "volume-2", "lightbulb",
]

NUMBER = re.compile(r"[-+]?(?:\d+\.?\d*|\.\d+)(?:[eE][-+]?\d+)?")
ATTR = re.compile(r'([\w-]+)\s*=\s*"([^"]*)"')
ELEMENT = re.compile(r"<(path|circle|ellipse|rect|line|polyline|polygon)\b([^>]*)/?>", re.S)


def fmt(v: float) -> str:
    v = round(v, 3)
    if v == 0:
        v = 0.0  # no "-0"
    s = f"{v:.3f}".rstrip("0").rstrip(".")
    return s


class PathTokens:
    """Tokenizer for SVG path data, incl. compact arc flags ("a1 1 0 011 1")."""

    def __init__(self, d: str):
        self.d = d
        self.i = 0

    def _skip(self):
        while self.i < len(self.d) and self.d[self.i] in " ,\t\r\n":
            self.i += 1

    def command(self):
        self._skip()
        if self.i < len(self.d) and self.d[self.i].isalpha():
            c = self.d[self.i]
            self.i += 1
            return c
        return None

    def has_number(self):
        self._skip()
        return self.i < len(self.d) and NUMBER.match(self.d, self.i) is not None

    def number(self) -> float:
        self._skip()
        m = NUMBER.match(self.d, self.i)
        if not m:
            raise ValueError(f"expected number at {self.i} in {self.d!r}")
        self.i = m.end()
        return float(m.group(0))

    def flag(self) -> int:
        self._skip()
        c = self.d[self.i]
        if c not in "01":
            raise ValueError(f"expected flag at {self.i} in {self.d!r}")
        self.i += 1
        return int(c)


def path_to_absolute(d: str) -> str:
    t = PathTokens(d)
    out = []
    x = y = sx = sy = 0.0
    cmd = None
    while True:
        c = t.command()
        if c is None:
            if cmd is None or not t.has_number():
                break
            c = cmd  # implicit repeat
        rel = c.islower()
        C = c.upper()
        if C == "Z":
            out.append("Z")
            x, y = sx, sy
            cmd = None
            continue
        first = True
        while first or t.has_number():
            first = False
            if C == "M":
                nx, ny = t.number(), t.number()
                if rel:
                    nx, ny = nx + x, ny + y
                out.append(f"M {fmt(nx)} {fmt(ny)}")
                x, y, sx, sy = nx, ny, nx, ny
                C, c = "L", ("l" if rel else "L")  # subsequent pairs are lineto
            elif C in ("L", "T"):
                nx, ny = t.number(), t.number()
                if rel:
                    nx, ny = nx + x, ny + y
                out.append(f"{C} {fmt(nx)} {fmt(ny)}")
                x, y = nx, ny
            elif C == "H":
                nx = t.number() + (x if rel else 0)
                out.append(f"L {fmt(nx)} {fmt(y)}")
                x = nx
            elif C == "V":
                ny = t.number() + (y if rel else 0)
                out.append(f"L {fmt(x)} {fmt(ny)}")
                y = ny
            elif C == "C":
                pts = [t.number() for _ in range(6)]
                if rel:
                    pts = [p + (x if i % 2 == 0 else y) for i, p in enumerate(pts)]
                out.append("C " + " ".join(fmt(p) for p in pts))
                x, y = pts[4], pts[5]
            elif C in ("S", "Q"):
                pts = [t.number() for _ in range(4)]
                if rel:
                    pts = [p + (x if i % 2 == 0 else y) for i, p in enumerate(pts)]
                out.append(f"{C} " + " ".join(fmt(p) for p in pts))
                x, y = pts[2], pts[3]
            elif C == "A":
                rx, ry, rot = t.number(), t.number(), t.number()
                large, sweep = t.flag(), t.flag()
                nx, ny = t.number(), t.number()
                if rel:
                    nx, ny = nx + x, ny + y
                out.append(f"A {fmt(rx)} {fmt(ry)} {fmt(rot)} {large} {sweep} {fmt(nx)} {fmt(ny)}")
                x, y = nx, ny
            else:
                raise ValueError(f"unsupported command {c!r} in {d!r}")
        cmd = c if C != "M" else ("l" if rel else "L")
    return " ".join(out)


def f(attrs, name, default=0.0):
    return float(attrs.get(name, default))


def element_to_path(tag: str, attrs: dict) -> str:
    if tag == "path":
        return path_to_absolute(attrs["d"])
    if tag in ("circle", "ellipse"):
        cx, cy = f(attrs, "cx"), f(attrs, "cy")
        rx = f(attrs, "r") if tag == "circle" else f(attrs, "rx")
        ry = f(attrs, "r") if tag == "circle" else f(attrs, "ry")
        return (f"M {fmt(cx + rx)} {fmt(cy)} A {fmt(rx)} {fmt(ry)} 0 1 1 {fmt(cx - rx)} {fmt(cy)} "
                f"A {fmt(rx)} {fmt(ry)} 0 1 1 {fmt(cx + rx)} {fmt(cy)} Z")
    if tag == "rect":
        x, y, w, h = f(attrs, "x"), f(attrs, "y"), f(attrs, "width"), f(attrs, "height")
        rx = float(attrs.get("rx", attrs.get("ry", 0)))
        ry = float(attrs.get("ry", attrs.get("rx", 0)))
        rx, ry = min(rx, w / 2), min(ry, h / 2)
        if rx == 0 and ry == 0:
            return f"M {fmt(x)} {fmt(y)} L {fmt(x + w)} {fmt(y)} L {fmt(x + w)} {fmt(y + h)} L {fmt(x)} {fmt(y + h)} Z"
        return (f"M {fmt(x + rx)} {fmt(y)} L {fmt(x + w - rx)} {fmt(y)} "
                f"A {fmt(rx)} {fmt(ry)} 0 0 1 {fmt(x + w)} {fmt(y + ry)} L {fmt(x + w)} {fmt(y + h - ry)} "
                f"A {fmt(rx)} {fmt(ry)} 0 0 1 {fmt(x + w - rx)} {fmt(y + h)} L {fmt(x + rx)} {fmt(y + h)} "
                f"A {fmt(rx)} {fmt(ry)} 0 0 1 {fmt(x)} {fmt(y + h - ry)} L {fmt(x)} {fmt(y + ry)} "
                f"A {fmt(rx)} {fmt(ry)} 0 0 1 {fmt(x + rx)} {fmt(y)} Z")
    if tag == "line":
        return f"M {fmt(f(attrs, 'x1'))} {fmt(f(attrs, 'y1'))} L {fmt(f(attrs, 'x2'))} {fmt(f(attrs, 'y2'))}"
    if tag in ("polyline", "polygon"):
        nums = [float(n) for n in NUMBER.findall(attrs["points"])]
        pts = list(zip(nums[0::2], nums[1::2]))
        d = f"M {fmt(pts[0][0])} {fmt(pts[0][1])} " + " ".join(f"L {fmt(px)} {fmt(py)}" for px, py in pts[1:])
        return d + (" Z" if tag == "polygon" else "")
    raise ValueError(tag)


def svg_to_path(svg: str) -> str:
    body = svg[svg.index(">", svg.index("<svg")) + 1:]
    parts = []
    for tag, raw in ELEMENT.findall(body):
        parts.append(element_to_path(tag, dict(ATTR.findall(raw))))
    if not parts:
        raise ValueError("no shapes")
    return arcs_to_curves(" ".join(parts))


def arc_to_cubics(x1, y1, rx, ry, rotation, large, sweep, x2, y2):
    """SVG elliptical arc -> cubic Bezier segments (c1x, c1y, c2x, c2y, x, y), each <= 90 degrees."""
    if (x1, y1) == (x2, y2):
        return []
    rx, ry = abs(rx), abs(ry)
    if rx == 0 or ry == 0:
        return [(x1, y1, x2, y2, x2, y2)]
    phi = math.radians(rotation)
    cos_phi, sin_phi = math.cos(phi), math.sin(phi)
    dx, dy = (x1 - x2) / 2, (y1 - y2) / 2
    x1p = cos_phi * dx + sin_phi * dy
    y1p = -sin_phi * dx + cos_phi * dy
    # Radii too small to span the endpoints are scaled up (SVG spec, F.6.6).
    scale = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry)
    if scale > 1:
        rx, ry = rx * math.sqrt(scale), ry * math.sqrt(scale)
    num = rx * rx * ry * ry - rx * rx * y1p * y1p - ry * ry * x1p * x1p
    den = rx * rx * y1p * y1p + ry * ry * x1p * x1p
    coef = math.sqrt(max(0.0, num / den)) * (-1 if large == sweep else 1)
    cxp, cyp = coef * rx * y1p / ry, -coef * ry * x1p / rx
    cx = cos_phi * cxp - sin_phi * cyp + (x1 + x2) / 2
    cy = sin_phi * cxp + cos_phi * cyp + (y1 + y2) / 2

    def angle(ux, uy, vx, vy):
        return math.atan2(ux * vy - uy * vx, ux * vx + uy * vy)

    ux, uy = (x1p - cxp) / rx, (y1p - cyp) / ry
    theta = angle(1, 0, ux, uy)
    delta = angle(ux, uy, (-x1p - cxp) / rx, (-y1p - cyp) / ry)
    if not sweep and delta > 0:
        delta -= 2 * math.pi
    elif sweep and delta < 0:
        delta += 2 * math.pi

    count = max(1, math.ceil(abs(delta) / (math.pi / 2) - 1e-9))
    step = delta / count
    t = 4 / 3 * math.tan(step / 4)

    def point(px, py):
        return (cx + rx * cos_phi * px - ry * sin_phi * py, cy + rx * sin_phi * px + ry * cos_phi * py)

    curves = []
    for i in range(count):
        a1 = theta + i * step
        a2 = a1 + step
        c1 = point(math.cos(a1) - t * math.sin(a1), math.sin(a1) + t * math.cos(a1))
        c2 = point(math.cos(a2) + t * math.sin(a2), math.sin(a2) - t * math.cos(a2))
        end = (x2, y2) if i == count - 1 else point(math.cos(a2), math.sin(a2))
        curves.append((*c1, *c2, *end))
    return curves


def arcs_to_curves(d: str) -> str:
    """Rewrite every arc (A) in an absolute path as cubic Beziers (C).

    The output then only uses M, L, C and Z, which every platform draws the same way. Arc
    handling differs per platform and is the fragile part: on Windows MAUI fails to draw some
    arcs at all (e.g. the radius-0.25 arcs in "activity"), which takes the app down.
    """
    tokens = d.split()
    sizes = {"M": 2, "L": 2, "T": 2, "C": 6, "S": 4, "Q": 4, "A": 7, "Z": 0}
    out = []
    x = y = sx = sy = 0.0
    i = 0
    while i < len(tokens):
        cmd = tokens[i]
        args = [float(v) for v in tokens[i + 1:i + 1 + sizes[cmd]]]
        i += 1 + sizes[cmd]
        if cmd == "A":
            rx, ry, rotation, large, sweep, nx, ny = args
            for curve in arc_to_cubics(x, y, rx, ry, rotation, bool(large), bool(sweep), nx, ny):
                out.append("C " + " ".join(fmt(v) for v in curve))
            x, y = nx, ny
            continue
        out.append(" ".join([cmd] + [fmt(v) for v in args]))
        if cmd == "Z":
            x, y = sx, sy
        else:
            x, y = args[-2], args[-1]
            if cmd == "M":
                sx, sy = x, y
    return " ".join(out)


def pascal(kebab: str) -> str:
    return "".join(p[:1].upper() + p[1:] for p in re.split(r"[-_]", kebab))


def main():
    catalog = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_CATALOG
    icons_dir = os.path.join(catalog, "catalog", "lucide", "icons")
    custom_dir = os.path.join(catalog, "catalog", "custom", "icons")
    version = open(os.path.join(catalog, "LUCIDE_VERSION.txt"), encoding="utf-8-sig").read().strip()

    entries = []
    for name in ICONS:
        src = os.path.join(custom_dir, f"{name}.svg")
        if not os.path.exists(src):
            src = os.path.join(icons_dir, f"{name}.svg")
        if not os.path.exists(src):
            raise SystemExit(f"icon not in catalog: {name}")
        entries.append((pascal(name), name, svg_to_path(open(src, encoding="utf-8").read())))

    enum_lines = ",\n".join(f"    {p}" for p, _, _ in entries)
    cases = "\n".join(f'        IconName.{p} => "{d}",' for p, _, d in entries)
    header = open(OUTPUT, encoding="utf-8").read()
    view = header[header.index("public partial class Icon : ContentView"):]

    out = f"""using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Stroke icons from ShellIcons (https://github.com/shellui-dev/shell-icons), derived from
// Lucide {version} (ISC license) — the icon set ShellUI uses. Drawn with MAUI shapes: no font or
// package, crisp at any size, theme-aware. Generated by scripts/generate-icons.py; a curated set
// of {len(entries)} icons — add names there and re-run to include more.
// Usage: <ui:Icon Name="Search" Size="16" />   Tint with Token="MutedForeground" or Color="Red".
public enum IconName
{{
    None,
{enum_lines}
}}

public static class IconPaths
{{
    // Absolute path data on Lucide's 24x24 grid, drawn with a 2px round stroke.
    public static string Get(IconName name) => name switch
    {{
{cases}
        _ => string.Empty
    }};
}}

{view}"""
    open(OUTPUT, "w", encoding="utf-8", newline="\n").write(out)
    print(f"wrote {len(entries)} icons (Lucide {version}) to {os.path.relpath(OUTPUT, REPO)}")


if __name__ == "__main__":
    main()
