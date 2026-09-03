using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableAchievement(Achievement Achievement, Plugin Plugin) : IUnlockable {
    public uint Id() => Achievement.RowId;
    public UnlockableType Type() => UnlockableType.Achievement;
    public string Name() => Achievement.Name.ToString();
    public string Description() => Achievement.Description.ToString();
    public AchievementCategory SubCategory() => Achievement.AchievementCategory.Value;
    public AchievementKind Category() => Achievement.AchievementCategory.Value.AchievementKind.Value;
    public uint Icon() => Achievement.Icon;
    public byte Points() => Achievement.Points;
    public byte AchievementType() => Achievement.Type;
    public uint Maximum() => Achievement.Maximum();

    private readonly string nameLowercase = Achievement.Name.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = Achievement.Description.ToString().ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly uint? current = Plugin.AchievementProgressService.GetProgress(Achievement.RowId);
    public uint? Current() => current;
    private readonly bool unlocked = Plugin.UnlockState.IsAchievementComplete(Achievement);
    public bool Unlocked() => unlocked;
    private readonly bool pinned = Plugin.Configuration.PinnedAchievements.Contains(Achievement.RowId);
    public bool Pinned() => pinned;
}

public static class AchievementExtensions {
    extension(Achievement achievement) {
        public uint Maximum() {
            return achievement.Type switch {
                1 or 3 or 11 or 18 or 21 or 25 => achievement.Data[0].RowId,
                10 or 12 or 13 or 17 or 19 => achievement.Key.RowId,
                _ => 1,
            };
        }

        /**
         * This is only valid when Type() == 2, when this is a compounded achievement.
         */
        public List<uint>? CompoundedAchievementIds() {
            if (achievement.Type != 2) {
                return null;
            }

            List<uint> ids = new();

            if (achievement.Key.RowId > 0) {
                ids.Add(achievement.Key.RowId);
            }

            foreach (var elem in achievement.Data) {
                if (elem.RowId > 0) {
                    ids.Add(elem.RowId);
                }
            }

            return ids;
        }

        /**
         * This is only valid when Type() == 15, when this is a beast tribe achievement.
         */
        public BeastTribe? BeastTribe() {
            if (achievement.Type != 15) {
                return null;
            }

            return achievement.Key.GetValueOrDefault<BeastTribe>();
        }

        /**
         * This is only valid when Type() == 15, when this is a beast tribe achievement.
         */
        public BeastReputationRank? BeastReputationRank() {
            if (achievement.Type != 15) {
                return null;
            }

            return achievement.Data[0].GetValueOrDefault<BeastReputationRank>();
        }

        /**
         * This is only valid when Type() == 11, when this is a PvP rank achievement.
         */
        public GrandCompany? GrandCompany() {
            if (achievement.Type != 11) {
                return null;
            }

            return achievement.Data[0].GetValueOrDefault<GrandCompany>();
        }

        /**
         * This is only valid when Type() == 20, when this is an aether current achievement.
         */
        public AetherCurrentCompFlgSet? AetherCurrentCompFlgSet() {
            if (achievement.Type != 20) {
                return null;
            }

            return achievement.Data[0].GetValueOrDefault<AetherCurrentCompFlgSet>();
        }

        /**
         * This is only valid when Type() == 24, when this is a relic weapon achievement.
         */
        public ClassJob? RelicClassJob() {
            if (achievement.Type != 24) {
                return null;
            }

            return achievement.Data[0].GetValueOrDefault<ClassJob>();
        }

        /**
         * This is only valid when Type() == 29, when this is a triple triad "get all cards until x" achievement.
         */
        public uint? TripleTriadCardSet() {
            if (achievement.Type != 29) {
                return null;
            }

            return achievement.Data[0].RowId;
        }
    }
}
