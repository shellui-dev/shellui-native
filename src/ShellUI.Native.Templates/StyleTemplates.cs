namespace ShellUI.Native.Templates;

// XAML theme resources written by `shellui-native init` (Resources/Styles/ShellUITheme.xaml).
// Values mirror ShellTheme.Light / ShellTheme.Dark in the `shell` template; keep them in sync.
public static class StyleTemplates
{
    // Theme ResourceDictionary template for MAUI App.xaml
    public static string ThemeResourceDictionary => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ResourceDictionary xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <!-- ShellUI Native theme tokens (Light). Generated from ShellTheme.Light in Components/UI/Shell.cs.
         At runtime ShellTheme publishes the active palette (and a matching ShellUI*Brush for each
         token) into Application.Resources and swaps it on theme change, so components don't need
         this file. Merge it for design-time previews, or use the keys in your own XAML:
         TextColor=""{DynamicResource ShellUIMutedForeground}"". -->
    <Color x:Key=""ShellUIBackground"">#FCFCFC</Color>
    <Color x:Key=""ShellUIForeground"">#0A0A0A</Color>
    <Color x:Key=""ShellUICard"">#FFFFFF</Color>
    <Color x:Key=""ShellUICardForeground"">#0A0A0A</Color>
    <Color x:Key=""ShellUIPopover"">#FFFFFF</Color>
    <Color x:Key=""ShellUIPopoverForeground"">#0A0A0A</Color>
    <Color x:Key=""ShellUIPrimary"">#171717</Color>
    <Color x:Key=""ShellUIPrimaryForeground"">#FAFAFA</Color>
    <Color x:Key=""ShellUISecondary"">#F0F0F0</Color>
    <Color x:Key=""ShellUISecondaryForeground"">#171717</Color>
    <Color x:Key=""ShellUIMuted"">#F5F5F5</Color>
    <Color x:Key=""ShellUIMutedForeground"">#737373</Color>
    <Color x:Key=""ShellUIAccent"">#F0F0F0</Color>
    <Color x:Key=""ShellUIAccentForeground"">#171717</Color>
    <Color x:Key=""ShellUIDestructive"">#E54B4F</Color>
    <Color x:Key=""ShellUIDestructiveForeground"">#FFFFFF</Color>
    <Color x:Key=""ShellUISuccess"">#16A34A</Color>
    <Color x:Key=""ShellUISuccessForeground"">#FFFFFF</Color>
    <Color x:Key=""ShellUIWarning"">#D97706</Color>
    <Color x:Key=""ShellUIWarningForeground"">#FFFFFF</Color>
    <Color x:Key=""ShellUIInfo"">#2563EB</Color>
    <Color x:Key=""ShellUIInfoForeground"">#FFFFFF</Color>
    <Color x:Key=""ShellUIBorder"">#E4E4E4</Color>
    <Color x:Key=""ShellUIInput"">#E4E4E4</Color>
    <Color x:Key=""ShellUIRing"">#A3A3A3</Color>
    <Color x:Key=""ShellUIOverlay"">#80000000</Color>

    <!-- Spacing -->
    <x:Double x:Key=""ShellUISpacingXs"">4</x:Double>
    <x:Double x:Key=""ShellUISpacingSm"">8</x:Double>
    <x:Double x:Key=""ShellUISpacingMd"">16</x:Double>
    <x:Double x:Key=""ShellUISpacingLg"">24</x:Double>
    <x:Double x:Key=""ShellUISpacingXl"">32</x:Double>

    <!-- Border Radius (base radius 0.5rem = 8) -->
    <CornerRadius x:Key=""ShellUIRadiusSm"">4</CornerRadius>
    <CornerRadius x:Key=""ShellUIRadiusMd"">6</CornerRadius>
    <CornerRadius x:Key=""ShellUIRadiusLg"">8</CornerRadius>
    <CornerRadius x:Key=""ShellUIRadiusXl"">12</CornerRadius>
    <CornerRadius x:Key=""ShellUIRadiusFull"">9999</CornerRadius>

    <!-- Font Sizes -->
    <x:Double x:Key=""ShellUIFontXs"">12</x:Double>
    <x:Double x:Key=""ShellUIFontSm"">14</x:Double>
    <x:Double x:Key=""ShellUIFontBase"">16</x:Double>
    <x:Double x:Key=""ShellUIFontLg"">18</x:Double>
    <x:Double x:Key=""ShellUIFontXl"">20</x:Double>
    <x:Double x:Key=""ShellUIFont2Xl"">24</x:Double>

</ResourceDictionary>";

    // Dark theme variant
    public static string DarkThemeResourceDictionary => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ResourceDictionary xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">

    <!-- ShellUI Native theme tokens (Dark). Generated from ShellTheme.Dark in Components/UI/Shell.cs.
         At runtime ShellTheme publishes the active palette (and a matching ShellUI*Brush for each
         token) into Application.Resources and swaps it on theme change, so components don't need
         this file. Merge it for design-time previews, or use the keys in your own XAML:
         TextColor=""{DynamicResource ShellUIMutedForeground}"". -->
    <Color x:Key=""ShellUIBackground"">#0A0A0A</Color>
    <Color x:Key=""ShellUIForeground"">#FAFAFA</Color>
    <Color x:Key=""ShellUICard"">#141414</Color>
    <Color x:Key=""ShellUICardForeground"">#FAFAFA</Color>
    <Color x:Key=""ShellUIPopover"">#171717</Color>
    <Color x:Key=""ShellUIPopoverForeground"">#FAFAFA</Color>
    <Color x:Key=""ShellUIPrimary"">#FAFAFA</Color>
    <Color x:Key=""ShellUIPrimaryForeground"">#171717</Color>
    <Color x:Key=""ShellUISecondary"">#262626</Color>
    <Color x:Key=""ShellUISecondaryForeground"">#FAFAFA</Color>
    <Color x:Key=""ShellUIMuted"">#262626</Color>
    <Color x:Key=""ShellUIMutedForeground"">#A3A3A3</Color>
    <Color x:Key=""ShellUIAccent"">#262626</Color>
    <Color x:Key=""ShellUIAccentForeground"">#FAFAFA</Color>
    <Color x:Key=""ShellUIDestructive"">#FF5B5B</Color>
    <Color x:Key=""ShellUIDestructiveForeground"">#0A0A0A</Color>
    <Color x:Key=""ShellUISuccess"">#22C55E</Color>
    <Color x:Key=""ShellUISuccessForeground"">#0A0A0A</Color>
    <Color x:Key=""ShellUIWarning"">#F59E0B</Color>
    <Color x:Key=""ShellUIWarningForeground"">#0A0A0A</Color>
    <Color x:Key=""ShellUIInfo"">#3B82F6</Color>
    <Color x:Key=""ShellUIInfoForeground"">#FFFFFF</Color>
    <Color x:Key=""ShellUIBorder"">#2A2A2A</Color>
    <Color x:Key=""ShellUIInput"">#333333</Color>
    <Color x:Key=""ShellUIRing"">#737373</Color>
    <Color x:Key=""ShellUIOverlay"">#B3000000</Color>

</ResourceDictionary>";
}
