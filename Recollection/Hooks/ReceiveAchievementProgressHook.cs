using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

#pragma warning disable CS0649 // for the hook

namespace Recollection.Hooks;

public sealed unsafe class ReceiveAchievementProgressHook : IDisposable {
    private readonly IPluginLog log = Plugin.GetLogger<ReceiveAchievementProgressHook>();

    public delegate void ReceiveAchievementProgressDelegate(Achievement* achievement, uint id, uint current, uint max);

    public event ReceiveAchievementProgressDelegate? OnDetour;

    [Signature("C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 89 91 ?? ?? ?? ?? 44 89 81", DetourName = nameof(ReceiveAchievementProgressDetour))]
    private readonly Hook<ReceiveAchievementProgressDelegate>? hook;

    public ReceiveAchievementProgressHook() {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        hook?.Enable();
    }

    private void ReceiveAchievementProgressDetour(Achievement* achievement, uint id, uint current, uint max) {
        try {
            OnDetour?.Invoke(achievement, id, current, max);
        } catch (Exception ex) {
            log.Error(ex, "Exception caught in ReceiveAchievementProgress hook");
        }
        hook?.Original(achievement, id, current, max);
    }

    public void Dispose() {
        hook?.Disable();
        hook?.Dispose();
    }
}
