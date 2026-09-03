using System;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.NativeWrapper;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using AtkValueType = FFXIVClientStructs.FFXIV.Component.GUI.AtkValueType;

namespace BetterAchievements.Services;

public class AddonLifecycleService {
    public static readonly IPluginLog Log = Plugin.GetLogger<AddonLifecycleService>();

    public delegate void FateCompletionDelegate(Fate fate, FateMedal medal);

    public event FateCompletionDelegate? OnFateCompleted;

    public AddonLifecycleService() {
        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FateReward", OnFateRewardSetup);
        Log.Information("Fate event setup");
    }

    private void OnFateRewardSetup(AddonEvent type, AddonArgs args) {
        if (args is not AddonSetupArgs setupArgs) return;

        var values = setupArgs.AtkValueEnumerable;
        var atkValuePtrs = values.ToList();
        var name = atkValuePtrs.GetStringAt(0);
        var medal = atkValuePtrs.GetUIntAt(3) switch {
            0 => FateMedal.Gold, 1 => FateMedal.Silver, 2 => FateMedal.Bronze, _ => FateMedal.Unknown
        };

        var fate = Plugin.DataManager.GetExcelSheet<Fate>().FirstOrNull(fate => fate.Name.ToString().Equals(name));
        if (!fate.HasValue) {
            Log.Warning("Fate {Name} could not be matched to an existing Fate line in the excel sheet", name);
            return;
        }

        Log.Debug("Fate {Name} completed with {Medal} medal, matched with #{Fate} in {Area}", name, medal, fate.Value.RowId, fate.Value.Location);
        OnFateCompleted?.Invoke(fate.Value, medal);
    }

    private unsafe void LogAtkValue(AtkValue* atkValue) {
        switch (atkValue->Type) {
            case AtkValueType.Int: Log.Debug("Int: {I}", atkValue->Int); break;
            case AtkValueType.UInt: Log.Debug("UInt: {U}", atkValue->UInt); break;
            case AtkValueType.String: Log.Debug("String: {S}", atkValue->String.ExtractText()); break;
            case AtkValueType.Undefined: Log.Debug("Undefined value"); break;
            default: Log.Debug("Unmapped type {T}", atkValue->Type); break;
        }
    }
}

public static class AddonLifecycleExtensions {
    public static unsafe uint GetUIntAt(this IEnumerable<AtkValuePtr> enumerable, int index) {
        var atkValue = (AtkValue*)enumerable.Skip(index).First().Address;
        return atkValue->Type != AtkValueType.UInt ? throw new ArgumentException("AtkValuePtr is of wrong type at index " + index) : atkValue->UInt;
    }

    public static unsafe string GetStringAt(this IEnumerable<AtkValuePtr> enumerable, int index) {
        var atkValue = (AtkValue*)enumerable.Skip(index).First().Address;
        return atkValue->Type != AtkValueType.String
                   ? throw new ArgumentException("AtkValuePtr is of wrong type at index " + index)
                   : atkValue->String.ToString();
    }
}
