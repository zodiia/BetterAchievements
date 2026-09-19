using System;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Recollection.Helpers;

public static class ExcelSheets {
    public static readonly Lazy<ExcelSheet<Achievement>> Achievement = new(() => Plugin.DataManager.GetExcelSheet<Achievement>());
    public static readonly Lazy<ExcelSheet<BuddyEquip>> BuddyEquip = new(() => Plugin.DataManager.GetExcelSheet<BuddyEquip>());
    public static readonly Lazy<ExcelSheet<CharaMakeCustomize>> CharaMakeCustomize = new(() => Plugin.DataManager.GetExcelSheet<CharaMakeCustomize>());
    public static readonly Lazy<ExcelSheet<Companion>> Companion = new(() => Plugin.DataManager.GetExcelSheet<Companion>());
    public static readonly Lazy<ExcelSheet<CompanionTransient>> CompanionTransient = new(() => Plugin.DataManager.GetExcelSheet<CompanionTransient>());
    public static readonly Lazy<ExcelSheet<Emote>> Emote = new(() => Plugin.DataManager.GetExcelSheet<Emote>());
    public static readonly Lazy<ExcelSheet<ENpcBase>> ENpcBase = new(() => Plugin.DataManager.GetExcelSheet<ENpcBase>());
    public static readonly Lazy<ExcelSheet<ENpcResident>> ENpcResident = new(() => Plugin.DataManager.GetExcelSheet<ENpcResident>());
    public static readonly Lazy<ExcelSheet<ExportedGatheringPoint>> ExportedGatheringPoint = new(() => Plugin.DataManager.GetExcelSheet<ExportedGatheringPoint>());
    public static readonly Lazy<ExcelSheet<Fate>> Fate = new(() => Plugin.DataManager.GetExcelSheet<Fate>());
    public static readonly Lazy<ExcelSheet<GatheringItem>> GatheringItem = new(() => Plugin.DataManager.GetExcelSheet<GatheringItem>());
    public static readonly Lazy<ExcelSheet<GatheringPoint>> GatheringPoint = new(() => Plugin.DataManager.GetExcelSheet<GatheringPoint>());
    public static readonly Lazy<ExcelSheet<GlassesStyle>> GlassesStyle = new(() => Plugin.DataManager.GetExcelSheet<GlassesStyle>());
    public static readonly Lazy<ExcelSheet<Mount>> Mount = new(() => Plugin.DataManager.GetExcelSheet<Mount>());
    public static readonly Lazy<ExcelSheet<MountTransient>> MountTransient = new(() => Plugin.DataManager.GetExcelSheet<MountTransient>());
    public static readonly Lazy<ExcelSheet<Level>> Level = new(() => Plugin.DataManager.GetExcelSheet<Level>());
    public static readonly Lazy<ExcelSheet<Orchestrion>> Orchestrion = new(() => Plugin.DataManager.GetExcelSheet<Orchestrion>());
    public static readonly Lazy<ExcelSheet<OrchestrionUiparam>> OrchestrionUiparam = new(() => Plugin.DataManager.GetExcelSheet<OrchestrionUiparam>());
    public static readonly Lazy<ExcelSheet<Ornament>> Ornament = new(() => Plugin.DataManager.GetExcelSheet<Ornament>());
    public static readonly Lazy<ExcelSheet<OrnamentTransient>> OrnamentTransient = new(() => Plugin.DataManager.GetExcelSheet<OrnamentTransient>());
    public static readonly Lazy<ExcelSheet<Recipe>> Recipe = new(() => Plugin.DataManager.GetExcelSheet<Recipe>());
    public static readonly Lazy<ExcelSheet<Title>> Title = new(() => Plugin.DataManager.GetExcelSheet<Title>());
    public static readonly Lazy<ExcelSheet<TripleTriad>> TripleTriad = new(() => Plugin.DataManager.GetExcelSheet<TripleTriad>());
    public static readonly Lazy<ExcelSheet<TripleTriadCard>> TripleTriadCard = new(() => Plugin.DataManager.GetExcelSheet<TripleTriadCard>());
    public static readonly Lazy<ExcelSheet<TripleTriadResident>> TripleTriadResident = new(() => Plugin.DataManager.GetExcelSheet<TripleTriadResident>());
}
