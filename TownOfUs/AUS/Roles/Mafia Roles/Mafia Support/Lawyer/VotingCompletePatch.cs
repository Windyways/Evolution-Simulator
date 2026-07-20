namespace AmongUsSalem.Modifiers;

public sealed class IntervenedModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "IntervenedModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
}