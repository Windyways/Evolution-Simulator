namespace AmongUsSalem.Options;

public sealed class MechanicOptions : AbstractOptionGroup
{
    public override string GroupName => "Mechanic Options";
    public override uint GroupPriority => 0;
    
    [ModdedToggleOption("Enable Admin Location Mechanic")]
    public bool AdminLocation { get; set; } = true;
}