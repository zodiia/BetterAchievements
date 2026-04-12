using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableAchievement(Achievement Achievement) : IUnlockable
{
    public uint Id() => Achievement.RowId;
    public UnlockableType Type() => UnlockableType.Achievement;
    public string Name() => Achievement.Name.ToString();
    public string Description() => Achievement.Description.ToString();
    public AchievementCategory SubCategory() => Achievement.AchievementCategory.Value;
    public AchievementKind Category() => Achievement.AchievementCategory.Value.AchievementKind.Value;
    public uint Icon() => Achievement.Icon;
    public byte Points() => Achievement.Points;
    public byte AchievementType() => Achievement.Type;

    private readonly string nameLowercase = Achievement.Name.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = Achievement.Description.ToString().ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly uint? current = Plugin.UnlockablesProgressService.GetProgress(Achievement.RowId);
    public uint? Current() => current;
    private readonly bool unlocked = Plugin.UnlockState.IsAchievementComplete(Achievement);
    public bool Unlocked() => unlocked;

    public uint Maximum()
    {
        switch (Achievement.Type)
        {
            case 1: case 3: case 11: case 18: case 21: case 25:
                return Achievement.Data[0].RowId;
            case 10: case 12: case 13: case 17: case 19:
                return Achievement.Key.RowId;
            default:
                return 1;
        }
    }

    /**
     * This is only valid when Type() == 2, when this is a compounded achievement.
     */
    public List<uint>? CompoundedAchievementIds()
    {
        if (AchievementType() != 2)
        {
            return null;
        }

        List<uint> ids = new();

        if (Achievement.Key.RowId > 0)
        {
            ids.Add(Achievement.Key.RowId);
        }

        foreach (var elem in Achievement.Data)
        {
            if (elem.RowId > 0)
            {
                ids.Add(elem.RowId);
            }
        }
        return ids;
    }

    /**
     * This is only valid when Type() == 15, when this is a beast tribe achievement.
     */
    public BeastTribe? BeastTribe()
    {
        if (AchievementType() != 15)
        {
            return null;
        }

        return Achievement.Key.GetValueOrDefault<BeastTribe>();
    }

    /**
     * This is only valid when Type() == 15, when this is a beast tribe achievement.
     */
    public BeastReputationRank? BeastReputationRank()
    {
        if (AchievementType() != 15)
        {
            return null;
        }

        return Achievement.Data[0].GetValueOrDefault<BeastReputationRank>();
    }

    /**
     * This is only valid when Type() == 11, when this is a PvP rank achievement.
     */
    public GrandCompany? GrandCompany()
    {
        if (AchievementType() != 11)
        {
            return null;
        }

        return Achievement.Data[0].GetValueOrDefault<GrandCompany>();
    }

    /**
     * This is only valid when Type() == 20, when this is an aether current achievement.
     */
    public AetherCurrentCompFlgSet? AetherCurrentCompFlgSet()
    {
        if (AchievementType() != 20)
        {
            return null;
        }

        return Achievement.Data[0].GetValueOrDefault<AetherCurrentCompFlgSet>();
    }

    /**
     * This is only valid when Type() == 24, when this is a relic weapon achievement.
     */
    public ClassJob? RelicClassJob()
    {
        if (AchievementType() != 24)
        {
            return null;
        }

        return Achievement.Data[0].GetValueOrDefault<ClassJob>();
    }

    /**
     * This is only valid when Type() == 29, when this is a triple triad "get all cards until x" achievement.
     */
    public uint? TripleTriadCardSet()
    {
        if (AchievementType() != 29)
        {
            return null;
        }

        return Achievement.Data[0].RowId;
    }
}
