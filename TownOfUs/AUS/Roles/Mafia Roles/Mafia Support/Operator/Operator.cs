using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Operator(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Operator";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Mafia;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Mafia;
    public Alignment Alignment => Alignment.MafiaSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<MafiaOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<MafiaOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        // Icon = AUSAssets.SerialKillerRoleCard
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"While alive, the Mafia can communicate in private chat during the day. Sided with the Mafia.\n\n" +
            "Wins if Mafia outnumber the village. Sided with the Mafia." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return "- The Mafia can privately chat during the day.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill",
            "Kill a player at night to kill them.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }
}