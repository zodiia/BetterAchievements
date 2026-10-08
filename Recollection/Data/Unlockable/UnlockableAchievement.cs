using MethodTimer;
using System;
using System.Collections.Generic;
using Recollection.Helpers;
using Lumina.Excel.Sheets;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableAchievement : IUnlockable {
    private readonly Achievement achievement;

    [Time]
    public UnlockableAchievement(Achievement achievement, Plugin plugin) {
        this.achievement = achievement;
        Name = achievement.Name.ToString();
        Description = achievement.Description.ToString();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        Current = plugin.AchievementProgressService.GetProgress(achievement.RowId);
        Unlocked = Plugin.UnlockState.IsAchievementComplete(achievement);
        Pinned = plugin.Configuration.PinnedAchievements.Contains(achievement.RowId);
    }

    public uint Id => achievement.RowId;
    public UnlockableType Type => UnlockableType.Achievement;
    public uint Icon => achievement.Icon;

    public string Name { get; }
    public string Description { get; }
    public string NameLowercase { get; }
    public string DescriptionLowercase { get; }
    public string? HowTo => null;
    public string? HowToLowercase => null;

    public uint? Current { get; set; }
    public uint Maximum => achievement.Maximum();
    public bool Unlocked { get; set; }

    public bool Pinned { get; }
    public Title? Title => achievement.Title.ValueNullable;
    public byte Points => achievement.Points;
    public byte AchievementType => achievement.Type;
    public AchievementKind Category => achievement.AchievementCategory.Value.AchievementKind.Value;
    public AchievementCategory SubCategory => achievement.AchievementCategory.Value;

    public bool IsValid() => achievement.IsValidEntry();

    public AchievementCompletionRatio AchievementCompletionRatio() {
        return new AchievementCompletionRatio(this, Math.Clamp((Current ?? 0.0) / Maximum, 0.0, 1.0));
    }
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
