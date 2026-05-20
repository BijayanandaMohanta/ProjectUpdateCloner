using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace ProjectUpdateCloner.Helpers;

public static class SystemThemeManager
{
    private const string PersonalizeRegistryPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";
    private static System.Windows.Application? application;

    public static void Start(System.Windows.Application app)
    {
        application = app;
        ApplySystemTheme();
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General
            or UserPreferenceCategory.VisualStyle
            or UserPreferenceCategory.Color)
        {
            application?.Dispatcher.BeginInvoke(ApplySystemTheme);
        }
    }

    private static void ApplySystemTheme()
    {
        if (application is null)
        {
            return;
        }

        if (IsLightTheme())
        {
            ApplyLightTheme(application.Resources);
        }
        else
        {
            ApplyDarkTheme(application.Resources);
        }
    }

    private static bool IsLightTheme()
    {
        var value = Registry.GetValue(PersonalizeRegistryPath, AppsUseLightThemeValue, 1);
        return value is not int intValue || intValue != 0;
    }

    private static void ApplyLightTheme(ResourceDictionary resources)
    {
        Set(resources, "SurfaceBrush", "#F3F5F7");
        Set(resources, "PanelBrush", "#FFFFFF");
        Set(resources, "PanelAltBrush", "#F8FAFC");
        Set(resources, "InputBrush", "#FFFFFF");
        Set(resources, "BorderBrush", "#D7DEE8");
        Set(resources, "BorderStrongBrush", "#B9C3D0");
        Set(resources, "AccentBrush", "#0E7C66");
        Set(resources, "AccentHoverBrush", "#0A6654");
        Set(resources, "AccentSoftBrush", "#E6F4F1");
        Set(resources, "AccentBorderBrush", "#BFE5DD");
        Set(resources, "TextBrush", "#172033");
        Set(resources, "MutedTextBrush", "#667085");
        Set(resources, "ConsoleBrush", "#111827");
        Set(resources, "ConsoleTextBrush", "#D1FAE5");
        Set(resources, "ConsoleHeaderBrush", "#1F2937");
        Set(resources, "ConsoleProgressStripBrush", "#162031");
        Set(resources, "ConsoleBorderBrush", "#263241");
        Set(resources, "ButtonHoverBrush", "#EEF2F6");
        Set(resources, "ButtonPressedBrush", "#E4E9F0");
        Set(resources, "ProgressTrackBrush", "#334155");
        Set(resources, "CopiedBadgeBrush", "#E6F4F1");
        Set(resources, "CopiedBadgeBorderBrush", "#BFE5DD");
        Set(resources, "CopiedBadgeTextBrush", "#0E7C66");
        Set(resources, "SkippedBadgeBrush", "#FFF7E6");
        Set(resources, "SkippedBadgeBorderBrush", "#F5C16C");
        Set(resources, "SkippedBadgeTextBrush", "#8A5A00");
        Set(resources, "MatchedBadgeBrush", "#EEF4FF");
        Set(resources, "MatchedBadgeBorderBrush", "#B8CCFF");
        Set(resources, "MatchedBadgeTextBrush", "#2563EB");
        Set(resources, "ErrorBadgeBrush", "#FEF2F2");
        Set(resources, "ErrorBadgeBorderBrush", "#FECACA");
        Set(resources, "ErrorBadgeTextBrush", "#DC2626");
        Set(resources, "WarningBackgroundBrush", "#FFF7ED");
        Set(resources, "WarningBorderBrush", "#FDBA74");
        Set(resources, "WarningBadgeBrush", "#F97316");
        Set(resources, "WarningTextBrush", "#7C2D12");
        SetImage(resources, "HeaderLogoSource", "pack://application:,,,/Assets/inside-logo.png");
    }

    private static void ApplyDarkTheme(ResourceDictionary resources)
    {
        Set(resources, "SurfaceBrush", "#0F172A");
        Set(resources, "PanelBrush", "#111827");
        Set(resources, "PanelAltBrush", "#0B1220");
        Set(resources, "InputBrush", "#0F172A");
        Set(resources, "BorderBrush", "#334155");
        Set(resources, "BorderStrongBrush", "#475569");
        Set(resources, "AccentBrush", "#14B8A6");
        Set(resources, "AccentHoverBrush", "#0F766E");
        Set(resources, "AccentSoftBrush", "#123C3A");
        Set(resources, "AccentBorderBrush", "#1F766D");
        Set(resources, "TextBrush", "#E5E7EB");
        Set(resources, "MutedTextBrush", "#94A3B8");
        Set(resources, "ConsoleBrush", "#020617");
        Set(resources, "ConsoleTextBrush", "#BBF7D0");
        Set(resources, "ConsoleHeaderBrush", "#0B1220");
        Set(resources, "ConsoleProgressStripBrush", "#08111F");
        Set(resources, "ConsoleBorderBrush", "#1E293B");
        Set(resources, "ButtonHoverBrush", "#1E293B");
        Set(resources, "ButtonPressedBrush", "#273449");
        Set(resources, "ProgressTrackBrush", "#1E293B");
        Set(resources, "CopiedBadgeBrush", "#102E2B");
        Set(resources, "CopiedBadgeBorderBrush", "#2DD4BF");
        Set(resources, "CopiedBadgeTextBrush", "#99F6E4");
        Set(resources, "SkippedBadgeBrush", "#332510");
        Set(resources, "SkippedBadgeBorderBrush", "#A16207");
        Set(resources, "SkippedBadgeTextBrush", "#FBBF24");
        Set(resources, "MatchedBadgeBrush", "#172554");
        Set(resources, "MatchedBadgeBorderBrush", "#1D4ED8");
        Set(resources, "MatchedBadgeTextBrush", "#93C5FD");
        Set(resources, "ErrorBadgeBrush", "#3B1111");
        Set(resources, "ErrorBadgeBorderBrush", "#991B1B");
        Set(resources, "ErrorBadgeTextBrush", "#FCA5A5");
        Set(resources, "WarningBackgroundBrush", "#2D1B0E");
        Set(resources, "WarningBorderBrush", "#C2410C");
        Set(resources, "WarningBadgeBrush", "#EA580C");
        Set(resources, "WarningTextBrush", "#FDBA74");
        SetImage(resources, "HeaderLogoSource", "pack://application:,,,/Assets/inside-logo-dark.png");
    }

    private static void Set(ResourceDictionary resources, string key, string color)
    {
        if (resources.Contains(key))
        {
            resources[key] = new SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
        }
    }

    private static void SetImage(ResourceDictionary resources, string key, string path)
    {
        if (resources.Contains(key))
        {
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            resources[key] = image;
        }
    }
}
