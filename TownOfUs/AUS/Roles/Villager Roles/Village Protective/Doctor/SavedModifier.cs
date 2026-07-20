namespace AmongUsSalem.Modifiers;

public sealed class SavedModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "SavedModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;

    public int PerformInteraction(PlayerControl visitor, PlayerControl target, DeathReasonShow dr = DeathReasonShow.None)
    {
        Caster.RpcAddModifier<Confirmed>();
        target.RpcAddModifier<Confirmed>();

        RpcPerformInteraction(visitor, target, dr);
        target.RpcRemoveModifier<SavedModifier>();
        return 1;
    }

    [MethodRpc((uint)AUSRpc.RpcPerformInteraction_Doctor)]
    public static void RpcPerformInteraction(PlayerControl player, PlayerControl target, DeathReasonShow dr)
    { 
        PlayerControl.LocalPlayer.Notify(Doctor.Info(target, player.GetDeathReason(dr)), NotifyMode.OnlyMeeting);
    }
}