using BetterAchievements.Helpers;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableCraftingLog : IUnlockable {
    private readonly Recipe recipe;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string description;
    private readonly string descriptionLowercase;
    private readonly bool unlocked;

    public UnlockableCraftingLog(Recipe recipe) {
        this.recipe = recipe;
        name = recipe.ItemResult.Value.Name.ToString();
        description = recipe.ItemResult.Value.Description.ToString();
        nameLowercase = recipe.ItemResult.Value.Name.ToString().ToLower();
        descriptionLowercase = recipe.ItemResult.Value.Description.ToString().ToLower();
        unlocked = Plugin.QuestManager.Valid && Plugin.QuestManager.Value.IsRecipeCompleted(recipe.RowId);
    }

    public uint Id() => recipe.RowId;
    public UnlockableType Type() => UnlockableType.CraftingLog;
    public uint Icon() => recipe.ItemResult.Value.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => null;
    public string? HowToLowercase() => null;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => true; // TODO
}
