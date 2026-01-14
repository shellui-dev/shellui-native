namespace ShellUI.Native.Templates;

// XAML style templates for theming
public static class StyleTemplates
{
    // Theme ResourceDictionary template for MAUI App.xaml
    public static string ThemeResourceDictionary => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ResourceDictionary xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
    
    <!-- ShellUI Native Theme - Design tokens mapped from ShellUI CSS variables -->
    
    <!-- Background Colors -->
    <Color x:Key=""ShellUIBackground"">#FFFFFF</Color>
    <Color x:Key=""ShellUIForeground"">#0A0A0A</Color>
    
    <!-- Primary Colors -->
    <Color x:Key=""ShellUIPrimary"">#2563EB</Color>
    <Color x:Key=""ShellUIPrimaryForeground"">#FFFFFF</Color>
    
    <!-- Secondary Colors -->
    <Color x:Key=""ShellUISecondary"">#F4F4F5</Color>
    <Color x:Key=""ShellUISecondaryForeground"">#18181B</Color>
    
    <!-- Destructive Colors -->
    <Color x:Key=""ShellUIDestructive"">#EF4444</Color>
    <Color x:Key=""ShellUIDestructiveForeground"">#FFFFFF</Color>
    
    <!-- Muted Colors -->
    <Color x:Key=""ShellUIMuted"">#F4F4F5</Color>
    <Color x:Key=""ShellUIMutedForeground"">#71717A</Color>
    
    <!-- Accent Colors -->
    <Color x:Key=""ShellUIAccent"">#F4F4F5</Color>
    <Color x:Key=""ShellUIAccentForeground"">#18181B</Color>
    
    <!-- Border and Input -->
    <Color x:Key=""ShellUIBorder"">#E4E4E7</Color>
    <Color x:Key=""ShellUIInput"">#E4E4E7</Color>
    <Color x:Key=""ShellUIRing"">#2563EB</Color>
    
    <!-- Card Colors -->
    <Color x:Key=""ShellUICard"">#FFFFFF</Color>
    <Color x:Key=""ShellUICardForeground"">#0A0A0A</Color>
    
    <!-- Popover Colors -->
    <Color x:Key=""ShellUIPopover"">#FFFFFF</Color>
    <Color x:Key=""ShellUIPopoverForeground"">#0A0A0A</Color>
    
    <!-- Spacing -->
    <x:Double x:Key=""ShellUISpacingXs"">4</x:Double>
    <x:Double x:Key=""ShellUISpacingSm"">8</x:Double>
    <x:Double x:Key=""ShellUISpacingMd"">16</x:Double>
    <x:Double x:Key=""ShellUISpacingLg"">24</x:Double>
    <x:Double x:Key=""ShellUISpacingXl"">32</x:Double>
    
    <!-- Border Radius -->
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
    
    <!-- ShellUI Native Dark Theme -->
    
    <!-- Background Colors -->
    <Color x:Key=""ShellUIBackground"">#0A0A0A</Color>
    <Color x:Key=""ShellUIForeground"">#FAFAFA</Color>
    
    <!-- Primary Colors -->
    <Color x:Key=""ShellUIPrimary"">#3B82F6</Color>
    <Color x:Key=""ShellUIPrimaryForeground"">#FFFFFF</Color>
    
    <!-- Secondary Colors -->
    <Color x:Key=""ShellUISecondary"">#27272A</Color>
    <Color x:Key=""ShellUISecondaryForeground"">#FAFAFA</Color>
    
    <!-- Destructive Colors -->
    <Color x:Key=""ShellUIDestructive"">#EF4444</Color>
    <Color x:Key=""ShellUIDestructiveForeground"">#FFFFFF</Color>
    
    <!-- Muted Colors -->
    <Color x:Key=""ShellUIMuted"">#27272A</Color>
    <Color x:Key=""ShellUIMutedForeground"">#A1A1AA</Color>
    
    <!-- Accent Colors -->
    <Color x:Key=""ShellUIAccent"">#27272A</Color>
    <Color x:Key=""ShellUIAccentForeground"">#FAFAFA</Color>
    
    <!-- Border and Input -->
    <Color x:Key=""ShellUIBorder"">#27272A</Color>
    <Color x:Key=""ShellUIInput"">#27272A</Color>
    <Color x:Key=""ShellUIRing"">#3B82F6</Color>
    
    <!-- Card Colors -->
    <Color x:Key=""ShellUICard"">#0A0A0A</Color>
    <Color x:Key=""ShellUICardForeground"">#FAFAFA</Color>
    
    <!-- Popover Colors -->
    <Color x:Key=""ShellUIPopover"">#0A0A0A</Color>
    <Color x:Key=""ShellUIPopoverForeground"">#FAFAFA</Color>
    
</ResourceDictionary>";
}
