
namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    public static readonly string[] OptionStrings =
    [
        "<color=#3f7095>Village</color> <color=#ffffff>Investigative</color>",
        "<color=#3f7095>Village</color> <color=#ffffff>Killing</color>",
        "<color=#3f7095>Village</color> <color=#ffffff>Protective</color>",
        "<color=#3f7095>Village</color> <color=#ffffff>Support</color>",
        "<color=#3f7095>Village</color> <color=#ffffff>Utility</color>",
        "<color=#ffffff>Random</color> <color=#3f7095>Village</color>",

        "<color=#3f7095>Independent </color> <color=#ffffff>Evil</color>",
        "<color=#ffffff>Random</color> <color=#3f7095>Independent</color>",

        "<color=#903e3f>Mafia</color> <color=#ffffff>Deception</color>",
        "<color=#903e3f>Mafia</color> <color=#ffffff>Killing</color>",
        "<color=#903e3f>Mafia</color> <color=#ffffff>Support</color>",
        "<color=#903e3f>Mafia</color> <color=#ffffff>Utility</color>",
        "<color=#ffffff>Random</color> <color=#903e3f>Mafia</color>",

        "Any",
        "Not <color=#903e3f>Mafia</color>"
    ];

    public override string GroupName => "Role";
    public override uint GroupPriority => 2;

    public ModdedEnumOption Slot1 { get; } =
        new("Slot 1", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot2 { get; } =
        new("Slot 2", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot3 { get; } =
        new("Slot 3", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot4 { get; } =
        new("Slot 4", (int)RoleListOption.RandomMafia, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot5 { get; } =
        new("Slot 5", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot6 { get; } =
        new("Slot 6", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot7 { get; } =
        new("Slot 7", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot8 { get; } =
        new("Slot 8", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot9 { get; } =
        new("Slot 9", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot10 { get; } =
        new("Slot 10", (int)RoleListOption.RandomMafia, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot11 { get; } =
        new("Slot 11", (int)RoleListOption.RandomMafia, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot12 { get; } =
        new("Slot 12", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot13 { get; } =
        new("Slot 13", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot14 { get; } =
        new("Slot 14", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot15 { get; } =
        new("Slot 15", (int)RoleListOption.RandomVillage, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };
}

public enum RoleListOption
{
    VillageInvestigative,
    VillageKilling,
    VillageProtective,
    VillageSupport,
    VillageUtility,
    RandomVillage,

    IndependentEvil,
    RandomIndependent,

    MafiaDeception,
    MafiaKilling,
    MafiaSupport,
    MafiaUtility,
    RandomMafia,

    Any,

    NotMafia,
    None
}