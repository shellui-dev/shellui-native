using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaDemo.Components.UI;

namespace ShellUI.Native.Avalonia.Tests;

// Shows controls in a headless window and drives them through the real input pipeline:
// pointer events land on whatever is under the point, keys go to the focused control.
public static class Ui
{
    // Windows from earlier tests, closed when the next one shows.
    private static readonly List<Window> Shown = new();

    public static Window Show(Control content, ThemeVariant? theme = null, double width = 900, double height = 700)
    {
        foreach (var open in Shown) open.Close();
        Shown.Clear();
        ShellTheme.SetTheme(theme ?? ThemeVariant.Light);
        var window = new Window { Content = content, Width = width, Height = height };
        Shown.Add(window);
        window.Show();
        Settle();
        return window;
    }

    // Runs queued work, layout and a render tick, so the next step sees the result of the last one.
    public static void Settle()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }
    // Lets animations and transitions (popups open in about 120 ms) run for `milliseconds`.
    public static void Wait(int milliseconds)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (clock.ElapsedMilliseconds < milliseconds)
        {
            System.Threading.Thread.Sleep(16);
            Settle();
        }
    }

    public static Point CenterOf(Window window, Visual target)
    {
        var center = new Point(target.Bounds.Width / 2, target.Bounds.Height / 2);
        return target.TranslatePoint(center, window) ?? throw new InvalidOperationException($"{target.GetType().Name} is not in the window");
    }

    public static void Click(Window window, Visual target) => ClickAt(window, CenterOf(window, target));

    public static void ClickAt(Window window, Point point)
    {
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Settle();
    }

    public static void Hover(Window window, Visual target)
    {
        window.MouseMove(CenterOf(window, target), RawInputModifiers.None);
        Settle();
    }

    public static void Press(Window window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, PhysicalKey.None, null);
        window.KeyRelease(key, modifiers, PhysicalKey.None, null);
        Settle();
    }

    public static IInputElement? Focused(Window window) => window.FocusManager?.GetFocusedElement();

    // Where popups, menus and overlays show (they stay in the window's visual tree).
    public static Visual Overlay(Window window) => OverlayLayer.GetOverlayLayer(window) ?? (Visual)window;

    // True when `element` is `ancestor` or inside it in the visual tree.
    public static bool IsWithin(object? element, Visual ancestor) =>
        element is Visual v && (ReferenceEquals(v, ancestor) || v.GetVisualAncestors().Contains(ancestor));
}
