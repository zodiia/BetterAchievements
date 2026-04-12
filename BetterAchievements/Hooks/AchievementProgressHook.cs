using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Serilog;

namespace BetterAchievements.Hooks;

public sealed unsafe class AchievementProgressHook : IDisposable
{
    private delegate void ReceiveAchievementProgressDelegate(Achievement* achievement, uint id, uint current, uint max);
    
    [Signature("C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 89 91 ?? ?? ?? ?? 44 89 81", DetourName = nameof(ReceiveAchievementProgressDetour))]
#pragma warning disable CS0649
    private readonly Hook<ReceiveAchievementProgressDelegate>? hook;
#pragma warning restore CS0649

    public AchievementProgressHook(IGameInteropProvider interopProvider)
    {
        interopProvider.InitializeFromAttributes(this);
        hook?.Enable();
    }

    private void ReceiveAchievementProgressDetour(Achievement* achievement, uint id, uint current, uint max)
    {
        hook?.Original(achievement, id, current, max);

        Log.Information("[AchievementProgressHook] Received achievement progress for id {Id} at {Current}/{Max}", id, current, max);

        Plugin.UnlockablesProgressService.SetProgress(id, current);
    }

    public void Dispose()
    {
        hook?.Disable();
        hook?.Dispose();
    }
}
