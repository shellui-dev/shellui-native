using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace AvaloniaDemo.Components.UI;

// File picker — a dashed drop zone (rounded-lg border-2 border-dashed p-8) with an upload icon
// and a prompt; clicking it (or Enter / Space) opens the file picker, and files can be dropped on
// it. The picked files are listed underneath with a × to remove each.
//   <ui:FileUpload Multiple="True" MaxFiles="5" FilesChanged="OnFiles" />
// Read a file with IStorageFile.OpenReadAsync().
public class FileUpload : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<FileUpload, string?>(nameof(Text), "Drag and drop or click here to upload files");

    // More than one file can be picked; new picks are added to the list.
    public static readonly StyledProperty<bool> MultipleProperty =
        AvaloniaProperty.Register<FileUpload, bool>(nameof(Multiple));

    // The most files the list holds when Multiple is on. 0 means no limit.
    public static readonly StyledProperty<int> MaxFilesProperty =
        AvaloniaProperty.Register<FileUpload, int>(nameof(MaxFiles));

    // Which files the picker offers, e.g. FilePickerFileTypes.ImageAll. Null offers every file.
    public static readonly StyledProperty<IReadOnlyList<FilePickerFileType>?> FileTypesProperty =
        AvaloniaProperty.Register<FileUpload, IReadOnlyList<FilePickerFileType>?>(nameof(FileTypes));

    // The list of picked files under the drop zone.
    public static readonly StyledProperty<bool> ShowFilesProperty =
        AvaloniaProperty.Register<FileUpload, bool>(nameof(ShowFiles), true);

    private readonly List<IStorageFile> _files = new();
    private readonly Panel _zone;
    private readonly Rectangle _outline;
    private readonly TextBlock _text;
    private readonly TextBlock _hint;
    private readonly StackPanel _list = new() { Spacing = 4, Margin = new Thickness(0, 8, 0, 0), IsVisible = false };
    private bool _dragOver;
    private bool _picking;

    static FileUpload()
    {
        TextProperty.Changed.AddClassHandler<FileUpload>((f, _) => f.Update());
        MultipleProperty.Changed.AddClassHandler<FileUpload>((f, _) => f.Update());
        MaxFilesProperty.Changed.AddClassHandler<FileUpload>((f, _) => f.Update());
        ShowFilesProperty.Changed.AddClassHandler<FileUpload>((f, _) => f.Rebuild());
        IsEnabledProperty.Changed.AddClassHandler<FileUpload>((f, _) => f.Paint());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool Multiple
    {
        get => GetValue(MultipleProperty);
        set => SetValue(MultipleProperty, value);
    }

    public int MaxFiles
    {
        get => GetValue(MaxFilesProperty);
        set => SetValue(MaxFilesProperty, value);
    }

    public IReadOnlyList<FilePickerFileType>? FileTypes
    {
        get => GetValue(FileTypesProperty);
        set => SetValue(FileTypesProperty, value);
    }

    public bool ShowFiles
    {
        get => GetValue(ShowFilesProperty);
        set => SetValue(ShowFilesProperty, value);
    }

    public IReadOnlyList<IStorageFile> Files => _files;

    public event EventHandler<IReadOnlyList<IStorageFile>>? FilesChanged;

    public FileUpload()
    {
        _text = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap };
        _text.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _hint = new TextBlock { FontSize = 12, TextAlignment = TextAlignment.Center, IsVisible = false };
        _hint.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        // A Border has no dashed stroke, so the outline is a Rectangle behind the content.
        _outline = new Rectangle { StrokeThickness = 2, StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 3, 3 }, RadiusX = ShellTheme.RadiusLg, RadiusY = ShellTheme.RadiusLg };
        _zone = new Panel
        {
            // Transparent, not null, so the whole zone takes clicks and drops.
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand),
            Children =
            {
                _outline,
                new StackPanel
                {
                    Spacing = 4,
                    Margin = new Thickness(32),
                    Children =
                    {
                        new Icon { Kind = IconName.Upload, Size = 32, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) },
                        _text,
                        _hint
                    }
                }
            }
        };
        _zone.PointerEntered += (_, _) => Paint();
        _zone.PointerExited += (_, _) => Paint();
        _zone.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            _ = PickAsync();
            e.Handled = true;
        };

        DragDrop.SetAllowDrop(_zone, true);
        _zone.AddHandler(DragDrop.DragEnterEvent, (_, e) => SetDragOver(e, true));
        _zone.AddHandler(DragDrop.DragOverEvent, (_, e) => SetDragOver(e, true));
        _zone.AddHandler(DragDrop.DragLeaveEvent, (_, e) => SetDragOver(e, false));
        _zone.AddHandler(DragDrop.DropEvent, OnDrop);

        // The focus ring is a shadow, and a border with no background paints none.
        var ring = new Border { Child = _zone, CornerRadius = new CornerRadius(ShellTheme.RadiusLg), Background = Brushes.Transparent };
        Child = new StackPanel { Children = { ring, _list } };
        ShellFocus.Ring(this, ring);
        Update();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!ReferenceEquals(e.Source, this) || e.Key is not (Key.Enter or Key.Space)) return;
        _ = PickAsync();
        e.Handled = true;
    }

    // Opens the file picker and adds what was picked.
    public async Task PickAsync()
    {
        if (!IsEffectivelyEnabled || _picking || TopLevel.GetTopLevel(this) is not { } top) return;
        _picking = true;
        try
        {
            var picked = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = Multiple,
                FileTypeFilter = FileTypes
            });
            Add(picked);
        }
        catch (Exception)
        {
            // The picker isn't available here.
        }
        finally
        {
            _picking = false;
        }
    }

    // Adds files as a pick or a drop would: one file replaces the current one unless Multiple
    // is on, and the list never grows past MaxFiles.
    public void Add(IEnumerable<IStorageFile> files)
    {
        var changed = false;
        foreach (var file in files)
        {
            if (!Multiple)
            {
                _files.Clear();
                _files.Add(file);
                changed = true;
                break;
            }
            if (MaxFiles > 0 && _files.Count >= MaxFiles) break;
            if (_files.Any(f => f.Path == file.Path)) continue;
            _files.Add(file);
            changed = true;
        }
        if (changed) OnFilesChanged();
    }

    public void Remove(IStorageFile file)
    {
        if (_files.Remove(file)) OnFilesChanged();
    }

    public void Clear()
    {
        if (_files.Count == 0) return;
        _files.Clear();
        OnFilesChanged();
    }

    private void OnFilesChanged()
    {
        Rebuild();
        FilesChanged?.Invoke(this, _files.ToList());
    }

    private void SetDragOver(DragEventArgs e, bool over)
    {
        e.DragEffects = over && IsEffectivelyEnabled ? DragDropEffects.Copy : DragDropEffects.None;
        _dragOver = over && IsEffectivelyEnabled;
        Paint();
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        _dragOver = false;
        Paint();
        if (!IsEffectivelyEnabled) return;
        if (e.DataTransfer.TryGetFiles() is { } items) Add(items.OfType<IStorageFile>());
        e.Handled = true;
    }

    private void Rebuild()
    {
        _list.Children.Clear();
        _list.IsVisible = ShowFiles && _files.Count > 0;
        if (!ShowFiles) return;
        foreach (var file in _files)
            _list.Children.Add(CreateRow(file));
    }

    // rounded-md border px-3 h-10: file icon, name, × to remove.
    private Control CreateRow(IStorageFile file)
    {
        var name = new TextBlock { Text = file.Name, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        name.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        Grid.SetColumn(name, 1);
        var close = new Icon { Kind = IconName.X, Size = 14, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // 14px icon in a 24px click target.
        var remove = new Border { Width = 24, Height = 24, Background = Brushes.Transparent, Child = close, VerticalAlignment = VerticalAlignment.Center, Cursor = new Cursor(StandardCursorType.Hand) };
        AutomationProperties.SetName(remove, $"Remove {file.Name}");
        remove.PointerEntered += (_, _) => close.Token = ShellToken.Destructive;
        remove.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        remove.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            if (IsEffectivelyEnabled) Remove(file);
            e.Handled = true;
        };
        Grid.SetColumn(remove, 2);

        var row = new Border
        {
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                ColumnSpacing = 8,
                Children = { new Icon { Kind = IconName.File, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center }, name, remove }
            },
            Height = 40,
            Padding = new Thickness(12, 0, 8, 0),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd)
        };
        row.Token(BorderBrushProperty, ShellToken.Border);
        return row;
    }

    private void Update()
    {
        _text.Text = Text ?? string.Empty;
        _hint.Text = MaxFiles > 1 ? $"Select up to {MaxFiles} files" : string.Empty;
        _hint.IsVisible = Multiple && MaxFiles > 1;
        AutomationProperties.SetName(this, Text ?? string.Empty);
        Paint();
    }

    private void Paint()
    {
        var active = _dragOver || (_zone.IsPointerOver && IsEffectivelyEnabled);
        _outline.Token(Shape.StrokeProperty, _dragOver ? ShellToken.Primary : active ? ShellToken.Ring : ShellToken.Border);
        if (active) _outline.Token(Shape.FillProperty, ShellToken.Muted);
        else
        {
            _outline.ClearToken(Shape.FillProperty);
            _outline.Fill = null;
        }
        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
