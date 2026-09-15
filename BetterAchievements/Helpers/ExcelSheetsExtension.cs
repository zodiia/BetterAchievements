using System;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace BetterAchievements.Helpers;

public static class ExcelSheetsExtension {
    private const string AchievementCategoryLegacy = "Legacy";
    private const uint CharaMakeCustomizeFacePaintIds = 2400;
    private const uint MaxRecordableRecipeId = 30000;
    private const uint MaxRecordableGatheringItemId = 10000;
    private const uint MaxGatheringTypeId = 4;
    private static readonly Type ItemType = typeof(Item);

    public static bool IsValidEntry(this Mount mount) => mount is { Order: > -1, Singular.IsEmpty: false };
    public static bool IsValidEntry(this Companion companion) => companion is { Singular.IsEmpty: false };
    public static bool IsValidEntry(this Title title) => title is { Feminine.IsEmpty: false };
    public static bool IsValidEntry(this TripleTriadCard card) => card is { Name.IsEmpty: false };
    public static bool IsValidEntry(this BuddyEquip equip) => equip is { Order: > 0, Name.IsEmpty: false };
    public static bool IsValidEntry(this Ornament ornament) => ornament is { Singular.IsEmpty: false };
    public static bool IsValidEntry(this Emote emote) => emote is { Order: > 0, Name.IsEmpty: false };
    public static bool IsValidEntry(this Recipe recipe) => recipe is { RowId: < MaxRecordableRecipeId, ItemResult: { IsValid: true, RowId: > 0 } };
    public static bool IsValidEntry(this GatheringItem item) => item is { RowId: < MaxRecordableGatheringItemId, RowId: > 0 } && item.Item.RowType == ItemType;

    public static bool IsValidEntry(this GatheringPoint point) => point is {
        GatheringPointBase: { IsValid: true, Value.GatheringType.RowId: < MaxGatheringTypeId }, PlaceName: { IsValid: true, Value.Name.IsEmpty: false }, TerritoryType.IsValid: true
    };

    public static bool IsValidEntry(this GlassesStyle facewear) => facewear is { Name.IsEmpty: false } && facewear.Glasses.FirstOrNull()?.IsValid == true &&
                                                                   !facewear.Glasses.FirstOrNull()?.Value.Name.IsEmpty == true;

    public static bool IsValidEntry(this Achievement achievement) => achievement is {
        Name.IsEmpty: false,
        AchievementCategory.IsValid: true,
        AchievementCategory.ValueNullable.AchievementKind.IsValid: true,
        AchievementCategory.ValueNullable.AchievementKind.ValueNullable.Name.IsEmpty: false,
    } && !achievement.AchievementCategory.Value.AchievementKind.Value.Name.ToString().Equals(AchievementCategoryLegacy);

    // 130 and 159 are both duplicate ids for fem variants of hairstyles 129 and 158.
    public static bool IsValidEntry(this CharaMakeCustomize hairstyle) =>
        hairstyle is { IsPurchasable: true, RowId: < CharaMakeCustomizeFacePaintIds, FeatureID: not (130 or 159) };
}
