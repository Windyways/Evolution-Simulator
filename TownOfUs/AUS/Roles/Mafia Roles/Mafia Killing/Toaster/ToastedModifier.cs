using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Modifiers;

public sealed class ToastedModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "ToastedModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
    public enum State { ApplyNight, KillDay }
    public State currentState = State.ApplyNight;
    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        if (currentState == State.ApplyNight)
        {
            currentState = State.KillDay;
            Player.Notify(Toaster.Info(), NotifyMode.InstantlyAndMeeting);
            Player.AddModifier<SoftCleared>();
        }
        else
        {
            if (Player.TryGetModifier<SavedModifier>(out var saved)) saved.PerformInteraction(Caster, Player, DeathReasonShow.KilledByTheToaster);
            else if (Player.Data.Role is Bulletproof bulletproof && bulletproof.hasVest) bulletproof.PerformInteraction(Caster, Player, DeathReasonShow.KilledByTheToaster);
            else
            {
                Caster.RpcCustomMurder(Player);
                VisitingMechanic.RpcAddDeathReason(Player, (int)Caster.GetDeathReason(DeathReasonShow.KilledByTheToaster));
            }

            Player.RpcRemoveModifier<ToastedModifier>();
        }
    }

    public int PerformInteraction()
    {
        if (currentState == State.ApplyNight) return 100;
        return 0;
    }
}