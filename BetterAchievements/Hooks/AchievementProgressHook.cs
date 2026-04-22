using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Serilog;

#pragma warning disable CS0649 // for the hook

namespace BetterAchievements.Hooks;

public sealed unsafe class AchievementProgressHook : IDisposable
{
    private readonly Plugin plugin;

    private delegate void ReceiveAchievementProgressDelegate(Achievement* achievement, uint id, uint current, uint max);
    
    [Signature("C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 89 91 ?? ?? ?? ?? 44 89 81", DetourName = nameof(ReceiveAchievementProgressDetour))]
    private readonly Hook<ReceiveAchievementProgressDelegate>? hook;

    public AchievementProgressHook(Plugin plugin)
    {
        this.plugin = plugin;
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        hook?.Enable();
    }

    private void ReceiveAchievementProgressDetour(Achievement* achievement, uint id, uint current, uint max)
    {
        hook?.Original(achievement, id, current, max);

        Log.Information("[AchievementProgressHook] Received achievement progress for id {Id} at {Current}/{Max}", id, current, max);

        plugin.UnlockablesProgressService.SetProgress(id, current);
    }

    public void Dispose()
    {
        hook?.Disable();
        hook?.Dispose();
    }
}
