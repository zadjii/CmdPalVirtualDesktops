// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;

namespace Zadjii.CmdPal.VirtualDesktops;

public class VirtualDesktopSettings : JsonSettingsManager
{
    internal const string WallpaperValue = "wallpaper";
    internal const string CircleFillBadge12Value = "circleFillBadge12";
    internal const string ToggleFilledValue = "toggleFilled";
    internal const string CheckboxFillValue = "checkboxFill";
    internal const string CheckboxEmptyValue = "checkboxEmpty";

    private static readonly string _namespace = "virtualDesktops";

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

    private static readonly List<ChoiceSetSetting.Choice> _iconChoices =
    [
        new("Dot", CircleFillBadge12Value),
        new("Pill", ToggleFilledValue),
        new("Filled square", CheckboxFillValue),
        new("Empty square", CheckboxEmptyValue),
        new("Desktop wallpaper", WallpaperValue),
    ];

#pragma warning disable SA1401 // Fields should be private
    internal static VirtualDesktopSettings Instance = new();
#pragma warning restore SA1401

    public string ActiveDesktopIcon => _activeDesktopIcon.Value ?? ToggleFilledValue;

    public string InactiveDesktopIcon => _inactiveDesktopIcon.Value ?? CircleFillBadge12Value;

    private readonly ChoiceSetSetting _activeDesktopIcon = new(
        Namespaced(nameof(ActiveDesktopIcon)),
        "Active desktop icon",
        "The icon to display for the currently active desktop in the band",
        _iconChoices);

    private readonly ChoiceSetSetting _inactiveDesktopIcon = new(
        Namespaced(nameof(InactiveDesktopIcon)),
        "Inactive desktop icon",
        "The icon to display for inactive desktops in the band",
        _iconChoices);

    internal static string SettingsJsonPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Zadjii.CmdPal.VirtualDesktops");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "settings.json");
    }

    public VirtualDesktopSettings()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_activeDesktopIcon);
        Settings.Add(_inactiveDesktopIcon);
        // default to pill for the active desktop
        _activeDesktopIcon.Value = _iconChoices[1].Value;

        LoadSettings();

        Settings.SettingsChanged += (s, a) => SaveSettings();
    }

    public static IconInfo GetIconForValue(string value, string? wallpaperPath = null)
    {
        return value switch
        {
            CircleFillBadge12Value => Icons.CircleFillBadge12Icon,
            ToggleFilledValue => Icons.ToggleFilledIcon,
            CheckboxFillValue => Icons.CheckboxFillIcon,
            CheckboxEmptyValue => Icons.CheckboxEmptyIcon,
            WallpaperValue when wallpaperPath is not null => new IconInfo(wallpaperPath),
            _ => Icons.CircleFillBadge12Icon,
        };
    }
}
