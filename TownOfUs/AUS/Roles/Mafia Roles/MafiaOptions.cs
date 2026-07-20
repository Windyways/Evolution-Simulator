namespace AmongUsSalem.MafiaRoles;

public sealed class MafiaOptions : AbstractOptionGroup
{
    public override string GroupName => "Mafia Settings";
    public override uint GroupPriority => 1;

    [ModdedNumberOption("Max Mafia Roles Per Game", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float MaxMafia { get; set; } = 4;

    [ModdedNumberOption("Mafia Kill Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedToggleOption("Mafia Roles Can Vent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("Mafia Roles Can Sabotage")]
    public bool CanSabotage { get; set; } = true;
}