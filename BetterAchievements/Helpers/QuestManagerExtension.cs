using FFXIVClientStructs.FFXIV.Client.Game;

namespace BetterAchievements.Helpers;

public static class QuestManagerExtension {
    public static unsafe QuestManager? QuestManagerInstanceOrNull() {
        return Pointers.GetOrNull(QuestManager.Instance());
    }

    extension(QuestManager manager) {
        public bool IsRecipeCompletable(uint recipeRowId) => recipeRowId < manager.CompletedRecipesBitArray.BitCount;
        public bool IsRecipeCompleted(uint recipeRowId) => manager.IsRecipeCompletable(recipeRowId) && manager.CompletedRecipesBitArray[(int)recipeRowId];
    }
}
