namespace AmongUsSalem.Modifiers;

public sealed class ThreatenedModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "ThreatenedModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;

    public override void OnMeetingStart()
    {
        if (Player.AmOwner()) Player.Notify(Henchman.Info(), NotifyMode.InstantlyAndMeeting);
    }
}