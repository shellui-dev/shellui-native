using ShellUI.Native.CLI.Services;

namespace ShellUI.Native.Tests;

public class LoadersTests
{
    [Fact]
    public void SnakeSpinner_walks_the_grid_edge_with_a_three_dot_trail()
    {
        var frames = SnakeSpinner.Instance.Frames;

        Assert.Equal(8, frames.Count);
        Assert.Equal(8, frames.Distinct().Count());
        foreach (var frame in frames)
        {
            Assert.Equal(2, frame.Length);
            Assert.All(frame, ch => Assert.InRange(ch, '⠀', '⣿'));
            Assert.Equal(3, frame.Sum(ch => System.Numerics.BitOperations.PopCount((uint)(ch - 0x2800))));
        }

        // The centre cell (row 1, column 1 → first character, dot 0x10) is never part of the snake path.
        Assert.DoesNotContain(frames, f => ((f[0] - 0x2800) & 0x10) != 0);
    }

    [Fact]
    public void Braille_maps_grid_cells_to_dots()
    {
        Assert.Equal("⠉⠁", SnakeSpinner.Braille(i => i < 3));
        Assert.Equal("⠇⠀", SnakeSpinner.Braille(i => i % 3 == 0));
    }

    // Same rows as the ShellUI Native mark in md-files BRAND_MARKS.md (two screens).
    [Fact]
    public void LogoLoader_builds_the_two_screens_mark_along_the_diagonal_then_clears_it()
    {
        Assert.Equal(new[] { "XXX..", "X.X..", "XXXXX", "..X.X", "..XXX" }, LogoLoader.Pattern);
        Assert.Equal(new[] { "X....", ".....", ".....", ".....", "....." }, LogoLoader.Frame(0));
        Assert.Equal(LogoLoader.Pattern, LogoLoader.Frame(8));
        Assert.Equal(LogoLoader.Frame(8), LogoLoader.Frame(14));
        Assert.Equal(new[] { ".....", ".....", ".....", ".....", "....." }, LogoLoader.Frame(29));
        Assert.Equal(LogoLoader.Frame(0), LogoLoader.Frame(30));
    }
}
