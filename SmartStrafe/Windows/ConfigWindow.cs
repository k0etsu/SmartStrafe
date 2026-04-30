using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SmartStrafe.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Smart Strafe###SmartStrafeConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(340, 130);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var inCombat = configuration.InCombat;
        var changed1 = DrawModeSelector("In combat", ref inCombat);
        if (changed1) configuration.InCombat = inCombat;

        var outOfCombat = configuration.OutOfCombat;
        var changed2 = DrawModeSelector("Out of combat", ref outOfCombat);
        if (changed2) configuration.OutOfCombat = outOfCombat;

        var backpedal = configuration.ManualBackpedal;
        var changed3 = ImGui.Checkbox("Enable manual quick turning and backpedaling", ref backpedal);
        if (changed3) configuration.ManualBackpedal = backpedal;
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Activates strafing when both left and right are held");

        if (changed1 || changed2 || changed3)
            configuration.Save();
    }

    private static bool DrawModeSelector(string label, ref Mode selectedMode)
    {
        var changed = false;
        ImGui.SetNextItemWidth(220);
        if (ImGui.BeginCombo(label, GetModeLabel(selectedMode)))
        {
            foreach (var mode in Enum.GetValues<Mode>())
            {
                if (ImGui.Selectable(GetModeLabel(mode), mode == selectedMode))
                {
                    selectedMode = mode;
                    changed = true;
                }
            }
            ImGui.EndCombo();
        }
        return changed;
    }

    private static string GetModeLabel(Mode mode) => mode switch
    {
        Mode.StrafingNoBackpedal => "Strafing (no backpedaling)",
        _ => mode.ToString(),
    };
}
