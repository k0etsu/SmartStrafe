using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;
using SmartStrafe.Windows;
using System;
using System.Runtime.InteropServices;

namespace SmartStrafe;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInterop { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/smartstrafe";
    private const string CheckStrafeKeybindSig = "E8 ?? ?? ?? ?? 84 C0 74 04 41 C6 06 01 BA 44 01 00 00";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SmartStrafe");
    private ConfigWindow ConfigWindow { get; init; }

    private enum Keybind : int
    {
        MoveForward = 321,
        MoveBack = 322,
        TurnLeft = 323,
        TurnRight = 324,
        StrafeLeft = 325,
        StrafeRight = 326,
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool CheckStrafeKeybindDelegate(IntPtr ptr, Keybind keybind);

    private Hook<CheckStrafeKeybindDelegate>? _hook;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open Smart Strafe configuration"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;

        try
        {
            var addr = SigScanner.ScanText(CheckStrafeKeybindSig);
            _hook = GameInterop.HookFromAddress<CheckStrafeKeybindDelegate>(addr, CheckStrafeKeybind);
            _hook.Enable();
            Log.Information("SmartStrafe hook enabled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to find CheckStrafeKeybind signature — plugin will not function.");
        }
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        _hook?.Dispose();
    }

    private void OnCommand(string command, string args) => ToggleConfigUi();

    public void ToggleConfigUi() => ConfigWindow.Toggle();

    private bool CheckStrafeKeybind(IntPtr ptr, Keybind keybind)
    {
        if (keybind is Keybind.StrafeLeft or Keybind.StrafeRight && !ClientState.IsGPosing)
        {
            // Both turn+strafe held on either side: honour manual quick-turn / backpedal regardless of mode
            if (Configuration.ManualBackpedal &&
                (_hook!.Original(ptr, Keybind.TurnLeft)  || _hook.Original(ptr, Keybind.StrafeLeft)) &&
                (_hook!.Original(ptr, Keybind.TurnRight) || _hook.Original(ptr, Keybind.StrafeRight)))
            {
                return true;
            }

            var mode = Condition[ConditionFlag.InCombat]
                ? Configuration.InCombat
                : Configuration.OutOfCombat;

            switch (mode)
            {
                case Mode.Turning:
                    return false;

                case Mode.StrafingNoBackpedal:
                    if (_hook!.Original(ptr, Keybind.MoveBack))
                        return false;
                    goto case Mode.Strafing;

                case Mode.Strafing:
                    // StrafeLeft(325)-2 = TurnLeft(323), StrafeRight(326)-2 = TurnRight(324)
                    return _hook!.Original(ptr, (Keybind)((int)keybind - 2)) || _hook.Original(ptr, keybind);
            }
        }

        return _hook!.Original(ptr, keybind);
    }
}
