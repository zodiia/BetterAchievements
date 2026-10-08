using MethodTimer;
using System;
using Recollection.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableCraftingLog : IUnlockable {
    private readonly Recipe recipe;

    [Time]
    public UnlockableCraftingLog(Recipe recipe) {
        this.recipe = recipe;
        Name = recipe.ItemResult.Value.Name.ToString();
        Description = recipe.ItemResult.Value.Description.ToString();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        Unlocked = Plugin.QuestManager.Valid && QuestManager.IsRecipeComplete(recipe.RowId);
    }

    public uint Id => recipe.RowId;
    public UnlockableType Type => UnlockableType.CraftingLog;
    public uint Icon => recipe.ItemResult.Value.Icon;

    public string Name { get; }
    public string? Description { get; }
    public string? HowTo => null;
    public string NameLowercase { get; }
    public string? DescriptionLowercase { get; }
    public string? HowToLowercase => null;

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => recipe.IsValidEntry();
}
