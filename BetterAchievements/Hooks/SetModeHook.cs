using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

#pragma warning disable CS0649 // for the hook

namespace BetterAchievements.Hooks;

public sealed unsafe class SetModeHook : IDisposable
{
    public delegate void SetModeDelegate(Character* chara, CharacterModes mode, byte modeParam);
    public event SetModeDelegate? OnDetour;

    [Signature("E8 ?? ?? ?? ?? 45 84 FF 75 40", DetourName = nameof(SetModeDetour))]
    private readonly Hook<SetModeDelegate>? hook;

    public SetModeHook()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        hook?.Enable();
    }

    private void SetModeDetour(Character* chara, CharacterModes mode, byte modeParam)
    {
        hook?.Original(chara, mode, modeParam);
        OnDetour?.Invoke(chara, mode, modeParam);
    }

    public void Dispose()
    {
        hook?.Disable();
        hook?.Dispose();
    }
}
