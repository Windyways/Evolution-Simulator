using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Tenebrist(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Tenebrist";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Impostor;
    public Alignment Alignment => Alignment.ImpostorDeception;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<ImpostorOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<ImpostorOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        // Icon = AUSAssets.Tenebrist
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
            "If I am out of range, I go invisible for this turn." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public bool WinConditionMet() => ImpostorGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || ImpostorGameOver.AnyWon(gameOverReason);
    }

    public void DoVisit(PlayerControl target, int Button, bool visiting, bool kill) => VisitingMechanic.CheckVisit(Player, target, Button, kill, visiting);
    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcCustomMurder(target, showKillAnim: false);
            VisitingMechanic.RpcAddDeathReason(target, DeathReasonShow.KilledByATenebrist, RoleColors.Impostor);
        }
        else if (Button == 2)
        {
            Player.AddModifier<InvisibleStatus>();
        }
    }
}