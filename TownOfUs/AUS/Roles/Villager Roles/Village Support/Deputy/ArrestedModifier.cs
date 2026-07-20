namespace AmongUsSalem.Modifiers;

public sealed class ArrestedModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "ArrestedModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
}