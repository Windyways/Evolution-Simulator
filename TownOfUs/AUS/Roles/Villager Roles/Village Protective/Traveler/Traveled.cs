namespace AmongUsSalem.Modifiers;

public sealed class Traveled(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "Traveled";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
}