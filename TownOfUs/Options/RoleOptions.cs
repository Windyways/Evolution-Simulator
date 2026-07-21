
namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    public static readonly string[] OptionStrings =
    [
        "<color=#ffffff>Random</color> <color=#b3ffff>Crewmate</color>",
        "<color=#ffffff>Random</color> <color=#a9a9a9>Neutral</color>",
        "<color=#ffffff>Random</color> <color=#ff0000>Impostor</color>",

        "Any",
        "Not <color=#ff0000>Impostor</color>"
    ];

    public override string GroupName => "Role";
    public override uint GroupPriority => 2;

    public ModdedEnumOption Slot1 { get; } =
        new("Slot 1", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot2 { get; } =
        new("Slot 2", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot3 { get; } =
        new("Slot 3", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot4 { get; } =
        new("Slot 4", (int)RoleListOption.RandomImpostor, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot5 { get; } =
        new("Slot 5", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot6 { get; } =
        new("Slot 6", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot7 { get; } =
        new("Slot 7", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot8 { get; } =
        new("Slot 8", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot9 { get; } =
        new("Slot 9", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot10 { get; } =
        new("Slot 10", (int)RoleListOption.RandomImpostor, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot11 { get; } =
        new("Slot 11", (int)RoleListOption.RandomImpostor, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot12 { get; } =
        new("Slot 12", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot13 { get; } =
        new("Slot 13", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot14 { get; } =
        new("Slot 14", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot15 { get; } =
        new("Slot 15", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };
}

public enum RoleListOption
{
    RandomCrewmate,
    RandomNeutral,
    RandomImpostor,

    Any,

    NotImpostor,

    CrewmateInvestigative,
    CrewmateKilling,
    CrewmateProtective,
    CrewmateSupport,
    CrewmateUtility,

    NeutralEvil,

    ImpostorDeception,
    ImpostorKilling,
    ImpostorSupport,
    ImpostorUtility,
    None
}