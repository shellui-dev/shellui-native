using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// File picker — a dashed drop zone (rounded-lg border-2 border-dashed p-8) with an upload icon
// and a prompt; tapping it opens the platform file picker. On Windows, files can also be dropped
// on it. The picked files are listed underneath with a × to remove each.
//   <ui:FileUpload Multiple="True" MaxFiles="5" FilesChanged="OnFiles" />
// Read a file with FileResult.OpenReadAsync().
public partial class FileUpload : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FileUpload), "Drag and drop or click here to upload files",
            propertyChanged: (b, o, n) => ((FileUpload)b).Update());

    // More than one file can be picked; new picks are added to the list.
    public static readonly BindableProperty MultipleProperty =
        BindableProperty.Create(nameof(Multiple), typeof(bool), typeof(FileUpload), false,
            propertyChanged: (b, o, n) => ((FileUpload)b).Update());

    // The most files the list holds when Multiple is on. 0 means no limit.
    public static readonly BindableProperty MaxFilesProperty =
        BindableProperty.Create(nameof(MaxFiles), typeof(int), typeof(FileUpload), 0,
            propertyChanged: (b, o, n) => ((FileUpload)b).Update());

    // Which files the picker offers, e.g. FilePickerFileType.Images. Null offers every file.
    public static readonly BindableProperty FileTypesProperty =
        BindableProperty.Create(nameof(FileTypes), typeof(FilePickerFileType), typeof(FileUpload), null);

    // The list of picked files under the drop zone.
    public static readonly BindableProperty ShowFilesProperty =
        BindableProperty.Create(nameof(ShowFiles), typeof(bool), typeof(FileUpload), true,
            propertyChanged: (b, o, n) => ((FileUpload)b).Rebuild());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool Multiple
    {
        get => (bool)GetValue(MultipleProperty);
        set => SetValue(MultipleProperty, value);
    }

    public int MaxFiles
    {
        get => (int)GetValue(MaxFilesProperty);
        set => SetValue(MaxFilesProperty, value);
    }

    public FilePickerFileType? FileTypes
    {
        get => (FilePickerFileType?)GetValue(FileTypesProperty);
        set => SetValue(FileTypesProperty, value);
    }

    public bool ShowFiles
    {
        get => (bool)GetValue(ShowFilesProperty);
        set => SetValue(ShowFilesProperty, value);
    }

    public IReadOnlyList<FileResult> Files => _files;

    public event EventHandler<IReadOnlyList<FileResult>>? FilesChanged;

    private readonly List<FileResult> _files = new();
    private readonly Border _zone;
    private readonly Label _text;
    private readonly Label _hint;
    private readonly VerticalStackLayout _list;
    private bool _hovered;
    private bool _dragOver;
    private bool _picking;

    public FileUpload()
    {
        _text = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
        _text.Token(Label.TextColorProperty, ShellToken.Foreground);
        _hint = new Label { FontSize = 12, HorizontalTextAlignment = TextAlignment.Center, IsVisible = false };
        _hint.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        _zone = new Border
        {
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Icon { Name = IconName.Upload, Size = 32, Token = ShellToken.MutedForeground, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, 0, 8) },
                    _text,
                    _hint
                }
            },
            Padding = new Thickness(32),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 3, 3 },
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Paint(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Paint(); };
        _zone.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); _ = PickAsync(); };
        _zone.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => _ = PickAsync());

        var drop = new DropGestureRecognizer();
        drop.DragOver += (_, e) =>
        {
            if (!IsEnabled) return;
            e.AcceptedOperation = DataPackageOperation.Copy;
            _dragOver = true;
            Paint();
        };
        drop.DragLeave += (_, _) => { _dragOver = false; Paint(); };
        drop.Drop += OnDrop;
        _zone.GestureRecognizers.Add(drop);

        _list = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(0, 8, 0, 0), IsVisible = false };
        Content = new VerticalStackLayout { Spacing = 0, Children = { _zone, _list } };
        Update();
    }

    // Opens the platform file picker and adds what was picked.
    public async Task PickAsync()
    {
        if (!IsEnabled || _picking) return;
        _picking = true;
        try
        {
            var options = new PickOptions { FileTypes = FileTypes };
            if (Multiple)
            {
                var picked = await FilePicker.Default.PickMultipleAsync(options);
                if (picked != null) Add(picked.Where(f => f != null)!);
            }
            else
            {
                var picked = await FilePicker.Default.PickAsync(options);
                if (picked != null) Add(new[] { picked });
            }
        }
        catch (Exception)
        {
            // The picker was cancelled or isn't available here.
        }
        finally
        {
            _picking = false;
        }
    }

    // Adds files as a pick or a drop would: one file replaces the current one unless Multiple
    // is on, and the list never grows past MaxFiles.
    public void Add(IEnumerable<FileResult> files)
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
            if (_files.Any(f => f.FullPath == file.FullPath)) continue;
            _files.Add(file);
            changed = true;
        }
        if (changed) OnFilesChanged();
    }

    public void Remove(FileResult file)
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

    private async void OnDrop(object? sender, DropEventArgs e)
    {
        _dragOver = false;
        Paint();
        if (!IsEnabled) return;
#if WINDOWS
        try
        {
            if (e.PlatformArgs?.DragEventArgs.DataView is not { } data ||
                !data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems)) return;
            var items = await data.GetStorageItemsAsync();
            Add(items.OfType<Windows.Storage.StorageFile>().Select(f => new FileResult(f.Path)));
        }
        catch (Exception)
        {
            // The dropped data couldn't be read as files.
        }
#else
        await Task.CompletedTask;
#endif
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
    private View CreateRow(FileResult file)
    {
        var name = new Label { Text = file.FileName, FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.MiddleTruncation };
        name.Token(Label.TextColorProperty, ShellToken.Foreground);
        var close = new Icon { Name = IconName.X, Size = 14, Token = ShellToken.MutedForeground, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        // 14px icon in a 24px tap target.
        var remove = new Grid { WidthRequest = 24, HeightRequest = 24, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center, Children = { close } };
        SemanticProperties.SetDescription(remove, $"Remove {file.FileName}");
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => close.Token = ShellToken.Destructive;
        pointer.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        remove.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsEnabled) Remove(file); };
        remove.GestureRecognizers.Add(tap);

        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        grid.Add(new Icon { Name = IconName.File, Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Center }, 0, 0);
        grid.Add(name, 1, 0);
        grid.Add(remove, 2, 0);

        var row = new Border
        {
            Content = grid,
            HeightRequest = 40,
            Padding = new Thickness(12, 0, 8, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        row.Token(Border.StrokeProperty, ShellToken.Border);
        return row;
    }

    private void Update()
    {
        _text.Text = Text ?? string.Empty;
        _hint.Text = MaxFiles > 1 ? $"Select up to {MaxFiles} files" : string.Empty;
        _hint.IsVisible = Multiple && MaxFiles > 1;
        SemanticProperties.SetDescription(this, Text ?? string.Empty);
        Paint();
    }

    private void Paint()
    {
        var active = _dragOver || (_hovered && IsEnabled);
        _zone.Token(Border.StrokeProperty, _dragOver ? ShellToken.Primary : active ? ShellToken.Ring : ShellToken.Border);
        if (active) _zone.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        else
        {
            _zone.ClearValue(VisualElement.BackgroundColorProperty);
            _zone.BackgroundColor = Colors.Transparent;
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _zone != null)
        {
            Opacity = IsEnabled ? 1.0 : 0.5;
            Paint();
        }
    }
}
