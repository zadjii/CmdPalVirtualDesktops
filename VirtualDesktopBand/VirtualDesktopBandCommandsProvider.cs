// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsDesktop;

namespace VirtualDesktopBand;

public partial class VirtualDesktopBandCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public VirtualDesktopBandCommandsProvider()
    {
        DisplayName = "Deskband for virtual desktops";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new VirtualDesktopsListPage()) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}


public partial class VirtualDesktopsListPage : ListPage
{
    //private DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();
    TaskScheduler _scheduler;//  = TaskScheduler.Current;// TaskScheduler.FromCurrentSynchronizationContext();

    public override string Id => "com.zadjii.virtualDesktops.band";

    public static readonly IconInfo CheckboxEmptyIcon = new("\uE739");
    public static readonly IconInfo CheckboxFillIcon = new("\uE73B");
    public static readonly IconInfo ToggleFilledIcon = new("\uEC11");
    public static readonly IconInfo StatusCircleIcon = new("\uEA81");
    public static readonly IconInfo CircleFillBadge12Icon = new("\uEDB0");

    private VirtualDesktop[] _desktops;

    public VirtualDesktopsListPage()
    {
        _scheduler = TaskScheduler.Current;

        VirtualDesktop.CurrentChanged += (_, args) => UpdateDesktopsOffUiThread();
        VirtualDesktop.Created += (_, desktop) => UpdateDesktopsOffUiThread();

        _desktops = VirtualDesktop.GetDesktops();

    }

    public override IListItem[] GetItems()
    {
        VirtualDesktop[] desktops = [];



        //Logger.LogDebug("VirtualDesktopsListPage.GetItems");
        //VirtualDesktop[] desktops = VirtualDesktop.GetDesktops();
        List<IListItem> items = new(_desktops.Length);
        DebugPrint($"Current desktop is {VirtualDesktop.Current}");
        foreach (VirtualDesktop desktop in _desktops)
        {
            items.Add(DesktopToItem(desktop));

        }
        DebugPrint(string.Join(',', items.Select(i => i.Command.ToString())));
        return items.ToArray();
    }

    private void UpdateDesktopsOffUiThread()
    {
        //Task.Factory.StartNew(UpdateDesktopsOnUiThread, )
        //Task.Run(UpdateDesktopsOnUiThread, _scheduler);
        //_queue.TryEnqueue(UpdateDesktopsOnUiThread);
        Task.Factory.StartNew(UpdateDesktopsOnUiThread,
            CancellationToken.None,
            TaskCreationOptions.None,
            _scheduler);
    }

    private void UpdateDesktopsOnUiThread()
    {
        _desktops = VirtualDesktop.GetDesktops();
        RaiseItemsChanged();
    }

    private static IListItem DesktopToItem(VirtualDesktop desktop)
    {
        bool isCurrent = desktop == VirtualDesktop.Current;
        if (isCurrent)
        {
            DebugPrint($"    * I ({desktop.ToString()}) am current");
        }
        else
        {
            DebugPrint($"    - I am NOT current");
        }

        return new ListItem(new SwitchToDesktopCommand(desktop, isCurrent))
        {
            // Icon = isCurrent ? CheckboxFillIcon : CheckboxEmptyIcon
            Icon = isCurrent ? ToggleFilledIcon : CircleFillBadge12Icon
            //Icon = isCurrent ? new(desktop.WallpaperPath) : CircleFillBadge12Icon
        };
    }

    private sealed partial class SwitchToDesktopCommand(VirtualDesktop desktop, bool isCurrent) : InvokableCommand
    {
        public VirtualDesktop Desktop { get; } = desktop;
        public override string Name => string.Empty;
        internal bool IsCurrent { get; init; } = isCurrent;
        public override string ToString()
        {
            return $"{(IsCurrent ? "*" : string.Empty)}{Desktop.ToString()}";
        }
        public override ICommandResult Invoke()
        {
            try
            {
                DebugPrint($"Switching to '{Desktop.ToString()}'");
                desktop.Switch();
                DebugPrint($"...done");
            }
            catch (Exception e)
            {
                DebugPrint($"SwitchToDesktopCommand invoke\n{e.Message}\n{e.StackTrace}");
            }
            return CommandResult.KeepOpen();
        }
    }

    private static void DebugPrint(string? s)
    {
        Debug.WriteLine(s);
    }
}

