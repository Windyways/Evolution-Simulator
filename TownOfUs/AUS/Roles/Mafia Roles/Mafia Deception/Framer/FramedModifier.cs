namespace AmongUsSalem.Modifiers;

public sealed class FramedModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "FramedModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
}