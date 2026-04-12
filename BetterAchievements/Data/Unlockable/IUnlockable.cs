namespace BetterAchievements.Data.Unlockable;

public enum UnlockableType
{
    Achievement,
    Mount,
    Minion,
}

public interface IUnlockable
{
    uint Id();
    UnlockableType Type();
    string Name();
    string Description();
    string NameLowercase();
    string DescriptionLowercase();
    uint? Current();
    uint Maximum();
    bool Unlocked();
}
