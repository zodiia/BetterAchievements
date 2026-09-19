using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

#pragma warning disable CS0649 // for the hook

namespace Recollection.Hooks;

public sealed unsafe class SetModeHook : IDisposable {
    private readonly IPluginLog log = Plugin.GetLogger<SetModeHook>();

    public delegate void SetModeDelegate(Character* chara, CharacterModes mode, byte modeParam);

    public event SetModeDelegate? OnDetour;

    [Signature("E8 ?? ?? ?? ?? 45 84 FF 75 40", DetourName = nameof(SetModeDetour))]
    private readonly Hook<SetModeDelegate>? hook;

    public SetModeHook() {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        hook?.Enable();
    }

    private void SetModeDetour(Character* chara, CharacterModes mode, byte modeParam) {
        try {
            OnDetour?.Invoke(chara, mode, modeParam);
        } catch (Exception ex) {
            log.Error(ex, "Exception caught in SetMode hook");
        }
        hook?.Original(chara, mode, modeParam);
    }

    public void Dispose() {
        hook?.Disable();
        hook?.Dispose();
    }
}
