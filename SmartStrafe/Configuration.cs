using Dalamud.Configuration;
using System;

namespace SmartStrafe;

public enum Mode
{
    Turning,
    Strafing,
    StrafingNoBackpedal,
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public Mode InCombat { get; set; } = Mode.StrafingNoBackpedal;
    public Mode OutOfCombat { get; set; } = Mode.Turning;
    public bool ManualBackpedal { get; set; } = true;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
