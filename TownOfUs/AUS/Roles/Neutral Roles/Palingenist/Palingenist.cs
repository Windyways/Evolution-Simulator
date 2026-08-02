using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text;
using UnityEngine;
using static MeetingHud;

namespace AmongUsSalem.Roles;

public sealed class Palingenist(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Palingenist";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Palingenist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Neutral;
    public Alignment Alignment => Alignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = AUSAssets.Palingenist
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return 
            $"I can kill players.\n" +
            $"I gain Immunity against the ejection I receive.\n" +
            "I win alone if I am the last player alive." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public bool WinConditionMet() => NeutralGameOver.WinConditionMet(this);
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public void DoVisit(PlayerControl target, int Button, bool visiting, bool kill) => VisitingMechanic.CheckVisit(Player, target, Button, kill, visiting);
    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcCustomMurder(target, showKillAnim: false);
            VisitingMechanic.RpcAddDeathReason(target, DeathReasonShow.KilledByAPalingenist, RoleColors.Palingenist);
        }
    }

    /*public bool UsedSecondMove;
    public void OnTurnStart()
    {
        UsedSecondMove = false;

        var aliveImpostors = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.Is(Faction.Impostor));
        if (aliveImpostors > 0) UsedSecondMove = true;
    }*/

    public bool CanCounterattack = true;
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class VotingComplete
{
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (__instance.exiledPlayer != null)
        {
            var votedPlayer = MiscUtils.PlayerById(__instance.exiledPlayer.PlayerId);
            if (votedPlayer.GetRoleWhenAlive() is Palingenist palingenist && palingenist.CanCounterattack)
            {
                palingenist.Player.AddModifier<ConfirmedEvil>();
                palingenist.CanCounterattack = false;
                __instance.exiledPlayer = null;
            }
        }
    }
}