namespace AmongUsSalem.Modifiers;

public sealed class CleanedUp(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "CleanedUp";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
}