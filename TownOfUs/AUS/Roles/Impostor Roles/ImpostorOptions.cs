namespace AmongUsSalem.ImpostorRoles;

public sealed class ImpostorOptions : AbstractOptionGroup
{
    public override string GroupName => "Impostor Settings";
    public override uint GroupPriority => 1;

    [ModdedNumberOption("Max Impostor Roles Per Game", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float MaxImpostor { get; set; } = 2;

    [ModdedNumberOption("Impostor Kill Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedToggleOption("Impostor Roles Can Vent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("Impostor Roles Can Sabotage")]
    public bool CanSabotage { get; set; } = true;
}